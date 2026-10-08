using Mono.Cecil;
using Unity.Entities;
using UnityEngine;

public class BuildingHarversterAuthoring : MonoBehaviour
{

    public float m_HarversterMaxTimer;
    public ResourceTypeSO.ResourceType m_ResourceType;
    public class Baker : Baker<BuildingHarversterAuthoring>
   {
      public override void Bake(BuildingHarversterAuthoring authoring)
      {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BuildingHarverster
            {
                m_HarversterMaxTimer = authoring.m_HarversterMaxTimer,
                m_ResourceType = authoring.m_ResourceType,
            });
      }
    }
}

public struct BuildingHarverster : IComponentData
{
    public float m_HarversterTimer;
    public float m_HarversterMaxTimer;
    public ResourceTypeSO.ResourceType m_ResourceType;
}
