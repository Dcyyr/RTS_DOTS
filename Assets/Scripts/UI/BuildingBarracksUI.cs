using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class BuildingBarracksUI : MonoBehaviour
{
    [SerializeField]
    private Button m_SoldierButton;
    [SerializeField]
    private Button m_ScoutButton;
    [SerializeField]
    private Image m_ProgressImage;
    [SerializeField]
    private RectTransform m_UnitQueueContainer;
    [SerializeField]
    private RectTransform m_UnitQueue;


    private Entity m_BarracksEntity;
    private EntityManager m_EntityManager;
    private void Awake()
    {

        m_SoldierButton.onClick.AddListener(() =>
        {

            DynamicBuffer<SpawnUnitTypeBuffer> spawnUnitTypeBuffer = m_EntityManager.GetBuffer<SpawnUnitTypeBuffer>(m_BarracksEntity, false);
            spawnUnitTypeBuffer.Add(new SpawnUnitTypeBuffer
            {
                m_UnitType = UnitTypeSO.UnitType.Soldier,
            });
        });

        m_ScoutButton.onClick.AddListener(() =>
        {

            DynamicBuffer<SpawnUnitTypeBuffer> spawnUnitTypeBuffer = m_EntityManager.GetBuffer<SpawnUnitTypeBuffer>(m_BarracksEntity, false);
            spawnUnitTypeBuffer.Add(new SpawnUnitTypeBuffer
            {
                m_UnitType = UnitTypeSO.UnitType.Scout,
            });
        });

        m_UnitQueue.gameObject.SetActive(false);

    }
    private void Start()
    {
        m_EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        UnitSelectionManager.Instance.OnSelectedEntitiesChanged += OnSelectedEntitiesChanged;
        Hide();
    }

    private void Update()
    {
        UpdateProgressVisual();
        UpdateUnitQueueVisual();
    }

    private void OnSelectedEntitiesChanged(object sender, System.EventArgs e)
    {
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Selected, BuildingBarracks>().Build(m_EntityManager);

        NativeArray<Entity> entityArray = entityQuery.ToEntityArray(Allocator.Temp);

        if(entityArray.Length >0)
        {
            m_BarracksEntity = entityArray[0];

            Show();

        }
        else
        {
            m_BarracksEntity = Entity.Null;
            Hide();
        }
    }

    private void UpdateProgressVisual()
    {
        if(m_BarracksEntity == Entity.Null)
        {
            m_ProgressImage.fillAmount = 0; 
            return;
        }

        BuildingBarracks buildingBarracks = m_EntityManager.GetComponentData<BuildingBarracks>(m_BarracksEntity);

        if(buildingBarracks.m_ActiveUnitType == UnitTypeSO.UnitType.None)
        {
            m_ProgressImage.fillAmount = 0;
        }
        else
        {
            m_ProgressImage.fillAmount = buildingBarracks.m_CreateTimer / buildingBarracks.m_CreateMaxTimer;

        }
    }

    private void UpdateUnitQueueVisual()
    {
        //«Â¿Ìchild
        foreach(Transform child in m_UnitQueueContainer)
        {
            if(child == m_UnitQueue)
            {
                continue;
            }
            Destroy(child.gameObject);
        }

        DynamicBuffer<SpawnUnitTypeBuffer> spawnUnitTypeBuffer = m_EntityManager.GetBuffer<SpawnUnitTypeBuffer>(m_BarracksEntity, false);

        foreach(SpawnUnitTypeBuffer buffer in spawnUnitTypeBuffer)
        {
            RectTransform unitQueueTransform = Instantiate(m_UnitQueue, m_UnitQueueContainer);
            unitQueueTransform.gameObject.SetActive(true);

            UnitTypeSO unitTypeSO = GameAssets.Instance.m_UnitTypeSOList.GetUnitTypeSO(buffer.m_UnitType);
            unitQueueTransform.GetComponent<Image>().sprite = unitTypeSO.m_Sprite;
        }

    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);

    }
}
