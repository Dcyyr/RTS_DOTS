using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingPlacementManager : MonoBehaviour
{
    public static BuildingPlacementManager Instance { get; private set; }

    public event EventHandler OnActiveBuildingTypeSOChanged;

    [SerializeField]
    private BuildingTypeSO m_BuildingTypeSO;
    [SerializeField]
    private UnityEngine.Material m_GhostMaterial;

    private Transform m_GhostTransform;

    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        //预建造的建筑透明视觉效果跟着鼠标走
        if(m_GhostTransform != null)
        {
            m_GhostTransform.position = MouseWorldPosition.Instance.GetPosition();
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            // 想确认点击是否被 UI 拦截：把下面这行的注释去掉，点击时会在 Console 打印
            // Debug.Log("点击被 UI 拦截（鼠标下有 Raycast Target 的 UI 元素）");
            return;
        }


        if (m_BuildingTypeSO.IsNone())
        {
            return;
        }

        if(Input.GetMouseButton(1))
        {
            SetActiveBuildingTypeSO(GameAssets.Instance.m_BuildingTypeSOList.m_None);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (CanPlaceBuilding())
            {
                Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

                EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(EntitiesReferences));

                EntitiesReferences entitiesRef = entityQuery.GetSingleton<EntitiesReferences>();

                Entity entity = entityManager.Instantiate(m_BuildingTypeSO.GetPrefabEntity(entitiesRef));
                entityManager.SetComponentData(entity, LocalTransform.FromPosition(mouseWorldPosition));
            }

        }
    }

    //建筑不能叠放
    private bool CanPlaceBuilding()
    {
        Vector3 mouseWorldPosition = MouseWorldPosition.Instance.GetPosition();
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));

        PhysicsWorldSingleton physicsWorldSingleton = entityQuery.GetSingleton<PhysicsWorldSingleton>();

        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        CollisionFilter collisionFilter = new CollisionFilter
        {
            BelongsTo = ~0u,
            CollidesWith = 1u << GameAssets.BUILDINGS_LAYER,
            GroupIndex = 0,
        };


        UnityEngine.BoxCollider boxCollider = m_BuildingTypeSO.m_Prefab.GetComponent<UnityEngine.BoxCollider>();

        float halfExtents = 2f;
        NativeList<DistanceHit> hitDistance = new NativeList<DistanceHit>(Allocator.Temp);
        if (collisionWorld.OverlapBox(mouseWorldPosition, Quaternion.identity, boxCollider.size * halfExtents, ref hitDistance, collisionFilter))
        {
            //此处不能放建筑
            return false;
        }

        return true;

    }

    public BuildingTypeSO GetActiveBuildingTypeSO()
    {
        return m_BuildingTypeSO;

    }

    public void SetActiveBuildingTypeSO(BuildingTypeSO buildingTypeSO)
    {
        m_BuildingTypeSO = buildingTypeSO;

        if(m_GhostTransform != null)
        {
            Destroy(m_GhostTransform.gameObject);
        }

        if(!buildingTypeSO.IsNone())
        {
            m_GhostTransform = Instantiate(buildingTypeSO.m_VisualPrefab);
            foreach(MeshRenderer meshRenderer in m_GhostTransform.GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.material = m_GhostMaterial;
            }
        }

        OnActiveBuildingTypeSOChanged?.Invoke(this, EventArgs.Empty);
    }

}