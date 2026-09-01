using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using static HealthAuthoring;

partial struct ShootingSystem : ISystem
{


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        foreach ((RefRO<LocalTransform> localTransform, RefRW<Shooting> shoot,
            RefRO<Target> target) in
            SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRW<Shooting>,
                RefRO<Target>>())
        {

         
            if (target.ValueRO.m_TargetEntity == Entity.Null)
            {
                continue;
            }
            shoot.ValueRW.m_Timer -= SystemAPI.Time.DeltaTime;
            if (shoot.ValueRW.m_Timer > 0f)
            {
                continue;
            }
            shoot.ValueRW.m_Timer = shoot.ValueRO.m_MaxTimer;

            Entity bulletEntity = state.EntityManager.Instantiate(entitiesReferences.m_BulletPrefabs);
            SystemAPI.SetComponent(bulletEntity, LocalTransform.FromPosition(localTransform.ValueRO.Position));

            RefRW<Bullet> bullet = SystemAPI.GetComponentRW<Bullet>(bulletEntity);
            bullet.ValueRW.m_Damage = shoot.ValueRO.m_ShootDamage;


            RefRW<Target> bulletTarget = SystemAPI.GetComponentRW<Target>(bulletEntity);
            bulletTarget.ValueRW.m_TargetEntity = target.ValueRO.m_TargetEntity;

        }

        
    }


}
