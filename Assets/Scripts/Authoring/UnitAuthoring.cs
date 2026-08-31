using Unity.Entities;
using UnityEngine;

public class UnitAuthoring : MonoBehaviour
{
    public Faction m_Faction;
    public class Baker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity,new Unit
            {
                m_Faction = authoring.m_Faction
            });
        }

    }

}


public struct Unit : IComponentData
{
    public Faction m_Faction;

}

