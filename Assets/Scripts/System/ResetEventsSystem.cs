using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup),OrderLast = true)]
partial struct ResetEventsSystem : ISystem
{
   
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        new ResetSelectedEventsJob().ScheduleParallel();
        new ResetHealthEventsJob().ScheduleParallel();
        new ResetShootingEventsJob().ScheduleParallel();
        new ResetMeleeAttackEventsJob().ScheduleParallel();
        /**
        foreach(RefRW<Selected> selected in SystemAPI.Query<RefRW<Selected>>().WithPresent<Selected>())
        {
            selected.ValueRW.m_OnSelected = false;
            selected.ValueRW.m_OnDeselected = false;
        }

        foreach (RefRW<Health> health in SystemAPI.Query<RefRW<Health>>())
        {
            health.ValueRW.m_OnHealthChanged = false;
        }


        foreach (RefRW<Shooting> shooting in SystemAPI.Query<RefRW<Shooting>>())
        {
            shooting.ValueRW.m_OnShoot.m_IsTriggered = false;
        }
        **/
    }


}

[BurstCompile]
[WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
public partial struct ResetSelectedEventsJob : IJobEntity
{
    public void Execute(ref Selected selected)
    {
        selected.m_OnSelected = false;
        selected.m_OnDeselected = false;
    }

}

[BurstCompile]
public partial struct ResetHealthEventsJob : IJobEntity
{
    public void Execute(ref Health health)
    {
        health.m_OnHealthChanged = false;
    }
}

[BurstCompile]
public partial struct ResetShootingEventsJob : IJobEntity
{
    public void Execute(ref Shooting shooting)
    {
        shooting.m_OnShoot.m_IsTriggered = false;
    }
}

[BurstCompile]
public partial struct ResetMeleeAttackEventsJob : IJobEntity
{
    public void Execute(ref MeleeAttack attack)
    {
        attack.OnAttacked = false;
    }
}



