using Unity.Entities;
using UnityEngine;

public class ResourceTypeSOSetAuthoring : MonoBehaviour
{
    public ResourceTypeSO.ResourceType m_ResourceType;
    public class Baker : Baker<ResourceTypeSOSetAuthoring>
    {
        public override void Bake(ResourceTypeSOSetAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new ResourceTypeSOSet
            {
                m_ResourceType = authoring.m_ResourceType
            });
        }
    }
}

public struct ResourceTypeSOSet : IComponentData
{
    public ResourceTypeSO.ResourceType m_ResourceType;
}
