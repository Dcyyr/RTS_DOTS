using UnityEngine;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct HealthBarSystem : ISystem
{
  

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        Vector3 cameraForword = Vector3.zero;
        if(Camera.main != null)
        {
            cameraForword = Camera.main.transform.forward;
        }

        foreach((RefRW<LocalTransform> localTransform, RefRO<HealthBar> healthBar) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<HealthBar>>())
        {

            LocalTransform parentLoaclTransform = SystemAPI.GetComponent<LocalTransform>(healthBar.ValueRO.m_HealthEntity);
            localTransform.ValueRW.Rotation = parentLoaclTransform.InverseTransformRotation(quaternion.LookRotation(cameraForword, 1));

            Health health = SystemAPI.GetComponent<Health>(healthBar.ValueRO.m_HealthEntity);

            if(!health.m_OnHealthChanged)
            {
                continue;
            }

            Debug.Log("Health Update");
            float healthPercentage = (float)health.m_Health / health.m_MaxHealth;

            RefRW<PostTransformMatrix> healthBarTransformMatrix = SystemAPI.GetComponentRW<PostTransformMatrix>(healthBar.ValueRO.m_HealthBarEntity);
            healthBarTransformMatrix.ValueRW.Value = float4x4.Scale(healthPercentage, 1, 1);
        }
    }

    
}
