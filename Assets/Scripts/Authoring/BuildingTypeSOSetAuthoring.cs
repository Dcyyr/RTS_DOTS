using Unity.Entities;
using UnityEngine;

public class BuildingTypeSOSetAuthoring : MonoBehaviour
{
    public BuildingTypeSO.BuildingType m_BuildingType;

    public class Baker : Baker<BuildingTypeSOSetAuthoring>
    {
        public override void Bake(BuildingTypeSOSetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BuildingTypeSOSet
            {
                m_BuildingType = authoring.m_BuildingType,
            });
        }
    }
}


public struct BuildingTypeSOSet : IComponentData
{
    public BuildingTypeSO.BuildingType m_BuildingType;
}
