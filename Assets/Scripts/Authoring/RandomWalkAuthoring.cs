using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class RandomWalkAuthoring : MonoBehaviour
{

    public float3 m_TargetPosition;
    public float3 m_OriginPosition;
    public float m_DistanceMin;
    public float m_DistanceMax;

    public uint m_RandomSeed;

    public class Baker : Baker<RandomWalkAuthoring>
    {
        public override void Bake(RandomWalkAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new RandomWalk
            {

                m_TargetPosition = authoring.m_TargetPosition,
                m_OriginPosition = authoring.m_OriginPosition,
                m_DistanceMin = authoring.m_DistanceMin,
                m_DistanceMax = authoring.m_DistanceMax,

                m_Random = new Unity.Mathematics.Random(authoring.m_RandomSeed)
            });
        }
    }
}

public struct RandomWalk : IComponentData
{
    public float3 m_TargetPosition;
    public float3 m_OriginPosition;
    public float m_DistanceMin;
    public float m_DistanceMax;

    public Unity.Mathematics.Random m_Random;


}

