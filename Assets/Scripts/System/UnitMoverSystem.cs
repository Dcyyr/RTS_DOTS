using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

partial struct UnitMoverSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRW<LocalTransform> localTransform,
            RefRO<MoveSpeed> moveSpeed,
            RefRW<PhysicsVelocity> physicsVelocity)
            in SystemAPI.Query<RefRW<LocalTransform>,
            RefRO<MoveSpeed>,
            RefRW<PhysicsVelocity>>())
        {
            float3 targetPosition = MouseWorldPosition.Instance.GetPosition();
            float3 moveDirection = targetPosition - localTransform.ValueRO.Position;

            // 防止目标点等于当前位置时 normalize(0) 产生 NaN
            if (math.lengthsq(moveDirection) < 0.01f)
            {
                physicsVelocity.ValueRW.Linear = float3.zero;
                continue;
            }

            moveDirection = math.normalize(moveDirection);
            float rotateSpeed = 10f;
            localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation, quaternion.LookRotation(moveDirection, math.up()),SystemAPI.Time.DeltaTime * rotateSpeed);
            physicsVelocity.ValueRW.Linear = moveDirection * moveSpeed.ValueRO.m_MoveSpeed;
            physicsVelocity.ValueRW.Angular = float3.zero;
        }
    }
}
