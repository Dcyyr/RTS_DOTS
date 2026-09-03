using Unity.Burst;
using Unity.Entities;

partial struct ShootLightDestorySystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((RefRW<ShootLight> shootLight,Entity entity) in SystemAPI.Query<RefRW<ShootLight>>().WithEntityAccess())
        {
            shootLight.ValueRW.m_Timer -= SystemAPI.Time.DeltaTime;
            if(shootLight.ValueRO.m_Timer < 0f)
            {
                entityCommandBuffer.DestroyEntity(entity);
            }
        }
    }

   
}
