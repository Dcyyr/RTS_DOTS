using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using static HealthAuthoring;

partial struct HealthDeadTestSystem : ISystem
{
  
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer =
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach((RefRW<Health> health,Entity entity)
            in SystemAPI.Query<RefRW<Health>>().WithEntityAccess())
        {
            if(health.ValueRO.m_Health <=0)
            {
                health.ValueRW.m_Dead = true;
                entityCommandBuffer.DestroyEntity(entity);
            }
        }


    }

   
}
