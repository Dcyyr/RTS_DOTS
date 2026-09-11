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

        foreach((RefRW<ActiveAnimation> activeAnimation,RefRW<MaterialMeshInfo> materialMeshInfo)
            in SystemAPI.Query<RefRW<ActiveAnimation>,RefRW<MaterialMeshInfo>>())
        {

           

            ref AnimationData animationData = ref animationDataSet.m_AnimationDataBlobArrayAssetReference.Value[(int)activeAnimation.ValueRW.m_AnimationType];

            activeAnimation.ValueRW.m_FrameTimer += SystemAPI.Time.DeltaTime;
            if(activeAnimation.ValueRW.m_FrameTimer > animationData.m_FrameTimerMax)
            {
                activeAnimation.ValueRW.m_FrameTimer -= animationData.m_FrameTimerMax;
                activeAnimation.ValueRW.m_Frame = 
                    (activeAnimation.ValueRW.m_Frame + 1) % animationData.m_FrameMax;


                materialMeshInfo.ValueRW.MeshID = animationData.m_BatchMeshIdBlobArray[(int)activeAnimation.ValueRW.m_Frame];


                if(activeAnimation.ValueRO.m_Frame == 0 && activeAnimation.ValueRO.m_AnimationType == AnimationDataSO.AnimationType.SoldierAttack)
                {
                    activeAnimation.ValueRW.m_AnimationType = AnimationDataSO.AnimationType.None;
                }

                if (activeAnimation.ValueRO.m_Frame == 0 && activeAnimation.ValueRO.m_AnimationType == AnimationDataSO.AnimationType.ZombieMeleeAttack)
                {
                    activeAnimation.ValueRW.m_AnimationType = AnimationDataSO.AnimationType.None;
                }
            }


        }
    }

    
}