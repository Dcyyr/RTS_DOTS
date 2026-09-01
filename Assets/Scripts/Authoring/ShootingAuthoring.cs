using Unity.Entities;
using UnityEngine;

public class ShootingAuthoring : MonoBehaviour
{
    public float m_MaxTimer;
    public int m_ShootDamage;
    public class Baker : Baker<ShootingAuthoring>
    {
        public override void Bake(ShootingAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Shooting
            {
                m_MaxTimer = authoring.m_MaxTimer,
                m_ShootDamage = authoring.m_ShootDamage
            });
        }
    }

}

public struct Shooting : IComponentData
{
    public float m_Timer;
    public float m_MaxTimer;

    public int m_ShootDamage;

}
