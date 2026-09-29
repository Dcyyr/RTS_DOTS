using Unity.Entities;
using UnityEngine;

public class GridSystemDebug : MonoBehaviour
{
    public static GridSystemDebug instance { get; private set; }

    [SerializeField]
    private Transform m_DebugPrefab;

    private GridSystemDebugPrefab[,] m_GridSystemDebugPrefabArray;

    private bool m_IsInit;

    private void Awake()
    {
        instance = this;
    }

    public void InitizlizeGrid(GridSystem.GridSystemData gridSystemData)
    {
        if (m_IsInit)
        {
            return;

        }

        m_IsInit = true;

        m_GridSystemDebugPrefabArray = new GridSystemDebugPrefab[gridSystemData.m_Width, gridSystemData.m_Height];
        for (int x = 0; x < gridSystemData.m_Width; x++)
        {
            for (int y = 0; y < gridSystemData.m_Height; y++)
            {
                Transform debugTransform = Instantiate(m_DebugPrefab);
                GridSystemDebugPrefab gridSystemDebugPrefab = debugTransform.GetComponent<GridSystemDebugPrefab>();
                gridSystemDebugPrefab.Setup(x, y, gridSystemData.m_GridNodeSize);

                m_GridSystemDebugPrefabArray[x, y] = gridSystemDebugPrefab;
            }
        }
    }

    public void UpdateGrid(GridSystem.GridSystemData gridSystemData)
    {
        for (int x = 0; x < gridSystemData.m_Width; x++)
        {
            for (int y = 0; y < gridSystemData.m_Height; y++)
            {
                GridSystemDebugPrefab gridSystemDebugPrefab = m_GridSystemDebugPrefabArray[x, y];

                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                int index = GridSystem.CalculateIndex(x, y,gridSystemData.m_Width);
                Entity gridNodeEntity = gridSystemData.m_GridMap.m_GridEntityArray[index];
                GridSystem.GridNode gridNode = entityManager.GetComponentData<GridSystem.GridNode>(gridNodeEntity);
                
                //gridSystemDebugPrefab.SetColor(gridNode.m_Data == 0 ? Color.white : Color.blue);
            }
        }


    }
}
