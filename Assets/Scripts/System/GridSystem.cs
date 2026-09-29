#define GRID_DEBUG
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public partial struct GridSystem : ISystem
{
    public struct GridSystemData :IComponentData
    {
        public int m_Width;
        public int m_Height;
        public float m_GridNodeSize;
        public GridMap m_GridMap;

    }

    public struct GridMap
    {
        public NativeArray<Entity> m_GridEntityArray;
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


        GridMap gridMap = new GridMap();
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
                SystemAPI.SetComponent(gridMap.m_GridEntityArray[index],gridNode);
            }

        }


        state.EntityManager.AddComponent<GridSystemData>(state.SystemHandle);
        state.EntityManager.SetComponentData(state.SystemHandle, new GridSystemData
        {
            m_Width = width,
            m_Height = height,
            m_GridNodeSize = gridNodeSize,
            m_GridMap = gridMap,
        });


    }
#if !GRID_DEBUG
    [BurstCompile]
#endif
    public void OnUpdate(ref SystemState state)
    {
        GridSystemData gridSystemData = SystemAPI.GetComponent<GridSystemData>(state.SystemHandle);

        int2 targetGridPosition = new int2(2, 1);

        NativeArray<RefRW<GridNode>> gridNodeNativeArray = new NativeArray<RefRW<GridNode>>(gridSystemData.m_Width * gridSystemData.m_Height, Allocator.Temp);

        for (int x = 0; x < gridSystemData.m_Width; x++) 
        {
            for (int y = 0; y < gridSystemData.m_Height; y++) 
            {
                int index = CalculateIndex(x, y, gridSystemData.m_Width);
                Entity gridNodeEntity = gridSystemData.m_GridMap.m_GridEntityArray[index];

                RefRW<GridNode> gridNode = SystemAPI.GetComponentRW<GridNode>(gridNodeEntity);

                gridNode.ValueRW.m_Vector = new Vector2(0, 1);
                gridNodeNativeArray[index] = gridNode;

                if (x == targetGridPosition.x && y == targetGridPosition.y)
                {
                    gridNode.ValueRW.m_Cost = 0;
                    gridNode.ValueRW.m_BestCost = 0;
                }else
                {
                    gridNode.ValueRW.m_Cost = 1;
                    gridNode.ValueRW.m_BestCost = byte.MaxValue;
                }
            }
        }

        NativeQueue<RefRW<GridNode>> gridNodeQueue = new NativeQueue<RefRW<GridNode>>(Allocator.Temp);

        RefRW<GridNode> targetGridNode = gridNodeNativeArray[CalculateIndex(targetGridPosition.x, targetGridPosition.y, gridSystemData.m_Width)];
        gridNodeQueue.Enqueue(targetGridNode);


        int safety = 1000;//取决于width和height的乘积
        while(gridNodeQueue.Count >0)
        {
            safety--;
            if(safety < 0)
            {
                Debug.Log("Safety break");
                break;
            }

            RefRW<GridNode> currentGridNode = gridNodeQueue.Dequeue();

            NativeList<RefRW<GridNode>> neighbourGridNodeList = 
                GetNeighbourGridNodeList(currentGridNode, gridNodeNativeArray, gridSystemData.m_Width, gridSystemData.m_Height);

            foreach(RefRW<GridNode> neighbourGridNode in neighbourGridNodeList)
            {
                byte newBestCost = (byte)(neighbourGridNode.ValueRW.m_BestCost + neighbourGridNode.ValueRW.m_Cost);

                if(newBestCost < neighbourGridNode.ValueRO.m_BestCost)
                {
                    neighbourGridNode.ValueRW.m_BestCost = newBestCost;
                    neighbourGridNode.ValueRW.m_Vector =
                        CalculateVector(neighbourGridNode.ValueRO.x, neighbourGridNode.ValueRO.y, currentGridNode.ValueRO.x, currentGridNode.ValueRO.y);
                }

            }
                neighbourGridNodeList.Dispose();

            
        }

        gridNodeQueue.Dispose();
        gridNodeNativeArray.Dispose();

        if (Input.GetMouseButtonDown(0))
        {
            float3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();
            int2 mouseGridPosition = GetGridPosition(mouseWorldPosition, gridSystemData.m_GridNodeSize);
            if (IsValidGridPosition(mouseGridPosition, gridSystemData.m_Width, gridSystemData.m_Height))
            {
                int index = CalculateIndex(mouseGridPosition.x, mouseGridPosition.y, gridSystemData.m_Width);
                Entity entity = gridSystemData.m_GridMap.m_GridEntityArray[index];

                RefRW<GridNode> gridNode = SystemAPI.GetComponentRW<GridNode>(entity);
                Debug.Log(gridNode.ValueRO.m_Vector);
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
        gridSystemData.ValueRW.m_GridMap.m_GridEntityArray.Dispose();
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
}
