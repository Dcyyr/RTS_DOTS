using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;
[UpdateBefore(typeof(ActiveAnimationSystem))]
partial struct ChangeAnimationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        AnimationDataSet animationDataSet = SystemAPI.GetSingleton<AnimationDataSet>();

        foreach ((RefRW<AcitveAnimation> activeAnimation, RefRW<MaterialMeshInfo> materialMeshInfo)
            in SystemAPI.Query<RefRW<AcitveAnimation>, RefRW<MaterialMeshInfo>>())
        {

            if (activeAnimation.ValueRO.m_AnimationType != activeAnimation.ValueRO.m_NextAnimationType)
            {
                activeAnimation.ValueRW.m_FrameTimer = 0f;
                activeAnimation.ValueRW.m_Frame = 0;
                activeAnimation.ValueRW.m_AnimationType = activeAnimation.ValueRO.m_NextAnimationType;

                //¸Ämesh
                ref AnimationData animationData = ref animationDataSet.m_AnimationDataBlobArrayAssetReference.Value[(int)activeAnimation.ValueRO.m_Frame];
                materialMeshInfo.ValueRW.MeshID = animationData.m_BatchMeshIdBlobArray[0];

            }
        }
    }


}
