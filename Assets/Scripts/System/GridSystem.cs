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
        public byte m_Data;
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

        if(Input.GetMouseButtonDown(0))
        {
            float3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();
            int2 mouseGridPosition = GetGridPosition(mouseWorldPosition, gridSystemData.m_GridNodeSize);
            if (IsValidGridPosition(mouseGridPosition, gridSystemData.m_Width, gridSystemData.m_Height))
            {
                int index = CalculateIndex(mouseGridPosition.x, mouseGridPosition.y, gridSystemData.m_Width);
                Entity entity = gridSystemData.m_GridMap.m_GridEntityArray[index];

                RefRW<GridNode> gridNode = SystemAPI.GetComponentRW<GridNode>(entity);
                gridNode.ValueRW.m_Data = 1;
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
