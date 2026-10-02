using Unity.Entities;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GridSystemDebug : MonoBehaviour
{
    public static GridSystemDebug instance { get; private set; }

    [SerializeField]
    private Transform m_DebugPrefab;

    [SerializeField]
    private Sprite m_ArrowSprite;
    [SerializeField]
    private Sprite m_CriclePrefab;

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
                int gridIndex = gridSystemData.m_NextGridIndex - 1;
                if(gridIndex < 0)
                {
                    gridIndex = 0;
                }
                Entity gridNodeEntity = gridSystemData.m_GridMapArray[gridIndex].m_GridEntityArray[index];
                GridSystem.GridNode gridNode = entityManager.GetComponentData<GridSystem.GridNode>(gridNodeEntity);
                
                if(gridNode.m_Cost == 0)
                {
                    gridSystemDebugPrefab.SetSprite(m_CriclePrefab);
                    gridSystemDebugPrefab.SetColor(Color.green);
                }
                else
                {
                    if(gridNode.m_Cost == GridSystem.WALL_COST)
                    {
                        gridSystemDebugPrefab.SetSprite(m_CriclePrefab);
                        gridSystemDebugPrefab.SetColor(Color.black);
                    }
                    else
                    {
                        gridSystemDebugPrefab.SetSprite(m_ArrowSprite);
                        gridSystemDebugPrefab.SetColor(Color.white);
                        // 箭头朝向 = 流场向量 m_Vector（不是节点坐标 x！）
                        gridSystemDebugPrefab.SetSpriteRotation(Quaternion.LookRotation(new float3(gridNode.m_Vector.x, 0, gridNode.m_Vector.y), Vector3.up));
                    }
                }
               

                //gridSystemDebugPrefab.SetColor(gridNode.m_Data == 0 ? Color.white : Color.blue);
            }
        }


    }
}
