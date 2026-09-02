using Unity.Entities;
using UnityEngine;

public class HealthAuthoring : MonoBehaviour
{
    public int m_Health;
    public int m_MaxHealth;

    public class Baker : Baker<HealthAuthoring>
    {
        public override void Bake(HealthAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Health
            {
                m_Health = authoring.m_Health,
                m_MaxHealth = authoring.m_MaxHealth,
                m_OnHealthChanged = true,
            });
        }
    }


}


public struct Health : IComponentData
{
    public int m_Health;
    public int m_MaxHealth;

    public bool m_OnHealthChanged;

}
