using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup), OrderFirst = true)]
partial struct ResetTargetSystem : ISystem
{


    private ComponentLookup<LocalTransform> m_LocalTransformComponentLookup;
    private EntityStorageInfoLookup m_EntityStorageInfoLookup;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_LocalTransformComponentLookup = state.GetComponentLookup<LocalTransform>();
        m_EntityStorageInfoLookup = state.GetEntityStorageInfoLookup();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        m_LocalTransformComponentLookup.Update(ref state);
        m_EntityStorageInfoLookup.Update(ref state);

        ResetTargetJob resetTargetJob = new ResetTargetJob
        {
            localTransformComponentLookup = m_LocalTransformComponentLookup,
            entityStorageInfoLookup = m_EntityStorageInfoLookup,
        };
        resetTargetJob.ScheduleParallel();

        ResetTargetOverrideJob resetTargetOverrideJob = new ResetTargetOverrideJob
        {
            localTransformComponentLookup = m_LocalTransformComponentLookup,
            entityStorageInfoLookup = m_EntityStorageInfoLookup,
        };
        resetTargetOverrideJob.ScheduleParallel();

        //foreach (RefRW<Target> target in SystemAPI.Query<RefRW<Target>>())
        //{
        //    if (target.ValueRW.m_TargetEntity != Entity.Null)
        //    {
        //        //没有找到目标实体或者没有LocalTransform，重置目标实体为Entity.Null
        //        if (!SystemAPI.Exists(target.ValueRO.m_TargetEntity) || !SystemAPI.HasComponent<LocalTransform>(target.ValueRO.m_TargetEntity))
        //        {
        //            target.ValueRW.m_TargetEntity = Entity.Null;
        //        }
        //    }
        //}

        //foreach (RefRW<TargetOverride> targetOverride in SystemAPI.Query<RefRW<TargetOverride>>())
        //{
        //    if (targetOverride.ValueRW.m_TargetEntity != Entity.Null)
        //    {
        //        //没有找到目标实体或者没有LocalTransform，重置目标实体为Entity.Null
        //        if (!SystemAPI.Exists(targetOverride.ValueRO.m_TargetEntity) || !SystemAPI.HasComponent<LocalTransform>(targetOverride.ValueRO.m_TargetEntity))
        //        {
        //            targetOverride.ValueRW.m_TargetEntity = Entity.Null;
        //        }
        //    }
        //}
    }


}

[BurstCompile]
public partial struct ResetTargetJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<LocalTransform> localTransformComponentLookup;
    [ReadOnly] public EntityStorageInfoLookup entityStorageInfoLookup;
    public void Execute(ref Target target)
    {
        if (target.m_TargetEntity != Entity.Null)
        {
            //没有找到目标实体或者没有LocalTransform，重置目标实体为Entity.Null
            if (!entityStorageInfoLookup.Exists(target.m_TargetEntity) || !localTransformComponentLookup.HasComponent(target.m_TargetEntity))
            {
                target.m_TargetEntity = Entity.Null;
            }
        }
    }

}


[BurstCompile]
public partial struct ResetTargetOverrideJob : IJobEntity
{

    [ReadOnly] public ComponentLookup<LocalTransform> localTransformComponentLookup;
    [ReadOnly] public EntityStorageInfoLookup entityStorageInfoLookup;
    public void Execute(ref TargetOverride targetOverride)
    {
        if (targetOverride.m_TargetEntity != Entity.Null)
        {
            //没有找到目标实体或者没有LocalTransform，重置目标实体为Entity.Null
            if (!entityStorageInfoLookup.Exists(targetOverride.m_TargetEntity) || !localTransformComponentLookup.HasComponent(targetOverride.m_TargetEntity))
            {
                targetOverride.m_TargetEntity = Entity.Null;
            }
        }
    }

}

