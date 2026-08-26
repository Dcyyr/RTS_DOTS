using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

partial struct UnitMoverSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 将 Struct 转换为 Ref 用来读写操作
        foreach ((RefRW<LocalTransform> localTransform,
            RefRO<MoveSpeed> moveSpeed,
            RefRW<PhysicsVelocity> physicsVelocity)
            in SystemAPI.Query<RefRW<LocalTransform>,
            RefRO<MoveSpeed>,
            RefRW<PhysicsVelocity>>())
        {
            float3 targetPosition = localTransform.ValueRO.Position + new float3(10, 0, 0);
            float3 moveDirection = targetPosition - localTransform.ValueRO.Position;

            moveDirection = math.normalize(moveDirection);

            localTransform.ValueRW.Rotation = quaternion.LookRotation(moveDirection, math.up());
            physicsVelocity.ValueRW.Linear = moveDirection * moveSpeed.ValueRO.m_MoveSpeed;

            physicsVelocity.ValueRW.Angular = float3.zero;
            //localTransform.ValueRW.Position += moveDirection * moveSpeed.ValueRO.m_MoveSpeed * SystemAPI.Time.DeltaTime;
        }
    }
}