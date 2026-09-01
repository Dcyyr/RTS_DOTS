using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct ResetTargetSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach(RefRW<Target> target in SystemAPI.Query<RefRW<Target>>())
        {
            //没有找到目标实体，重置目标实体为Entity.Null
            if (!SystemAPI.Exists(target.ValueRO.m_TargetEntity))
            {
                target.ValueRW.m_TargetEntity = Entity.Null;
            }
        }
    }

   
}
