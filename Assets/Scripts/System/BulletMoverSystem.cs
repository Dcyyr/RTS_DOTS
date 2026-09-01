using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using static HealthAuthoring;

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
            //敌人死亡后，子弹也要销毁
            if (target.ValueRO.m_TargetEntity == Entity.Null)
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

                entityCommandBuffer.DestroyEntity(entity);
            }
        }
    }

   
}
