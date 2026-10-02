﻿#define GRID_DEBUG
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public partial struct GridSystem : ISystem
{

    public const int WALL_COST = byte.MaxValue;
    public const int FLOW_FIELDMAP_COUNT = 100;
    public struct GridSystemData :IComponentData
    {
        public int m_Width;
        public int m_Height;
        public float m_GridNodeSize;
        public NativeArray<GridMap> m_GridMapArray;
        public int m_NextGridIndex;

    }

    public struct GridMap
    {
        public NativeArray<Entity> m_GridEntityArray;
        public int2 m_TargetGridPosition;
        public bool m_IsValid;
    }

    public struct GridNode :IComponentData
    {
        public int x;
        public int y;
        public int m_Index;
        /// <summary>
        /// Data
        /// </summary>
        public byte m_Cost;
        public byte m_BestCost;
        public float2 m_Vector;
    }

#if !GRID_DEBUG
    [BurstCompile]
#endif
    public void OnCreate(ref SystemState state)
    {
        int width = 20;
        int height = 10;
        float gridNodeSize = 5f;
        int totalCount = width * height;

        Entity gridNodeEntity = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponent<GridNode>(gridNodeEntity);

        NativeArray<GridMap> gridMapArray = new NativeArray<GridMap>(FLOW_FIELDMAP_COUNT, Allocator.Persistent);

        for (int i = 0; i < FLOW_FIELDMAP_COUNT; i++)
        {
            GridMap gridMap = new GridMap();
            gridMap.m_IsValid = false;//初始化为false，只有在计算流场的时候才会设置为true
            gridMap.m_GridEntityArray = new NativeArray<Entity>(totalCount, Allocator.Persistent);

            state.EntityManager.Instantiate(gridNodeEntity, gridMap.m_GridEntityArray);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int index = CalculateIndex(x, y, width);
                    GridNode gridNode = new GridNode
                    {
                        x = x,
                        y = y,
                        m_Index = index,

                    };
#if GRID_DEBUG
                    state.EntityManager.SetName(gridMap.m_GridEntityArray[index], "GridNode" + x + "_" + y);
#endif
                    SystemAPI.SetComponent(gridMap.m_GridEntityArray[index], gridNode);
                }

            }

            gridMapArray[i] = gridMap;
        }


        state.EntityManager.AddComponent<GridSystemData>(state.SystemHandle);
        state.EntityManager.SetComponentData(state.SystemHandle, new GridSystemData
        {
            m_Width = width,
            m_Height = height,
            m_GridNodeSize = gridNodeSize,
            m_GridMapArray = gridMapArray,
        });


    }
#if !GRID_DEBUG
    [BurstCompile]
