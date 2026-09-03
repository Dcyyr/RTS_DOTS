using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct ShootLightSpawnerSystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();


        foreach (RefRO<Shooting> shoot in SystemAPI.Query<RefRO<Shooting>>())
        {
            if(shoot.ValueRO.m_OnShoot.m_IsTriggered)
            {
                Entity shootLightEntity = state.EntityManager.Instantiate(entitiesReferences.m_ShootingLightPrefab);
                SystemAPI.SetComponent(shootLightEntity, LocalTransform.FromPosition(shoot.ValueRO.m_OnShoot.m_ShootFromPosition));
            }

           
        }
    }

    
}
