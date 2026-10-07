using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;

partial struct VisualUnderFogOfWarSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

        EntityCommandBuffer entityCommandBuffer =
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((RefRW<VisualUnderForOfWar> visualUnderForOfWar, Entity entity) in SystemAPI.Query<RefRW<VisualUnderForOfWar>>().WithEntityAccess())
        {
            // 父实体（单位本体）已销毁/无效时跳过，避免崩溃
            if (!SystemAPI.Exists(visualUnderForOfWar.ValueRO.m_ParentEntity) ||
                !SystemAPI.HasComponent<LocalTransform>(visualUnderForOfWar.ValueRO.m_ParentEntity))
            {
                continue;
            }

            LocalTransform localTransform = SystemAPI.GetComponent<LocalTransform>(visualUnderForOfWar.ValueRO.m_ParentEntity);

            // 从单位位置向上打一个球形射线，只和 FOG_OF_WAR 层（我方视野球）相交
            // 命中 = 这个单位落在【我方视野内】；没命中 = 它在【迷雾里】
            bool isInsideSight = collisionWorld.SphereCast(
                localTransform.Position,
                visualUnderForOfWar.ValueRO.m_SphereCastSize,
                new float3(0, 1, 0),
                100,
                new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << GameAssets.FOG_OF_WAR,
                    GroupIndex = 0
                });

            if (isInsideSight)
            {
                // 在我方视野内 → 显示
                if (!visualUnderForOfWar.ValueRO.m_IsVisible)
                {
                    visualUnderForOfWar.ValueRW.m_IsVisible = true;
                    entityCommandBuffer.RemoveComponent<DisableRendering>(entity);
                }
            }
            else
            {
                // 在迷雾里 → 隐藏
                if (visualUnderForOfWar.ValueRO.m_IsVisible)
                {
                    visualUnderForOfWar.ValueRW.m_IsVisible = false;
                    entityCommandBuffer.AddComponent<DisableRendering>(entity);
                }
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}