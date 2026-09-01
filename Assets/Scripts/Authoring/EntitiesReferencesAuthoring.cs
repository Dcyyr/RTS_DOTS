using Unity.Entities;
using UnityEngine;

public class EntitiesReferencesAuthoring : MonoBehaviour
{

    public GameObject m_BulletPrefabs;
    public class Baker : Baker<EntitiesReferencesAuthoring>
    {
        public override void Bake(EntitiesReferencesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntitiesReferences
            {
                m_BulletPrefabs = GetEntity(authoring.m_BulletPrefabs, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct EntitiesReferences : IComponentData
{
    public Entity m_BulletPrefabs;
}

