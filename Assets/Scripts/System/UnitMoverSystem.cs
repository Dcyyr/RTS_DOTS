using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;

partial struct UnitMoverSystem : ISystem
{

    public const float REACHED_TARGET_DISTANCE = 2f;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GridSystem.GridSystemData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        GridSystem.GridSystemData gridSystem = SystemAPI.GetSingleton<GridSystem.GridSystemData>();

        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

        //检测什么时候需要寻路，什么时候不需要寻路，什么时候直接移动
        foreach ((RefRO<LocalTransform> localTransform,
            RefRW<TargetPositionPathQueued> targetPositionPathQueued, 
            EnabledRefRW<TargetPositionPathQueued> targetPositionPathQueuedEnable,
            RefRW <FlowFieldPathRequest> flowFieldPathRequest,
            EnabledRefRW<FlowFieldPathRequest> flowFieldPathRequestEnable,
            RefRW <UnitMover> unitMover)
           in SystemAPI.Query<RefRO<LocalTransform>, RefRW<TargetPositionPathQueued>, EnabledRefRW<TargetPositionPathQueued>, RefRW<FlowFieldPathRequest>, EnabledRefRW<FlowFieldPathRequest>, RefRW <UnitMover>>()
           .WithPresent<FlowFieldPathRequest>())
        {

            RaycastInput raycastInput  = new RaycastInput
            {
                Start = localTransform.ValueRO.Position,
                End = targetPositionPathQueued.ValueRO.m_TargetPosition,
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << GameAssets.PATHFINDING_WALL,
                    GroupIndex = 0
                }
            };
            if(!collisionWorld.CastRay(raycastInput))
            {
                unitMover.ValueRW.m_TargetPosition = targetPositionPathQueued.ValueRO.m_TargetPosition;
            }
            else
            {
                flowFieldPathRequest.ValueRW.m_TargetPosition = targetPositionPathQueued.ValueRO.m_TargetPosition;
                flowFieldPathRequestEnable.ValueRW = true;
            }


            targetPositionPathQueuedEnable.ValueRW = false;
        }



            foreach ((RefRO<LocalTransform> localTransform ,RefRW<FlowFieldFollower> flowFieldFollower, EnabledRefRW<FlowFieldFollower> flowFieldFollowerEnable,RefRW < UnitMover> unitMover) 
            in SystemAPI.Query<RefRO<LocalTransform>,RefRW<FlowFieldFollower>, EnabledRefRW<FlowFieldFollower>,RefRW <UnitMover>>())
        {
            int2 gridPosition = GridSystem.GetGridPosition(localTransform.ValueRO.Position, gridSystem.m_GridNodeSize);
            int index = GridSystem.CalculateIndex(gridPosition, gridSystem.m_Width);
            Entity gridSystemDataEntity = gridSystem.m_GridMapArray[flowFieldFollower.ValueRO.m_GridIndex].m_GridEntityArray[index];
            GridSystem.GridNode gridNode = SystemAPI.GetComponent<GridSystem.GridNode>(gridSystemDataEntity);

            float3 gridNodeMoveVector = GridSystem.GetWorldMovementVector(gridNode.m_Vector);

            if(GridSystem.IsWall(gridNode))
            {
                gridNodeMoveVector = flowFieldFollower.ValueRO.m_LastMoveVector;
            }
            else
            {
                flowFieldFollower.ValueRW.m_LastMoveVector = gridNodeMoveVector;
            }

            unitMover.ValueRW.m_TargetPosition = GridSystem.GetWorldCenterPosition(gridPosition.x, gridPosition.y, gridSystem.m_GridNodeSize)
                    + gridNodeMoveVector
                    * (gridSystem.m_GridNodeSize * 1f);

            if(math.distance(localTransform.ValueRO.Position,flowFieldFollower.ValueRO.m_TargetPosition) < gridSystem.m_GridNodeSize)
            {
                unitMover.ValueRW.m_TargetPosition = localTransform.ValueRO.Position;
                flowFieldFollowerEnable.ValueRW = false;
            }

            //检测什么时候需要寻路，什么时候不需要寻路，什么时候直接移动
            RaycastInput raycastInput = new RaycastInput
            {
                Start = localTransform.ValueRO.Position,
                End = flowFieldFollower.ValueRO.m_TargetPosition,
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << GameAssets.PATHFINDING_WALL,
                    GroupIndex = 0
                }
            };
            if (!collisionWorld.CastRay(raycastInput))
            {
                unitMover.ValueRW.m_TargetPosition = flowFieldFollower.ValueRO.m_TargetPosition;
                flowFieldFollowerEnable.ValueRW = false;
            }
           
        }



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
    public void Execute(ref LocalTransform localTransform, ref UnitMover unitMover, ref PhysicsVelocity physicsVelocity, ref PhysicsMass physicsMass)
    {

        physicsVelocity.Linear = float3.zero;
        physicsVelocity.Angular = float3.zero;

        float3 moveDirection = unitMover.m_TargetPosition - localTransform.Position;
        float reachedTargetDistance = UnitMoverSystem.REACHED_TARGET_DISTANCE;
        // 防止目标点等于当前位置时 normalize(0) 产生 NaN
        if (math.lengthsq(moveDirection) < reachedTargetDistance)
        {
            physicsVelocity.Linear = float3.zero;
            unitMover.m_IsMoving = false;
            return;
        }
        unitMover.m_IsMoving = true;


        moveDirection = math.normalize(moveDirection);
        localTransform.Rotation = math.slerp(localTransform.Rotation, quaternion.LookRotation(moveDirection, math.up()), delteTime * unitMover.m_RotateSpeed);
        physicsVelocity.Linear = moveDirection * unitMover.m_MoveSpeed;
    }
}
