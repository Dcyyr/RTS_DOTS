using Unity.Entities;
using UnityEngine;

public class VisualUnderForOfWarAuthoring : MonoBehaviour
{

    public GameObject m_ParentGameObject;
    public float m_SphereCastSize;

    public class Baker : Baker<VisualUnderForOfWarAuthoring>
    {
        public override void Bake(VisualUnderForOfWarAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new VisualUnderForOfWar
            {
                m_IsVisible = true,
                m_ParentEntity = GetEntity(authoring.m_ParentGameObject, TransformUsageFlags.Dynamic),
                m_SphereCastSize = authoring.m_SphereCastSize,
            });
        }
    }
}

public struct VisualUnderForOfWar : IComponentData
{
    public bool m_IsVisible;
    public Entity m_ParentEntity;
    public float m_SphereCastSize;
}
