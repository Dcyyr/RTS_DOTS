﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
partial struct MeleeAttackSystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        NativeList<RaycastHit> raycastHitList = new NativeList<RaycastHit>(Allocator.Temp);


        foreach ((RefRW<LocalTransform> LocalTransform,
            RefRW<MeleeAttack> meleeAttack,
            RefRO<Target> target,
            RefRW<TargetPositionPathQueued> targetPositionPathQueued, 
            EnabledRefRW<TargetPositionPathQueued> targetPositionPathQueuedEnable) in
            SystemAPI.Query<RefRW<LocalTransform>,
            RefRW<MeleeAttack>, 
            RefRO<Target>, 
            RefRW<TargetPositionPathQueued> ,
            EnabledRefRW<TargetPositionPathQueued>>().WithDisabled<MoveOverride>().WithPresent<TargetPositionPathQueued>())
        {
            if (target.ValueRO.m_TargetEntity == Entity.Null)
            {
                continue;
            }
            // 目标实体无效、已销毁、或缺少 LocalTransform/Health 时跳过，避免系统每帧崩溃
            if (!SystemAPI.Exists(target.ValueRO.m_TargetEntity) ||
                !SystemAPI.HasComponent<LocalTransform>(target.ValueRO.m_TargetEntity) ||
                !SystemAPI.HasComponent<Health>(target.ValueRO.m_TargetEntity))
            {
                continue;
            }

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.m_TargetEntity);
            float AttackDistance = 2f;

            // distancesq 是距离的平方，要和 AttackDistance 的平方比较
            bool isCloseEnoughToAttack = math.distancesq(LocalTransform.ValueRO.Position, targetLocalTransform.Position) < AttackDistance * AttackDistance;

            bool isTouchingTarget = false;
            if (!isCloseEnoughToAttack)
            {
                //
                float3 dirToTarget = targetLocalTransform.Position - LocalTransform.ValueRO.Position;
                dirToTarget = math.normalize(dirToTarget);
                float distanceOffset = .25f;
                RaycastInput raycastInput = new RaycastInput
                {
                    Start = LocalTransform.ValueRO.Position,
                    End = LocalTransform.ValueRO.Position + dirToTarget * (meleeAttack.ValueRO.m_ColliderSize + distanceOffset),
                    Filter = new CollisionFilter
                    {
                        BelongsTo = ~0u,
                        CollidesWith = 1u << GameAssets.UNITS_LAYER | 1u << GameAssets.BUILDINGS_LAYER,
                        GroupIndex = 0,


                    },

                };
                raycastHitList.Clear();

                if (collisionWorld.CastRay(raycastInput, ref raycastHitList))
                {
                    foreach (RaycastHit raycastHit in raycastHitList)
                    {
                        if (raycastHit.Entity == target.ValueRO.m_TargetEntity)
                        {
                            isTouchingTarget = true;
                            break;
                        }
                    }
                }



            }
            if (!isCloseEnoughToAttack && !isTouchingTarget)
            {
                // 目标不在攻击距离内，继续移动
                targetPositionPathQueued.ValueRW.m_TargetPosition = targetLocalTransform.Position;
                targetPositionPathQueuedEnable.ValueRW = true;
            }
            else
            {
                // 可以攻击
                targetPositionPathQueued.ValueRW.m_TargetPosition = LocalTransform.ValueRO.Position;
                targetPositionPathQueuedEnable.ValueRW = true;

                meleeAttack.ValueRW.m_Timer -= SystemAPI.Time.DeltaTime;
                if (meleeAttack.ValueRO.m_Timer > 0)
                {
                    continue;
                }
                meleeAttack.ValueRW.m_Timer = meleeAttack.ValueRO.m_MaxTimer;

                RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.m_TargetEntity);

                targetHealth.ValueRW.m_Health -= meleeAttack.ValueRO.m_Damage;
                targetHealth.ValueRW.m_OnHealthChanged = true;

                meleeAttack.ValueRW.OnAttacked = true;

            }
        }
    }

}