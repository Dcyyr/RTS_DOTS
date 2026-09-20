using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu()]
public class UnitTypeSOList : ScriptableObject
{
    public List<UnitTypeSO> m_UnitTypeSOList;


    public UnitTypeSO GetUnitTypeSO(UnitTypeSO.UnitType unitType)
    {

        foreach(UnitTypeSO unitTypeSO in m_UnitTypeSOList)
        {
            if(unitTypeSO.m_UnitType == unitType)
            {
                return unitTypeSO;
            }
        }

        return null;
    }


}
