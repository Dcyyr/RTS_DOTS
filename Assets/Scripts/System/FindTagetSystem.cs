﻿﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
partial struct FindTargetSystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

        NativeList<DistanceHit> distanceHitsList = new NativeList<DistanceHit>(Allocator.Temp);

        foreach ((RefRO<LocalTransform> localTransform, RefRW<FindTarget> findTarget, RefRW<Target> target, RefRO<TargetOverride> targetOverride)
            in SystemAPI.Query<RefRO<LocalTransform>, RefRW<FindTarget>, RefRW<Target>,RefRO<TargetOverride>>())
        {
            // 搜索计时，不需要每一帧都寻找
            findTarget.ValueRW.m_Timer -= SystemAPI.Time.DeltaTime;

            if (findTarget.ValueRW.m_Timer > 0)
            {
                continue;
            }
            findTarget.ValueRW.m_Timer = findTarget.ValueRO.m_MaxTimer;


            if (targetOverride.ValueRO.m_TargetEntity != Entity.Null)
            {
                target.ValueRW.m_TargetEntity = targetOverride.ValueRO.m_TargetEntity;
                continue;
            }

            distanceHitsList.Clear();
            CollisionFilter collisionFilter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.UNITS_LAYER | 1u << GameAssets.BUILDINGS_LAYER,
                GroupIndex = 0
            };

           


            Entity closestTargetEntity = Entity.Null;
            float closestTargetDistance = float.MaxValue;
            float currentTargetDistance = 0f;
            if(target.ValueRO.m_TargetEntity != Entity.Null)
            {
                // 目标可能是已销毁实体，或是不带 LocalTransform 的静态碰撞体实体（例如建筑的碰撞体）
                // → 直接清掉目标，下一帧重新寻找，避免抛异常
                if (!SystemAPI.Exists(target.ValueRO.m_TargetEntity) ||
                    !SystemAPI.HasComponent<LocalTransform>(target.ValueRO.m_TargetEntity))
                {
                    target.ValueRW.m_TargetEntity = Entity.Null;
                    closestTargetEntity = Entity.Null;
                }
                else
                {
                    closestTargetEntity = target.ValueRO.m_TargetEntity;
                    LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.m_TargetEntity);
                    closestTargetDistance = math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position);
                    currentTargetDistance = 2f;
                }
            }

            if (collisionWorld.OverlapSphere(localTransform.ValueRO.Position, findTarget.ValueRO.m_Range, ref distanceHitsList, collisionFilter))
            {
                foreach (DistanceHit distanceHit in distanceHitsList)
                {
                    // 命中的实体可能没有 Unit 组件，先判断再读取，避免异常
                    if (!SystemAPI.Exists(distanceHit.Entity) ||
                        !SystemAPI.HasComponent<Faction>(distanceHit.Entity) ||
                        !SystemAPI.HasComponent<LocalTransform>(distanceHit.Entity))
                    {
                        continue;
                    }
                    Faction targetFaction = SystemAPI.GetComponent<Faction>(distanceHit.Entity);
                    if (targetFaction.m_FactionType == findTarget.ValueRO.m_TargetFaction)
                    {   //最近的那个实体没有找到，让打中那个实体成最近的
                        if(closestTargetEntity == Entity.Null)
                        {
                            closestTargetEntity = distanceHit.Entity;
                            closestTargetDistance = distanceHit.Distance;
                        }
                        else
                        {
                            if(distanceHit.Distance + currentTargetDistance < closestTargetDistance)
                            {
                                closestTargetEntity = distanceHit.Entity;
                                closestTargetDistance = distanceHit.Distance;
                            }
                        }

                        target.ValueRW.m_TargetEntity = distanceHit.Entity;
                        UnityEngine.Debug.Log("Find Target");
                        break;
                    }
                    
                }
            }
        }
    }


}