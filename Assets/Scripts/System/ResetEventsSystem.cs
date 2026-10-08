using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup),OrderLast = true)]
partial struct ResetEventsSystem : ISystem
{
   
    // 注意：本系统要调用托管单例 DOTSEventsManager.instance，所以 OnUpdate 不能 Burst 编译
    // （内部的 Reset*EventsJob 各自带 [BurstCompile]，依然并行编译，效率不受影响）
    public void OnUpdate(ref SystemState state)
    {

        if(SystemAPI.HasSingleton<BuildingHQ>())
        {
            Health hqHealth = SystemAPI.GetComponent<Health>(SystemAPI.GetSingletonEntity<BuildingHQ>());
            if(hqHealth.m_Dead)
            {
                DOTSEventsManager.instance.TriggerOnHQDead();
            }
        }

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
        health.m_Dead = false;
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



