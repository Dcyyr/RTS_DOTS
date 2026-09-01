using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

partial struct ZombieSpawnerSystem : ISystem
{
   

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesRef = SystemAPI.GetSingleton<EntitiesReferences>();


        foreach ((RefRW<LocalTransform> localTransform
            ,RefRW<ZombieSpawner> zombieSpawner) 
            in SystemAPI.Query
            <RefRW<LocalTransform>,
            RefRW<ZombieSpawner>>())
        {
            zombieSpawner.ValueRW.m_Timer -= SystemAPI.Time.DeltaTime;
            if(zombieSpawner.ValueRW.m_Timer >0)
            {
                continue;
            }
            zombieSpawner.ValueRW.m_Timer = zombieSpawner.ValueRO.m_MaxTimer;

            Entity zombiePrefab = state.EntityManager.Instantiate(entitiesRef.m_ZombiePrefab);
            SystemAPI.SetComponent(zombiePrefab, LocalTransform.FromPosition(localTransform.ValueRO.Position));

        }
    }

  
}
