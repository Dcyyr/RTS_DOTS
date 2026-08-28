using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

partial struct UnitMoverSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRW<LocalTransform> localTransform,
            RefRO<UnitMover> unitMover,
            RefRW<PhysicsVelocity> physicsVelocity)
            in SystemAPI.Query<RefRW<LocalTransform>,
            RefRO<UnitMover>,
            RefRW<PhysicsVelocity>>())
        {
            float3 moveDirection = unitMover.ValueRO.m_TargetPosition - localTransform.ValueRO.Position;

            // 防止目标点等于当前位置时 normalize(0) 产生 NaN
            if (math.lengthsq(moveDirection) < 0.01f)
            {
                physicsVelocity.ValueRW.Linear = float3.zero;
                continue;
            }

            moveDirection = math.normalize(moveDirection);
            localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation, quaternion.LookRotation(moveDirection, math.up()),SystemAPI.Time.DeltaTime * unitMover.ValueRO.m_RotateSpeed);
            physicsVelocity.ValueRW.Linear = moveDirection * unitMover.ValueRO.m_MoveSpeed;
            physicsVelocity.ValueRW.Angular = float3.zero;
        }
    }
}
