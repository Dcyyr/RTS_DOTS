using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

partial struct GridSystem : ISystem
{
    public struct GridSystemData :IComponentData
    {
        public int m_Width;
        public int m_Heigth;
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

    [BurstCompile]
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
                state.EntityManager.SetName(gridMap.m_GridEntityArray[index], "GridNode" + x + "_" + y);
                SystemAPI.SetComponent(gridMap.m_GridEntityArray[index],gridNode);
            }

        }


        state.EntityManager.AddComponent<GridSystemData>(state.SystemHandle);
        state.EntityManager.SetComponentData(state.SystemHandle, new GridSystemData
        {
            m_Width = width,
            m_Heigth = height,
            m_GridNodeSize = gridNodeSize,
            m_GridMap = gridMap,
        });


    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        GridSystemData gridSystemData = SystemAPI.GetComponent<GridSystemData>(state.SystemHandle);

        if(Input.GetKeyDown(KeyCode.Q))
        {
            int index = CalculateIndex(3, 2, gridSystemData.m_Width);
            Entity entity = gridSystemData.m_GridMap.m_GridEntityArray[index];

            RefRW<GridNode> gridNode = SystemAPI.GetComponentRW<GridNode>(entity);
            gridNode.ValueRW.m_Data = 1;
        }
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
}
