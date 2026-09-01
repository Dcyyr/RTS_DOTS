using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup),OrderFirst = true)]
partial struct ResetTargetSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (RefRW<Target> target in SystemAPI.Query<RefRW<Target>>())
        {
            if (target.ValueRW.m_TargetEntity != null)
            {
                //没有找到目标实体或者没有LocalTransform，重置目标实体为Entity.Null
                if (!SystemAPI.Exists(target.ValueRO.m_TargetEntity) || !SystemAPI.HasComponent<LocalTransform>(target.ValueRO.m_TargetEntity))
                {
                    target.ValueRW.m_TargetEntity = Entity.Null;
                }
            }
        }
    }

   
}
