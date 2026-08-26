using Unity.Entities;
using UnityEngine;

public class MoveSpeedAuthoring : MonoBehaviour
{
    public float m_MoveSpeed;

    public class Baker : Baker<MoveSpeedAuthoring>
    {
        public override void Bake(MoveSpeedAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MoveSpeed()
            {
                m_MoveSpeed = authoring.m_MoveSpeed
            });
                

            
        }
    }

   

}

public struct MoveSpeed : IComponentData
{
    public float m_MoveSpeed;
}
