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

        foreach((RefRW<AcitveAnimation> activeAnimation,RefRW<MaterialMeshInfo> materialMeshInfo)
            in SystemAPI.Query<RefRW<AcitveAnimation>,RefRW<MaterialMeshInfo>>())
        {

            if(Input.GetKeyDown(KeyCode.Q))
            {
                activeAnimation.ValueRW.m_NextAnimationType = AnimationDataSO.AnimationType.SoldierIdle;
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                activeAnimation.ValueRW.m_NextAnimationType = AnimationDataSO.AnimationType.SoldierWalk;
            }

            ref AnimationData animationData = ref animationDataSet.m_AnimationDataBlobArrayAssetReference.Value[(int)activeAnimation.ValueRW.m_AnimationType];

            activeAnimation.ValueRW.m_FrameTimer += SystemAPI.Time.DeltaTime;
            if(activeAnimation.ValueRW.m_FrameTimer > animationData.m_FrameTimerMax)
            {
                activeAnimation.ValueRW.m_FrameTimer -= animationData.m_FrameTimerMax;
                activeAnimation.ValueRW.m_Frame = 
                    (activeAnimation.ValueRW.m_Frame + 1) % animationData.m_FrameMax;


                materialMeshInfo.ValueRW.MeshID = animationData.m_BatchMeshIdBlobArray[(int)activeAnimation.ValueRW.m_Frame];


            }
        }
    }

    
}