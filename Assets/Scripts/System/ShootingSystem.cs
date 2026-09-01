using Unity.Burst;
using Unity.Entities;
using static HealthAuthoring;

partial struct ShootingSystem : ISystem
{


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRW<Shooting> shoot,
            RefRO<Target> target) in
            SystemAPI.Query<RefRW<Shooting>,
            RefRO<Target>>())
        {
            if (target.ValueRO.m_TargetEntity == Entity.Null)
            {
                continue;
            }
            shoot.ValueRW.m_Timer -= SystemAPI.Time.DeltaTime;
            if (shoot.ValueRW.m_Timer > 0f)
            {
                continue;
            }
            shoot.ValueRW.m_Timer = shoot.ValueRO.m_MaxTimer;

            RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.m_TargetEntity);
            int damageAmount = 1;
            targetHealth.ValueRW.m_Health -= damageAmount;

        }

        
    }


}
