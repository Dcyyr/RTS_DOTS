using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

partial struct ActiveAnimationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimationData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        AnimationData animationData = SystemAPI.GetSingleton<AnimationData>();

        foreach((RefRW<AcitveAnimation> activeAnimation,RefRW<MaterialMeshInfo> materialMeshInfo)
            in SystemAPI.Query<RefRW<AcitveAnimation>,RefRW<MaterialMeshInfo>>())
        {

            //如果blobasset不存在
            if(!activeAnimation.ValueRO.m_AnimationInfoAssetReference.IsCreated)
            {   //默认idle
                activeAnimation.ValueRW.m_AnimationInfoAssetReference = animationData.m_SoldierIdle;
            }

            activeAnimation.ValueRW.m_FrameTimer += SystemAPI.Time.DeltaTime;
            if(activeAnimation.ValueRW.m_FrameTimer > activeAnimation.ValueRO.m_AnimationInfoAssetReference.Value.m_FrameTimerMax)
            {
                activeAnimation.ValueRW.m_FrameTimer -= activeAnimation.ValueRO.m_AnimationInfoAssetReference.Value.m_FrameTimerMax;
                activeAnimation.ValueRW.m_Frame = 
                    (activeAnimation.ValueRW.m_Frame + 1) % activeAnimation.ValueRO.m_AnimationInfoAssetReference.Value.m_FrameMax;


                materialMeshInfo.ValueRW.MeshID = activeAnimation.ValueRO.m_AnimationInfoAssetReference.Value.m_BatchMeshIdBlobArray[(int)activeAnimation.ValueRW.m_Frame];


            }
        }
    }

    
}