#endif
    public void OnUpdate(ref SystemState state)
    {
        GridSystemData gridSystemData = SystemAPI.GetComponent<GridSystemData>(state.SystemHandle);

        foreach ((RefRW<FlowFieldPathRequest> flowFieldPathRequest,EnabledRefRW<FlowFieldPathRequest> flowFieldPathRequestEnable, RefRW<FlowFieldFollower> flowFieldFollower, EnabledRefRW<FlowFieldFollower> flowFieldFollowerEnable)
            in SystemAPI.Query<RefRW<FlowFieldPathRequest>, EnabledRefRW<FlowFieldPathRequest>, RefRW<FlowFieldFollower>, EnabledRefRW<FlowFieldFollower>>().WithPresent<FlowFieldFollower>())
        {

            int2 targetGridPosition = GetGridPosition(flowFieldPathRequest.ValueRO.m_TargetPosition, gridSystemData.m_GridNodeSize);

            flowFieldPathRequestEnable.ValueRW = false;

            
            bool isAlreadyCalculated = false;
            for (int i = 0; i < FLOW_FIELDMAP_COUNT; i++) 
            {
                if (gridSystemData.m_GridMapArray[i].m_IsValid && gridSystemData.m_GridMapArray[i].m_TargetGridPosition.Equals(targetGridPosition))
                {
                    flowFieldFollower.ValueRW.m_GridIndex = i;
                    flowFieldFollower.ValueRW.m_TargetPosition = flowFieldPathRequest.ValueRO.m_TargetPosition;
                    flowFieldFollowerEnable.ValueRW = true;
                    isAlreadyCalculated = true;
                    break;
                }
            }

            if(isAlreadyCalculated)
            {
                //已经计算过了，就不需要再计算了，直接使用之前的流场数据
                continue;
            }

            //选择一个单位都要重新计算流场，所以每次都要切换一个gridmap
            int gridIndex = gridSystemData.m_NextGridIndex;
            gridSystemData.m_NextGridIndex = (gridSystemData.m_NextGridIndex + 1) % FLOW_FIELDMAP_COUNT;
            SystemAPI.SetComponent(state.SystemHandle, gridSystemData);
            //

            flowFieldFollower.ValueRW.m_GridIndex = gridIndex;
            flowFieldFollower.ValueRW.m_TargetPosition = flowFieldPathRequest.ValueRO.m_TargetPosition;
            flowFieldFollowerEnable.ValueRW = true;

            NativeArray<RefRW<GridNode>> gridNodeNativeArray = new NativeArray<RefRW<GridNode>>(gridSystemData.m_Width * gridSystemData.m_Height, Allocator.Temp);

            for (int x = 0; x < gridSystemData.m_Width; x++)
            {
                for (int y = 0; y < gridSystemData.m_Height; y++)
                {
                    int index = CalculateIndex(x, y, gridSystemData.m_Width);
                    Entity gridNodeEntity = gridSystemData.m_GridMapArray[gridIndex].m_GridEntityArray[index];

                    RefRW<GridNode> gridNode = SystemAPI.GetComponentRW<GridNode>(gridNodeEntity);

                    gridNode.ValueRW.m_Vector = new Vector2(0, 1);
                    gridNodeNativeArray[index] = gridNode;

                    if (x == targetGridPosition.x && y == targetGridPosition.y)
                    {
                        gridNode.ValueRW.m_Cost = 0;
                        gridNode.ValueRW.m_BestCost = 0;
                    }
                    else
                    {
                        gridNode.ValueRW.m_Cost = 1;
                        gridNode.ValueRW.m_BestCost = byte.MaxValue;
                    }
                }
            }

            //WallCost
            PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
            NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);

            for (int x = 0; x < gridSystemData.m_Width; x++)
            {
                for (int y = 0; y < gridSystemData.m_Height; y++)
                {
                    if (collisionWorld.OverlapSphere(
                        GetWorldCenterPosition(x, y, gridSystemData.m_GridNodeSize),
                        gridSystemData.m_GridNodeSize * .5f,
                        ref distanceHitList, new CollisionFilter
                        {
                            BelongsTo = ~0u,
                            CollidesWith = 1u << GameAssets.PATHFINDING_WALL,
                            GroupIndex = 0,
                        }))
                    {
                        int index = CalculateIndex(x, y, gridSystemData.m_Width);
                        gridNodeNativeArray[index].ValueRW.m_Cost = WALL_COST;
                    }
                }
            }
            distanceHitList.Dispose();



            NativeQueue<RefRW<GridNode>> gridNodeQueue = new NativeQueue<RefRW<GridNode>>(Allocator.Temp);

            RefRW<GridNode> targetGridNode = gridNodeNativeArray[CalculateIndex(targetGridPosition, gridSystemData.m_Width)];
            gridNodeQueue.Enqueue(targetGridNode);


            int safety = gridSystemData.m_Width * gridSystemData.m_Height * 8;//取决于width和height的乘积
            while (gridNodeQueue.Count > 0)
            {
                safety--;
                if (safety < 0)
                {
                    Debug.Log("Safety break");
                    break;
                }

                RefRW<GridNode> currentGridNode = gridNodeQueue.Dequeue();

                NativeList<RefRW<GridNode>> neighbourGridNodeList =
                    GetNeighbourGridNodeList(currentGridNode, gridNodeNativeArray, gridSystemData.m_Width, gridSystemData.m_Height);

                foreach (RefRW<GridNode> neighbourGridNode in neighbourGridNodeList)
                {
                    //检测到墙
                    if (neighbourGridNode.ValueRO.m_Cost == WALL_COST)
                    {
                        continue;
                    }

                    byte newBestCost = (byte)(currentGridNode.ValueRO.m_BestCost + neighbourGridNode.ValueRO.m_Cost);

                    if (newBestCost < neighbourGridNode.ValueRO.m_BestCost)
                    {
                        neighbourGridNode.ValueRW.m_BestCost = newBestCost;
                        neighbourGridNode.ValueRW.m_Vector =
                            CalculateVector(neighbourGridNode.ValueRO.x, neighbourGridNode.ValueRO.y, currentGridNode.ValueRO.x, currentGridNode.ValueRO.y);

                        // 关键：代价变好就重新入队，流场才能扩散到整张地图
                        gridNodeQueue.Enqueue(neighbourGridNode);
                    }
                }
                neighbourGridNodeList.Dispose();
            }

            gridNodeQueue.Dispose();
            gridNodeNativeArray.Dispose();

            //
            GridMap gridMap = gridSystemData.m_GridMapArray[gridIndex];
            gridMap.m_TargetGridPosition = targetGridPosition;
            gridMap.m_IsValid = true;
            gridSystemData.m_GridMapArray[gridIndex] = gridMap;

            SystemAPI.SetComponent(state.SystemHandle, gridSystemData);
        }


        if (Input.GetMouseButtonDown(0))
        {
            float3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();
            int2 mouseGridPosition = GetGridPosition(mouseWorldPosition, gridSystemData.m_GridNodeSize);
            if (IsValidGridPosition(mouseGridPosition, gridSystemData.m_Width, gridSystemData.m_Height))
            {
                /***
                int index = CalculateIndex(mouseGridPosition.x, mouseGridPosition.y, gridSystemData.m_Width);
                Entity entity = gridSystemData.m_GridMapArray.m_GridEntityArray[index];

                RefRW<GridNode> gridNode = SystemAPI.GetComponentRW<GridNode>(entity);***/
            }


        }
