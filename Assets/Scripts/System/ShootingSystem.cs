using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct ShootingSystem : ISystem
{


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        foreach ((RefRW<LocalTransform> localTransform, RefRW<Shooting> shoot,
            RefRO<Target> target, RefRW<UnitMover> unitMover) in
            SystemAPI.Query<
                RefRW<LocalTransform>,
                RefRW<Shooting>,
                RefRO<Target>,
                RefRW<UnitMover>>().WithDisabled<MoveOverride>())
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
            shoot.ValueRW.m_Timer -= SystemAPI.Time.DeltaTime;
            if (shoot.ValueRW.m_Timer > 0f)
            {
                continue;
            }
            shoot.ValueRW.m_Timer = shoot.ValueRO.m_MaxTimer;

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.m_TargetEntity);

            if (math.distance(localTransform.ValueRO.Position, targetLocalTransform.Position) > shoot.ValueRO.m_AttackDistance)
            {
                // 目标超出攻击范围，移动
                unitMover.ValueRW.m_TargetPosition = targetLocalTransform.Position;
                continue;
            }
            else
            {
                unitMover.ValueRW.m_TargetPosition = localTransform.ValueRO.Position;
            }

            float3 aimDirection = targetLocalTransform.Position - localTransform.ValueRO.Position;
            aimDirection = math.normalize(aimDirection);

            quaternion targetRotation = quaternion.LookRotation(aimDirection, math.up());
            localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation, targetRotation, unitMover.ValueRO.m_RotateSpeed * SystemAPI.Time.DeltaTime);


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