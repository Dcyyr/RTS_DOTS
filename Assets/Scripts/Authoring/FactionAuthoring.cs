using Unity.Entities;
using UnityEngine;

public class FactionAuthoring : MonoBehaviour
{
    public FactionType m_FactionType;

    public class Baker : Baker<FactionAuthoring>
    {
        public override void Bake(FactionAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Faction
            {
                m_FactionType = authoring.m_FactionType,
            });
        }
    }

}

public struct Faction : IComponentData
{
    public FactionType m_FactionType;
}
