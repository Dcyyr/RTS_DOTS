using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct BulletMoverSystem : ISystem
{
    

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer =
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((RefRW<LocalTransform> localTransform,RefRO<Target> target,RefRO<Bullet> bullet,Entity entity)
            in SystemAPI.Query<RefRW<LocalTransform>,RefRO<Target>,RefRO<Bullet>>().WithEntityAccess())
        {
            // 目标已销毁/无效（僵尸被打死,子弹也销毁，避免卡在枪口堆积、也避免系统每帧崩溃
            if (target.ValueRO.m_TargetEntity == Entity.Null ||
                !SystemAPI.Exists(target.ValueRO.m_TargetEntity) ||
                !SystemAPI.HasComponent<LocalTransform>(target.ValueRO.m_TargetEntity) ||
                !SystemAPI.HasComponent<ShootVictim>(target.ValueRO.m_TargetEntity))
            {
                entityCommandBuffer.DestroyEntity(entity);
                continue;
            }

            

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.m_TargetEntity);
            ShootVictim shootVictim = SystemAPI.GetComponent<ShootVictim>(target.ValueRO.m_TargetEntity);
            float3 targetPosition = targetLocalTransform.TransformPoint(shootVictim.m_HitPosition);


            float distanceBeforeSq = math.distancesq(localTransform.ValueRO.Position, targetPosition);

            float3 moveDirection = targetPosition - localTransform.ValueRO.Position;
            moveDirection = math.normalize(moveDirection);

            localTransform.ValueRW.Position += moveDirection * bullet.ValueRO.m_Speed * SystemAPI.Time.DeltaTime;

            float distanceAfterSq = math.distancesq(localTransform.ValueRO.Position, targetPosition);

            if(distanceAfterSq > distanceBeforeSq)
            {   //子弹速度过快而不能正确的命中敌人
                localTransform.ValueRW.Position = targetPosition;
            }

            float destroyDistance = 0.15f;
            // distancesq 是距离的平方，要和 destroyDistance 的平方比较
            if(math.distancesq(localTransform.ValueRO.Position, targetPosition) < destroyDistance * destroyDistance)
            {

                RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(target.ValueRO.m_TargetEntity);
                
                targetHealth.ValueRW.m_Health -= bullet.ValueRO.m_Damage;
                targetHealth.ValueRW.m_OnHealthChanged = true;

                entityCommandBuffer.DestroyEntity(entity);
            }
        }
    }

   
}
