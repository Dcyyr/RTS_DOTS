using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public class AcitveAnimationAuthoring : MonoBehaviour
{

    public AnimationDataSO.AnimationType m_NextAnimationType;

    public class Baker : Baker<AcitveAnimationAuthoring>
    {
        public override void Bake(AcitveAnimationAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            EntitiesGraphicsSystem entitiesGraphicsSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EntitiesGraphicsSystem>();
            AddComponent(entity, new ActiveAnimation
            {
                m_NextAnimationType = authoring.m_NextAnimationType,
            });
        }
    }

}

public struct ActiveAnimation : IComponentData
{
    public float m_Frame;
    public float m_FrameTimer;

    public AnimationDataSO.AnimationType m_AnimationType;
    public AnimationDataSO.AnimationType m_NextAnimationType;


}

