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


        //foreach ((RefRW<ActiveAnimation> activeAnimation,RefRW<MaterialMeshInfo> materialMeshInfo)
        //    in SystemAPI.Query<RefRW<ActiveAnimation>,RefRW<MaterialMeshInfo>>())
        //{

           

        //    ref AnimationData animationData = ref animationDataSet.m_AnimationDataBlobArrayAssetReference.Value[(int)activeAnimation.ValueRW.m_AnimationType];

        //    activeAnimation.ValueRW.m_FrameTimer += SystemAPI.Time.DeltaTime;
        //    if(activeAnimation.ValueRW.m_FrameTimer > animationData.m_FrameTimerMax)
        //    {
        //        activeAnimation.ValueRW.m_FrameTimer -= animationData.m_FrameTimerMax;
        //        activeAnimation.ValueRW.m_Frame = 
        //            (activeAnimation.ValueRW.m_Frame + 1) % animationData.m_FrameMax;


        //        materialMeshInfo.ValueRW.MeshID = animationData.m_BatchMeshIdBlobArray[(int)activeAnimation.ValueRW.m_Frame];


        //        if(activeAnimation.ValueRO.m_Frame == 0 && activeAnimation.ValueRO.m_AnimationType == AnimationDataSO.AnimationType.SoldierAttack)
        //        {
        //            activeAnimation.ValueRW.m_AnimationType = AnimationDataSO.AnimationType.None;
        //        }

        //        if (activeAnimation.ValueRO.m_Frame == 0 && activeAnimation.ValueRO.m_AnimationType == AnimationDataSO.AnimationType.ZombieMeleeAttack)
        //        {
        //            activeAnimation.ValueRW.m_AnimationType = AnimationDataSO.AnimationType.None;
        //        }
        //    }


        //}
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


            if (activeAnimation.m_Frame == 0 && activeAnimation.m_AnimationType == AnimationDataSO.AnimationType.SoldierAttack)
            {
                activeAnimation.m_AnimationType = AnimationDataSO.AnimationType.None;
            }

            if (activeAnimation.m_Frame == 0 && activeAnimation.m_AnimationType == AnimationDataSO.AnimationType.ZombieMeleeAttack)
            {
                activeAnimation.m_AnimationType = AnimationDataSO.AnimationType.None;
            }
        }
    }
}
