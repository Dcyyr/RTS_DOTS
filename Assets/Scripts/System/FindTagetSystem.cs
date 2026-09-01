using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

partial struct FindTagetSystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

        NativeList<DistanceHit> distanceHitsList = new NativeList<DistanceHit>(Allocator.Temp);
        foreach ((RefRO<LocalTransform> localTransform, RefRW<FindTarget> findTarget, RefRW<Target> target)
            in SystemAPI.Query<RefRO<LocalTransform>, RefRW<FindTarget>, RefRW<Target>>())
        {
            distanceHitsList.Clear();
            CollisionFilter collisionFilter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.UNITS_LAYER,
                GroupIndex = 0
            };

            // 搜索计时，不需要每一帧都寻找
            findTarget.ValueRW.m_Timer -= SystemAPI.Time.DeltaTime;

            if (findTarget.ValueRW.m_Timer > 0)
            {
                continue;
            }
            findTarget.ValueRW.m_Timer = findTarget.ValueRO.m_MaxTimer;


            if (collisionWorld.OverlapSphere(localTransform.ValueRO.Position, findTarget.ValueRO.m_Range, ref distanceHitsList, collisionFilter))
            {
                foreach (DistanceHit distanceHit in distanceHitsList)
                {
                    // 命中的实体可能没有 Unit 组件，先判断再读取，避免异常
                    if (SystemAPI.HasComponent<Unit>(distanceHit.Entity))
                    {
                        Unit unit = SystemAPI.GetComponent<Unit>(distanceHit.Entity);
                        if (unit.m_Faction == findTarget.ValueRO.m_TargetFaction)
                        {
                            target.ValueRW.m_TargetEntity = distanceHit.Entity;
                            UnityEngine.Debug.Log("Find Target");
                            break;
                        }
                    }
                }
            }
        }
    }


}