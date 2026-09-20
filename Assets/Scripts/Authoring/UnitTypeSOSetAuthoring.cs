using Unity.Entities;
using UnityEngine;

public class UnitTypeSOSetAuthoring : MonoBehaviour
{
    public UnitTypeSO.UnitType m_UnitType;
    public class Baker : Baker<UnitTypeSOSetAuthoring>
    {
        public override void Bake(UnitTypeSOSetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitTypeSOSet
            {
                m_UnitType = authoring.m_UnitType,
            });
        }
    }
}

public struct UnitTypeSOSet :IComponentData
{
    public UnitTypeSO.UnitType m_UnitType;
}