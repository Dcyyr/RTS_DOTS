using Unity.Entities;
using UnityEngine;

public class UnitAnimationAuthoring : MonoBehaviour
{
    public AnimationDataSO.AnimationType m_IdleAnimation;
    public AnimationDataSO.AnimationType m_WalkAnimation;
    public class Baker : Baker<UnitAnimationAuthoring>
    {
        public override void Bake(UnitAnimationAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitAnimation
            {
                m_IdleAnimation = authoring.m_IdleAnimation,
                m_WalkAnimation = authoring.m_WalkAnimation,
            });
        }
    }
}

public struct UnitAnimation : IComponentData
{
    public AnimationDataSO.AnimationType m_IdleAnimation;
    public AnimationDataSO.AnimationType m_WalkAnimation;
}
