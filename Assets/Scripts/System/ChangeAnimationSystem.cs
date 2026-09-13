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
        ChangeAnimationJob changeAnimationJob = new ChangeAnimationJob
        { 
            m_AnimationDataBlobArrayAssetReference = animationDataSet.m_AnimationDataBlobArrayAssetReference,
        
        };
        changeAnimationJob.ScheduleParallel();
    }


}


public partial struct ChangeAnimationJob : IJobEntity
{
    public BlobAssetReference<BlobArray<AnimationData>> m_AnimationDataBlobArrayAssetReference;


    public void Execute(ref ActiveAnimation activeAnimation, ref MaterialMeshInfo materialMeshInfo)
    {
        if (AnimationDataSO.IsAnimationUninterruptible(activeAnimation.m_AnimationType))
        {
            return;
        }

        

        if (activeAnimation.m_AnimationType != activeAnimation.m_NextAnimationType)
        {
            activeAnimation.m_Frame = 0;
            activeAnimation.m_FrameTimer = 0f;
            activeAnimation.m_AnimationType = activeAnimation.m_NextAnimationType;

            //改mesh
            ref AnimationData animationData = ref m_AnimationDataBlobArrayAssetReference.Value[(int)activeAnimation.m_AnimationType];
            materialMeshInfo.MeshID = animationData.m_BatchMeshIdBlobArray[0];

        }
    }

}



