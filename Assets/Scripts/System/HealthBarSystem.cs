using UnityEngine;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct HealthBarSystem : ISystem
{

    private ComponentLookup<LocalTransform> m_LocalTransformComponentLookup;
    private ComponentLookup<Health> m_HealthComponentLookup;
    private ComponentLookup<PostTransformMatrix> m_PostTransformMatrixComponentLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_LocalTransformComponentLookup = state.GetComponentLookup<LocalTransform>();
        m_HealthComponentLookup = state.GetComponentLookup<Health>();
        m_PostTransformMatrixComponentLookup = state.GetComponentLookup<PostTransformMatrix>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        Vector3 cameraForword = Vector3.zero;
        if(Camera.main != null)
        {
            cameraForword = Camera.main.transform.forward;
        }

        m_LocalTransformComponentLookup.Update(ref state);
        m_HealthComponentLookup.Update(ref state);
        m_PostTransformMatrixComponentLookup.Update(ref state);

        HealthBarJob healthBarJob = new HealthBarJob
        {

            localTransformComponentLookup = m_LocalTransformComponentLookup,
            healthComponentLookup = m_HealthComponentLookup,
            postTransformMatrixComponentLookup = m_PostTransformMatrixComponentLookup,
            cameraForword = cameraForword
        };

        healthBarJob.ScheduleParallel();

        //foreach((RefRW<LocalTransform> localTransform, RefRO<HealthBar> healthBar) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<HealthBar>>())
        //{

        //    LocalTransform parentLoaclTransform = SystemAPI.GetComponent<LocalTransform>(healthBar.ValueRO.m_HealthEntity);
        //    localTransform.ValueRW.Rotation = parentLoaclTransform.InverseTransformRotation(quaternion.LookRotation(cameraForword, 1));

        //    Health health = SystemAPI.GetComponent<Health>(healthBar.ValueRO.m_HealthEntity);

        //    if(!health.m_OnHealthChanged)
        //    {
        //        continue;
        //    }

        //    Debug.Log("Health Update");
        //    float healthPercentage = (float)health.m_Health / health.m_MaxHealth;

        //    RefRW<PostTransformMatrix> healthBarTransformMatrix = SystemAPI.GetComponentRW<PostTransformMatrix>(healthBar.ValueRO.m_HealthBarEntity);
        //    healthBarTransformMatrix.ValueRW.Value = float4x4.Scale(healthPercentage, 1, 1);
        //}
    }


}
public partial struct HealthBarJob : IJobEntity
{

    [NativeDisableParallelForRestriction] public ComponentLookup<LocalTransform> localTransformComponentLookup;
    [ReadOnly] public ComponentLookup<Health> healthComponentLookup;
    [NativeDisableParallelForRestriction] public ComponentLookup<PostTransformMatrix> postTransformMatrixComponentLookup;

    public float3 cameraForword;
    public void Execute(in HealthBar healthBar,Entity entity)
    {


        RefRW<LocalTransform> LocalTransform = localTransformComponentLookup.GetRefRW(entity);
        LocalTransform parentLoaclTransform = localTransformComponentLookup[healthBar.m_HealthEntity];

        LocalTransform.ValueRW.Rotation = parentLoaclTransform.InverseTransformRotation(quaternion.LookRotation(cameraForword, 1));

        Health health = healthComponentLookup[healthBar.m_HealthEntity];

        if (!health.m_OnHealthChanged)
        {
            return;
        }

        Debug.Log("Health Update");
        float healthPercentage = (float)health.m_Health / health.m_MaxHealth;

        RefRW<PostTransformMatrix> healthBarTransformMatrix = postTransformMatrixComponentLookup.GetRefRW(healthBar.m_HealthBarEntity);
        healthBarTransformMatrix.ValueRW.Value = float4x4.Scale(healthPercentage, 1, 1);
        
    }


}


