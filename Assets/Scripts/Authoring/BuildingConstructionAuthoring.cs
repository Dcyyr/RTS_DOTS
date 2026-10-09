using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BuildingConstructionAuthoring : MonoBehaviour
{
    public class Baker : Baker<BuildingConstructionAuthoring>
    {
        public override void Bake(BuildingConstructionAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BuildingConstruction());

        }
    }
}

public struct BuildingConstruction : IComponentData
{
    public float m_ConstructionTimer;
    public float m_ConstructionTimerMax;
    public float3 m_StartPosition;
    public float3 m_EndPosition;
    public BuildingTypeSO.BuildingType m_BuildingType;
    public Entity m_FinalPrefabEntity;
    public Entity m_VisualEntity;

}

