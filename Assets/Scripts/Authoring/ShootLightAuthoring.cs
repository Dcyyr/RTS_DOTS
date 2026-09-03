using Unity.Entities;
using UnityEngine;

public class ShootLightAuthoring : MonoBehaviour
{
    public float m_Timer;
    public class Baker : Baker<ShootLightAuthoring>
    {
        public override void Bake(ShootLightAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ShootLight
            {
                m_Timer = authoring.m_Timer
            });
        }
    }

}

public struct ShootLight : IComponentData
{

    public float m_Timer;

}


