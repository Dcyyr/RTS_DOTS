using Unity.Entities;
using UnityEngine;

public class HealthBarAuthoring : MonoBehaviour
{
    public GameObject m_HealthBarEntity;
    public GameObject m_HealthEntity;

    public class Baker : Baker<HealthBarAuthoring>
    {
        public override void Bake(HealthBarAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new HealthBar
            {
                m_HealthBarEntity = GetEntity(authoring.m_HealthBarEntity, TransformUsageFlags.NonUniformScale),
                m_HealthEntity = GetEntity(authoring.m_HealthEntity, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct HealthBar :IComponentData
{
    public Entity m_HealthBarEntity;
    public Entity m_HealthEntity;
}