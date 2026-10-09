using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct ConstructionBuildingSystem : ISystem
{
   

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //用来延迟执行实体创建、销毁、修改组件的操作
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach((RefRW<LocalTransform> localTransform,RefRW<BuildingConstruction> buildingConstruction,Entity entity) in 
            SystemAPI.Query<RefRW<LocalTransform>,RefRW<BuildingConstruction>>().WithEntityAccess())
        {

            RefRW<LocalTransform> localVisualTransform = SystemAPI.GetComponentRW<LocalTransform>(buildingConstruction.ValueRO.m_VisualEntity);
            localVisualTransform.ValueRW.Position = math.lerp(buildingConstruction.ValueRO.m_StartPosition, buildingConstruction.ValueRO.m_EndPosition,
                buildingConstruction.ValueRO.m_ConstructionTimer / buildingConstruction.ValueRO.m_ConstructionTimerMax);

            buildingConstruction.ValueRW.m_ConstructionTimer += SystemAPI.Time.DeltaTime;

            if (buildingConstruction.ValueRW.m_ConstructionTimer >= buildingConstruction.ValueRO.m_ConstructionTimerMax)
            {
                // 1. 实例化建筑预制体（记录进ECB，不是立刻生成）
                Entity spawnBuildingEntity = entityCommandBuffer.Instantiate(buildingConstruction.ValueRO.m_FinalPrefabEntity);
                // 2. 设置新建筑的位置 = 原来建造预览实体的位置
                entityCommandBuffer.SetComponent(spawnBuildingEntity, LocalTransform.FromPosition(localTransform.ValueRO.Position));

                // 3. 删除建造预览的可视化实体
                // 如果有「施工期间的可视化实体」就一并删除；没有就跳过（避免对 Entity.Null 调用 DestroyEntity）
                if (buildingConstruction.ValueRO.m_VisualEntity != Entity.Null)
                {
                    entityCommandBuffer.DestroyEntity(buildingConstruction.ValueRO.m_VisualEntity);
                }
                // 4. 删除当前这个「建造标记实体」（buildingConstruction所在的entity）
                entityCommandBuffer.DestroyEntity(entity);

            }
        }
    }

   
}
