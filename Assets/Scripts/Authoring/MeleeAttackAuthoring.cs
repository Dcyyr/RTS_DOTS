using Unity.Entities;
using UnityEngine;

public class MeleeAttackAuthoring : MonoBehaviour
{

    public float m_MaxTimer;
    public int m_Damage;
    public float m_ColliderSize;

    public class Baker : Baker<MeleeAttackAuthoring>
    {
        public override void Bake(MeleeAttackAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MeleeAttack
            {
                m_MaxTimer = authoring.m_MaxTimer,
                m_Damage = authoring.m_Damage,
                m_ColliderSize = authoring.m_ColliderSize,
            });
        }
    }
}

public struct MeleeAttack : IComponentData
{
    public float m_Timer;   
    public float m_MaxTimer;

    public int m_Damage;

    public float m_ColliderSize;
}

