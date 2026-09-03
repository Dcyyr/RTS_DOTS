using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class LoseTargetAuthoring : MonoBehaviour
{
    public float m_LoseTargetDistance;
    public class Baker : Baker<LoseTargetAuthoring>
    {
        public override void Bake(LoseTargetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new LoseTarget
            {
                m_LoseTargetDistance = authoring.m_LoseTargetDistance,
            });
        }
    }

}

public struct LoseTarget : IComponentData
{
    public float m_LoseTargetDistance;


}