#if GRID_DEBUG
        GridSystemDebug.instance?.InitizlizeGrid(gridSystemData);
        GridSystemDebug.instance?.UpdateGrid(gridSystemData);
#endif
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        RefRW<GridSystemData> gridSystemData = SystemAPI.GetComponentRW<GridSystemData>(state.SystemHandle);
        for (int i = 0; i < FLOW_FIELDMAP_COUNT; i++) 
        {
            gridSystemData.ValueRW.m_GridMapArray[i].m_GridEntityArray.Dispose();
        }
        gridSystemData.ValueRW.m_GridMapArray.Dispose();
    }


    public static NativeList<RefRW<GridNode>> GetNeighbourGridNodeList(
        RefRW<GridNode> currentGridNode,
        NativeArray<RefRW<GridNode>> gridNodeNativeArray,
        int width,
        int height)
    {
        NativeList<RefRW<GridNode>> neighbourGridNodeList = new NativeList<RefRW<GridNode>>(Allocator.Temp);

        int gridNodeX = currentGridNode.ValueRO.x;
        int gridNodeY = currentGridNode.ValueRO.y;

        int2 positionLeft = new int2(gridNodeX - 1, gridNodeY + 0);
        int2 positionRight = new int2(gridNodeX + 1, gridNodeY + 0);
        int2 positionUp = new int2(gridNodeX + 0, gridNodeY + 1);
        int2 positionDown = new int2(gridNodeX + 0, gridNodeY - 1);

        int2 positionLowerLeft = new int2(gridNodeX - 1, gridNodeY - 1);
        int2 positionLowerRight = new int2(gridNodeX + 1, gridNodeY - 1);
        int2 positionUpperLeft = new int2(gridNodeX - 1, gridNodeY + 1);
        int2 positionUpperRight = new int2(gridNodeX + 1, gridNodeY + 1);

        if (IsValidGridPosition(positionLeft, width, height))
        {
            neighbourGridNodeList.Add(gridNodeNativeArray[CalculateIndex(positionLeft, width)]);
        }
        if (IsValidGridPosition(positionRight, width, height))
        {
            neighbourGridNodeList.Add(gridNodeNativeArray[CalculateIndex(positionRight, width)]);
        }
        if (IsValidGridPosition(positionUp, width, height))
        {
            neighbourGridNodeList.Add(gridNodeNativeArray[CalculateIndex(positionUp, width)]);
        }
        if (IsValidGridPosition(positionDown, width, height))
        {
            neighbourGridNodeList.Add(gridNodeNativeArray[CalculateIndex(positionDown, width)]);
        }

        if (IsValidGridPosition(positionLowerLeft, width, height))
        {
            neighbourGridNodeList.Add(gridNodeNativeArray[CalculateIndex(positionLowerLeft, width)]);
        }
        if (IsValidGridPosition(positionLowerRight, width, height))
        {
            neighbourGridNodeList.Add(gridNodeNativeArray[CalculateIndex(positionLowerRight, width)]);
        }
        if (IsValidGridPosition(positionUpperLeft, width, height))
        {
            neighbourGridNodeList.Add(gridNodeNativeArray[CalculateIndex(positionUpperLeft, width)]);
        }
        if (IsValidGridPosition(positionUpperRight, width, height))
        {
            neighbourGridNodeList.Add(gridNodeNativeArray[CalculateIndex(positionUpperRight, width)]);
        }

        return neighbourGridNodeList;
    }

    public static float2 CalculateVector(int fromX, int fromY, int toX, int toY)
    {
        return new float2(toX, toY) - new float2(fromX, fromY);
    }

    public static int CalculateIndex(int2 gridPosition, int width)
    {
        return CalculateIndex(gridPosition.x, gridPosition.y, width);
    }

    public static int CalculateIndex(int x,int y,int width)
    {
        return x + y * width;
    }

    public static float3 GetWorldPosition(int x,int y,float gridSize)
    {
        return new float3(x * gridSize, 0, y * gridSize);
    }

    public static float3 GetWorldCenterPosition(int x, int y, float gridSize)
    {
        return new float3(x * gridSize + gridSize * .5f, 0, y * gridSize + gridSize * .5f);
    }

    public static int2 GetGridPosition(float3 worldPosition, float gridSize)
    {
        return new int2
        (
            (int)math.floor(worldPosition.x / gridSize),
            (int)math.floor(worldPosition.z / gridSize)

        );
    }

    public static bool IsValidGridPosition(int2 gridPositon,int width,int height)
    {
        return gridPositon.x >= 0 && gridPositon.y >= 0 && gridPositon.x < width && gridPositon.y < height;
    }


    public static float3 GetWorldMovementVector(float2 vector)
    {
        return new float3(vector.x, 0, vector.y);
    }

    public static bool IsWall(GridNode gridNode)
    {
        return gridNode.m_Cost == WALL_COST;
    }
}
