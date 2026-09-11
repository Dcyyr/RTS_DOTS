using Unity.Entities;
using UnityEngine;

public class AnimatedMeshAuthoring : MonoBehaviour
{
    public GameObject m_MeshGameObject;
    public class Baker : Baker<AnimatedMeshAuthoring>
    {
        public override void Bake(AnimatedMeshAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new AnimatedMesh
            {
                m_MeshEntity = GetEntity(authoring.m_MeshGameObject, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct AnimatedMesh :IComponentData
{
    public Entity m_MeshEntity;
}
