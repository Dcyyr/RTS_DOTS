using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class AcitveAnimationAuthoring : MonoBehaviour
{

    public AnimationDataSO m_SoldierIdle;

    public class Baker : Baker<AcitveAnimationAuthoring>
    {
        public override void Bake(AcitveAnimationAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            EntitiesGraphicsSystem entitiesGraphicsSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<EntitiesGraphicsSystem>();
            AddComponent(entity, new AcitveAnimation
            {
            });
        }
    }

}

public struct AcitveAnimation : IComponentData
{
    public float m_Frame;
    public float m_FrameTimer;

    public BlobAssetReference<AnimationInfo> m_AnimationInfoAssetReference;

}

