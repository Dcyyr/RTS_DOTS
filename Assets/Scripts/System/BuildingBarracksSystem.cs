using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

partial struct BuildingBarracksSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        foreach ((RefRW<BuildingBarracks> buildingBarracks, RefRO<LocalTransform> localTransform, DynamicBuffer<SpawnUnitTypeBuffer> spawnUnitTypeDynamicBuffer)
            in SystemAPI.Query<RefRW<BuildingBarracks>, RefRO<LocalTransform>, DynamicBuffer<SpawnUnitTypeBuffer>>())
        {

            if (spawnUnitTypeDynamicBuffer.IsEmpty)
            {
                continue;
            }

            if (buildingBarracks.ValueRO.m_ActiveUnitType != spawnUnitTypeDynamicBuffer[0].m_UnitType)
            {
                buildingBarracks.ValueRW.m_ActiveUnitType = spawnUnitTypeDynamicBuffer[0].m_UnitType;
                UnitTypeSO activeUnitTypeSO = GameAssets.Instance.m_UnitTypeSOList.GetUnitTypeSO(buildingBarracks.ValueRO.m_ActiveUnitType);


                buildingBarracks.ValueRW.m_CreateMaxTimer = activeUnitTypeSO.m_ProgressMax;
            }

            buildingBarracks.ValueRW.m_CreateTimer += SystemAPI.Time.DeltaTime;
            if (buildingBarracks.ValueRW.m_CreateTimer < buildingBarracks.ValueRW.m_CreateMaxTimer)
            {
                continue;
            }
            buildingBarracks.ValueRW.m_CreateTimer = 0;

            UnitTypeSO.UnitType unitType = spawnUnitTypeDynamicBuffer[0].m_UnitType;
            UnitTypeSO unitTypeSO = GameAssets.Instance.m_UnitTypeSOList.GetUnitTypeSO(unitType);
            spawnUnitTypeDynamicBuffer.RemoveAt(0);
            //生成实体对象
            Entity SpawnUnitEntity = state.EntityManager.Instantiate(unitTypeSO.GetPrefabEntities(entitiesReferences));
            SystemAPI.SetComponent(SpawnUnitEntity, LocalTransform.FromPosition(localTransform.ValueRO.Position));

            SystemAPI.SetComponent(SpawnUnitEntity, new MoveOverride
            {
                m_TargetPosition = localTransform.ValueRO.Position + buildingBarracks.ValueRO.m_RallyPositionOffset
            });

            SystemAPI.SetComponentEnabled<MoveOverride>(SpawnUnitEntity, true);
        }


    }

}