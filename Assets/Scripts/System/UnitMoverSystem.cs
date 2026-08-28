using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct UnitMoverSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        UnitMoverJob unitMoverJob = new UnitMoverJob
        {
            delteTime = SystemAPI.Time.DeltaTime
        };

        unitMoverJob.ScheduleParallel();
    }
}

// 多线程版本
[BurstCompile]
public partial struct UnitMoverJob : IJobEntity
{
    public float delteTime;

    //ref可以写入，in只能读
    public void Execute(ref LocalTransform localTransform, in UnitMover unitMover, ref PhysicsVelocity physicsVelocity)
    {
        float3 moveDirection = unitMover.m_TargetPosition - localTransform.Position;

        // 防止目标点等于当前位置时 normalize(0) 产生 NaN
        if (math.lengthsq(moveDirection) < 0.01f)
        {
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;

            return;
        }

        moveDirection = math.normalize(moveDirection);
        localTransform.Rotation = math.slerp(localTransform.Rotation, quaternion.LookRotation(moveDirection, math.up()), delteTime * unitMover.m_RotateSpeed);
        physicsVelocity.Linear = moveDirection * unitMover.m_MoveSpeed;
        physicsVelocity.Angular = float3.zero;
    }
}
