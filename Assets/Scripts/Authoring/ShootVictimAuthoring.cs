using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ShootVictimAuthoring : MonoBehaviour
{

    public Transform m_HitPosition;

    public class Baker : Baker<ShootVictimAuthoring>
    {
        public override void Bake(ShootVictimAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ShootVictim
            {

                m_HitPosition = authoring.m_HitPosition.localPosition
            });


        }
    }
}

public struct ShootVictim : IComponentData
{
    public float3 m_HitPosition;

}
