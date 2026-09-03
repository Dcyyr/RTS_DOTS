using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ShootingAuthoring : MonoBehaviour
{
    public float m_MaxTimer;
    public float m_AttackDistance;
    public int m_ShootDamage;

    public Transform m_BulletTransform;
    public class Baker : Baker<ShootingAuthoring>
    {
        public override void Bake(ShootingAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Shooting
            {
                m_MaxTimer = authoring.m_MaxTimer,
                m_ShootDamage = authoring.m_ShootDamage,
                m_AttackDistance = authoring.m_AttackDistance,
                m_BulletTransform = authoring.m_BulletTransform.localPosition
            });
        }
    }

}

public struct Shooting : IComponentData
{
    public int m_ShootDamage;

    public float m_Timer;
    public float m_MaxTimer;
    public float m_AttackDistance;

    public float3 m_BulletTransform;
    //Event
    public OnShootEvent m_OnShoot;

    public struct OnShootEvent
    {
        public bool m_IsTriggered;
        public float3 m_ShootFromPosition;
    }

}
