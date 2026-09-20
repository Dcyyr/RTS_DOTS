using Unity.Entities;
using UnityEngine;
[CreateAssetMenu()]
public class UnitTypeSO : ScriptableObject
{
    public enum UnitType
    {
        None,
        Soldier,
        Scout,
        Zombie,
    }

    public UnitType m_UnitType;

    public float m_ProgressMax;

    public Entity GetPrefabEntities(EntitiesReferences entitiesReference)
    {
        switch(m_UnitType)
        {
            default:
                case UnitType.None:
                case UnitType.Soldier:  return entitiesReference.m_SoldierPrefab;
                case UnitType.Scout:    return entitiesReference.m_ScoutPrefab;
                case UnitType.Zombie:   return entitiesReference.m_ZombiePrefab;
        }
    }
}
