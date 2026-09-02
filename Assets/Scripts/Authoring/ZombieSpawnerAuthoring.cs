using Unity.Entities;
using UnityEngine;

public class ZombieSpawnerAuthoring : MonoBehaviour
{
    public float m_MaxTimer;

    public float m_DistanceMin;
    public float m_DistanceMax;
    public class Baker : Baker<ZombieSpawnerAuthoring>
    {
        public override void Bake(ZombieSpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ZombieSpawner
            {
                m_MaxTimer = authoring.m_MaxTimer,
                m_DistanceMin = authoring.m_DistanceMin,
                m_DistanceMax = authoring.m_DistanceMax
            });
        }
    }

}

public struct ZombieSpawner : IComponentData
{
    public float m_Timer;
    public float m_MaxTimer;

    public float m_DistanceMin;
    public float m_DistanceMax;

}

