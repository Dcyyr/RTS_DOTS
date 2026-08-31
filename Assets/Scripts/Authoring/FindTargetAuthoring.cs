using Unity.Entities;
using UnityEngine;

public class FindTargetAuthoring : MonoBehaviour
{
    public float m_Range;
    public Faction m_TargetFaction;
    public float m_MaxTimer;
    public class Baker : Baker<FindTargetAuthoring>
    {
     
        public override void Bake(FindTargetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new FindTarget
            {
                m_Range = authoring.m_Range,
                m_TargetFaction = authoring.m_TargetFaction,
                m_MaxTimer = authoring.m_MaxTimer,
              
            });
        }
    }
}

public struct FindTarget : IComponentData
{
    public float m_Range;
    public Faction m_TargetFaction;

    public float m_Timer;
    public float m_MaxTimer;
}
