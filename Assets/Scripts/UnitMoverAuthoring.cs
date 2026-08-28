using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class UnitMoverAuthoring : MonoBehaviour
{
    public float m_MoveSpeed;
    public float m_RotateSpeed = 10f;

    public class Baker : Baker<UnitMoverAuthoring>
    {
        public override void Bake(UnitMoverAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitMover()
            {
                m_MoveSpeed = authoring.m_MoveSpeed,
                m_RotateSpeed = authoring.m_RotateSpeed,
                m_TargetPosition = authoring.transform.position
            });
                

            
        }
    }

}

public struct UnitMover : IComponentData
{
    public float m_MoveSpeed;
    public float m_RotateSpeed;

    public float3 m_TargetPosition;
}
