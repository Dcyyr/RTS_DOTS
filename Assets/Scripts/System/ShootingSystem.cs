using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct ShootingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        foreach ((RefRW<LocalTransform> localTransform, RefRW<Shooting> shoot,
            RefRO<Target> target, RefRW<UnitMover> unitMover, Entity entity) in
            SystemAPI.Query<
                RefRW<LocalTransform>,
                RefRW<Shooting>,
                RefRO<Target>,
                RefRW<UnitMover>>().WithDisabled<MoveOverride>().WithEntityAccess())
        {

            if (target.ValueRO.m_TargetEntity == Entity.Null)
            {
                continue;
            }
            // 目标已销毁或没有 LocalTransform 时跳过，避免系统崩溃
            if (!SystemAPI.Exists(target.ValueRO.m_TargetEntity) ||
                !SystemAPI.HasComponent<LocalTransform>(target.ValueRO.m_TargetEntity))
            {
                continue;
            }
            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.m_TargetEntity);

            if (math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position) > shoot.ValueRO.m_AttackDistance)
            {
                // 目标超出攻击范围，走过去（走路时 UnitMoverJob 会面向移动方向）
                unitMover.ValueRW.m_TargetPosition = targetLocalTransform.Position;
                continue;
            }
            else
            {
                unitMover.ValueRW.m_TargetPosition = localTransform.ValueRO.Position;
            }

            // 每帧转向目标（在射程内持续瞄准）
            float3 aimDirection = targetLocalTransform.Position - localTransform.ValueRO.Position;
            aimDirection = math.normalize(aimDirection);

            quaternion targetRotation = quaternion.LookRotation(aimDirection, math.up());
            quaternion newRotation = math.slerp(localTransform.ValueRO.Rotation, targetRotation, unitMover.ValueRO.m_RotateSpeed * SystemAPI.Time.DeltaTime);
            localTransform.ValueRW.Rotation = newRotation;

            // 还没面向目标就不开火，继续转（dot < 0.98 ≈ 夹角超过 ~11°）
            float3 forward = math.mul(newRotation, new float3(0, 0, 1));
            if (math.dot(forward, aimDirection) < 0.98f)
            {
                continue;
            }

            // 开火冷却：只有真正开火才消耗计时，转身过程不烧冷却
            shoot.ValueRW.m_Timer -= SystemAPI.Time.DeltaTime;
            if (shoot.ValueRW.m_Timer > 0f)
            {
                continue;
            }
            shoot.ValueRW.m_Timer = shoot.ValueRO.m_MaxTimer;

            // 攻击敌人时，敌人反击（目标需要有 TargetOverride 组件）
            if (SystemAPI.Exists(target.ValueRO.m_TargetEntity) &&
                SystemAPI.HasComponent<TargetOverride>(target.ValueRO.m_TargetEntity))
            {
                RefRW<TargetOverride> enemyTargetOverride = SystemAPI.GetComponentRW<TargetOverride>(target.ValueRO.m_TargetEntity);
                if (enemyTargetOverride.ValueRO.m_TargetEntity == Entity.Null)
                {
                    enemyTargetOverride.ValueRW.m_TargetEntity = entity;
                }
            }

            Entity bulletEntity = state.EntityManager.Instantiate(entitiesReferences.m_BulletPrefabs);
            float3 bulletSpawnWorldPos = localTransform.ValueRO.TransformPoint(shoot.ValueRO.m_BulletTransform);
            SystemAPI.SetComponent(bulletEntity, LocalTransform.FromPosition(bulletSpawnWorldPos));

            RefRW<Bullet> bullet = SystemAPI.GetComponentRW<Bullet>(bulletEntity);
            bullet.ValueRW.m_Damage = shoot.ValueRO.m_ShootDamage;

            RefRW<Target> bulletTarget = SystemAPI.GetComponentRW<Target>(bulletEntity);
            bulletTarget.ValueRW.m_TargetEntity = target.ValueRO.m_TargetEntity;

            shoot.ValueRW.m_OnShoot.m_IsTriggered = true;
            shoot.ValueRW.m_OnShoot.m_ShootFromPosition = bulletSpawnWorldPos;
        }


        foreach ((RefRW<LocalTransform> localTransform, RefRW<Shooting> shoot,
            RefRO<Target> target, Entity entity) in
            SystemAPI.Query<
                RefRW<LocalTransform>,
                RefRW<Shooting>,
                RefRO<Target>>().WithEntityAccess())
        {

            if (target.ValueRO.m_TargetEntity == Entity.Null)
            {
                continue;
            }

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.m_TargetEntity);


            if (math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position) > shoot.ValueRO.m_AttackDistance)
            {
                continue;
            }

            if(SystemAPI.HasComponent<MoveOverride>(entity) && SystemAPI.IsComponentEnabled<MoveOverride>(entity))
            {
                continue;
            }

            // 开火冷却：只有真正开火才消耗计时，转身过程不烧冷却
            shoot.ValueRW.m_Timer -= SystemAPI.Time.DeltaTime;
            if (shoot.ValueRW.m_Timer > 0f)
            {
                continue;
            }
            shoot.ValueRW.m_Timer = shoot.ValueRO.m_MaxTimer;

            // 攻击敌人时，敌人反击（目标需要有 TargetOverride 组件）
            if (SystemAPI.Exists(target.ValueRO.m_TargetEntity) &&
                SystemAPI.HasComponent<TargetOverride>(target.ValueRO.m_TargetEntity))
            {
                RefRW<TargetOverride> enemyTargetOverride = SystemAPI.GetComponentRW<TargetOverride>(target.ValueRO.m_TargetEntity);
                if (enemyTargetOverride.ValueRO.m_TargetEntity == Entity.Null)
                {
                    enemyTargetOverride.ValueRW.m_TargetEntity = entity;
                }
            }

            Entity bulletEntity = state.EntityManager.Instantiate(entitiesReferences.m_BulletPrefabs);
            float3 bulletSpawnWorldPos = localTransform.ValueRO.TransformPoint(shoot.ValueRO.m_BulletTransform);
            SystemAPI.SetComponent(bulletEntity, LocalTransform.FromPosition(bulletSpawnWorldPos));

            RefRW<Bullet> bullet = SystemAPI.GetComponentRW<Bullet>(bulletEntity);
            bullet.ValueRW.m_Damage = shoot.ValueRO.m_ShootDamage;

            RefRW<Target> bulletTarget = SystemAPI.GetComponentRW<Target>(bulletEntity);
            bulletTarget.ValueRW.m_TargetEntity = target.ValueRO.m_TargetEntity;

            shoot.ValueRW.m_OnShoot.m_IsTriggered = true;
            shoot.ValueRW.m_OnShoot.m_ShootFromPosition = bulletSpawnWorldPos;
        }
    }
}
