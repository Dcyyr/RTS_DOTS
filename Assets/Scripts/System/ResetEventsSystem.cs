using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup),OrderLast = true)]
partial struct ResetEventsSystem : ISystem
{
   
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach(RefRW<Selected> selected in SystemAPI.Query<RefRW<Selected>>().WithPresent<Selected>())
        {
            selected.ValueRW.m_OnSelected = false;
            selected.ValueRW.m_OnDeselected = false;
        }

        foreach (RefRW<Health> health in SystemAPI.Query<RefRW<Health>>())
        {
            health.ValueRW.m_OnHealthChanged = false;
        }
    }

   
}
