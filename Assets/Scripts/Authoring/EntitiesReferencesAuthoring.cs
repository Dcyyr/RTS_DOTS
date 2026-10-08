using Unity.Entities;
using UnityEngine;

public class EntitiesReferencesAuthoring : MonoBehaviour
{

    public GameObject m_BulletPrefabs;
    public GameObject m_ZombiePrefab;
    public GameObject m_ShootingLight;
    public GameObject m_ScoutPrefab;
    public GameObject m_SoldierPrefab;

    public GameObject m_BuildingTowerPrefab;
    public GameObject m_BuildingBarracksPrefab;
    public GameObject m_BuilindGoldHarverserPrefab;
    public GameObject m_BuilindIronHarverserPrefab;
    public GameObject m_BuilindOilHarverserPrefab;

    public class Baker : Baker<EntitiesReferencesAuthoring>
    {
        public override void Bake(EntitiesReferencesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntitiesReferences
            {
                m_BulletPrefabs = GetEntity(authoring.m_BulletPrefabs, TransformUsageFlags.Dynamic),
                m_ZombiePrefab = GetEntity(authoring.m_ZombiePrefab, TransformUsageFlags.Dynamic),
                m_ShootingLightPrefab = GetEntity(authoring.m_ShootingLight, TransformUsageFlags.Dynamic),
                m_ScoutPrefab = GetEntity(authoring.m_ScoutPrefab, TransformUsageFlags.Dynamic),
                m_SoldierPrefab = GetEntity(authoring.m_SoldierPrefab, TransformUsageFlags.Dynamic),

                m_BuildingTowerPrefab = GetEntity(authoring.m_BuildingTowerPrefab, TransformUsageFlags.Dynamic),
                m_BuildingBarracksPrefab = GetEntity(authoring.m_BuildingBarracksPrefab, TransformUsageFlags.Dynamic),

                m_BuilindGoldHarverserPrefab = GetEntity(authoring.m_BuilindGoldHarverserPrefab, TransformUsageFlags.Dynamic),
                m_BuilindIronHarverserPrefab = GetEntity(authoring.m_BuilindIronHarverserPrefab, TransformUsageFlags.Dynamic),
                m_BuilindOilHarverserPrefab = GetEntity(authoring.m_BuilindOilHarverserPrefab, TransformUsageFlags.Dynamic),
            });
        }
        
    }
}

public struct EntitiesReferences : IComponentData
{
    public Entity m_BulletPrefabs;
    public Entity m_ZombiePrefab;
    public Entity m_ShootingLightPrefab;
    public Entity m_ScoutPrefab;
    public Entity m_SoldierPrefab;


    public Entity m_BuildingTowerPrefab;
    public Entity m_BuildingBarracksPrefab;
    public Entity m_BuilindGoldHarverserPrefab;
    public Entity m_BuilindIronHarverserPrefab;
    public Entity m_BuilindOilHarverserPrefab;
}

