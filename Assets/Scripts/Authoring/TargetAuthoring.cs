using Unity.Entities;
using UnityEngine;

public class TargetAuthoring : MonoBehaviour
{
    public GameObject m_TargetEntity;
    public class Baker : Baker<TargetAuthoring>
    {
        public override void Bake(TargetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Target
            {
                m_TargetEntity = GetEntity(authoring.m_TargetEntity, TransformUsageFlags.Dynamic)
            });
        }
    }

}

public struct Target : IComponentData
{
    public Entity m_TargetEntity;

}
