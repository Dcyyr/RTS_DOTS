using JetBrains.Annotations;
using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

partial struct ActiveAnimationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimationDataSet>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        AnimationDataSet animationDataSet = SystemAPI.GetSingleton<AnimationDataSet>();

        ActiveAnimationJob activeAnimationJob = new ActiveAnimationJob
        {

            deltaTime = SystemAPI.Time.DeltaTime,
            m_AnimationDataBlobArrayAssetReference = animationDataSet.m_AnimationDataBlobArrayAssetReference

        };

        activeAnimationJob.ScheduleParallel();


    }

    
}

public partial struct ActiveAnimationJob :IJobEntity
{
    public float deltaTime;
    public BlobAssetReference<BlobArray<AnimationData>> m_AnimationDataBlobArrayAssetReference;
    public void Execute(ref ActiveAnimation activeAnimation, ref MaterialMeshInfo materialMeshInfo)
    {


        ref AnimationData animationData = ref m_AnimationDataBlobArrayAssetReference.Value[(int)activeAnimation.m_AnimationType];

        activeAnimation.m_FrameTimer += deltaTime;
        if (activeAnimation.m_FrameTimer > animationData.m_FrameTimerMax)
        {
            activeAnimation.m_FrameTimer -= animationData.m_FrameTimerMax;
            activeAnimation.m_Frame =
                (activeAnimation.m_Frame + 1) % animationData.m_FrameMax;


            materialMeshInfo.MeshID = animationData.m_BatchMeshIdBlobArray[(int)activeAnimation.m_Frame];


            if (activeAnimation.m_Frame == 0 && AnimationDataSO.IsAnimationUninterruptible(activeAnimation.m_AnimationType))
            {
                activeAnimation.m_AnimationType = AnimationDataSO.AnimationType.None;
            }

        }
    }
}
