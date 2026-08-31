using Mono.Cecil;
using Unity.Burst;
using Unity.Entities;
using Unity.VisualScripting;

partial struct ShootingSystem : ISystem
{
    

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach((RefRW<Shooting> shoot,
            RefRO<Target> target) in
            SystemAPI.Query<RefRW<Shooting>,
            RefRO<Target>>())
        {
            if(target.ValueRO.m_TargetEntity == Entity.Null)
            {
                continue;
            }
            shoot.ValueRW.m_Timer -= SystemAPI.Time.DeltaTime;
            if (shoot.ValueRW.m_Timer >0f)
            {
                continue;
            }
            shoot.ValueRW.m_Timer = shoot.ValueRO.m_MaxTimer;
        }

        UnityEngine.Debug.Log("Shooting");
    }

   
}
