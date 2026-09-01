using Unity.Entities;
using UnityEngine;

public class BulletAuthoring : MonoBehaviour
{

    public float m_Speed;
    public int m_Damage;
    public class Baker : Baker<BulletAuthoring>
    {
        public override void Bake(BulletAuthoring bulletAuthoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Bullet
            {
                m_Speed = bulletAuthoring.m_Speed,
                m_Damage = bulletAuthoring.m_Damage
            });
        }
    }
}

public struct Bullet : IComponentData
{
    public float m_Speed;
    public int m_Damage;
}
