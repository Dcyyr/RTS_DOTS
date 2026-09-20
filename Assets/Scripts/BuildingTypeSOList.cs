using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BuildingTypeSOList : ScriptableObject
{
    public List<BuildingTypeSO> m_BuildingTypeSOList;

    public BuildingTypeSO GetBuildingTypeSO(BuildingTypeSO.BuildingType buildingType)
    {
        foreach(BuildingTypeSO buildingTypeSO in m_BuildingTypeSOList)
        {
            if(buildingTypeSO.m_BuildingType == buildingType)
            {  return buildingTypeSO; }
        }


        return null;
    }
}
