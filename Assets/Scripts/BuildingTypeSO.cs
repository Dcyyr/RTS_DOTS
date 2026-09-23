using Unity.Entities;
using UnityEngine;
[CreateAssetMenu()]
public class BuildingTypeSO : ScriptableObject
{
    public enum BuildingType
    {
        None,
        ZombieSpawner,
        Tower,
        Barracks,
    }

    public BuildingType m_BuildingType;
    public Transform m_Prefab;

    public bool m_ShowInBuildingPlacementManagerUI;
    public Sprite m_Sprite;

    public Transform m_VisualPrefab;

    public bool IsNone()
    {
        return m_BuildingType == BuildingType.None;
    }

    public Entity GetPrefabEntity(EntitiesReferences entitiesReference)
    {
        switch (m_BuildingType)
        {
            default:
            case BuildingType.None:
            case BuildingType.Tower: return entitiesReference.m_BuildingTowerPrefab;
            case BuildingType.Barracks: return entitiesReference.m_BuildingBarracksPrefab;
        }
    }
}
