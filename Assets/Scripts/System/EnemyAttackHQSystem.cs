using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct EnemyAttackHQSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BuildingHQ>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity hqEntity = SystemAPI.GetSingletonEntity<BuildingHQ>();
        float3 hqPosition = SystemAPI.GetComponent<LocalTransform>(hqEntity).Position;

        foreach ((RefRW<EnemyAttackHQ> EnemyAttackHQ, RefRW<TargetPositionPathQueued> targetPositionPathQueued, EnabledRefRW<TargetPositionPathQueued> targetPositionPathQueuedEnable, RefRO<Target> target)
            in SystemAPI.Query<RefRW<EnemyAttackHQ>, RefRW<TargetPositionPathQueued>, EnabledRefRW<TargetPositionPathQueued>, RefRO<Target>>().WithPresent<TargetPositionPathQueued>()) 
        {
            //如果没有要攻击的目标就攻击HQ
            if(target.ValueRO.m_TargetEntity != Entity.Null)
            {
                continue;
            }

            targetPositionPathQueued.ValueRW.m_TargetPosition = hqPosition;
            targetPositionPathQueuedEnable.ValueRW = true;

        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
