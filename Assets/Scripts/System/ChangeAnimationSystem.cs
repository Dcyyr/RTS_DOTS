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

        foreach ((RefRW<ActiveAnimation> activeAnimation, RefRW<MaterialMeshInfo> materialMeshInfo)
            in SystemAPI.Query<RefRW<ActiveAnimation>, RefRW<MaterialMeshInfo>>())
        {

            if(activeAnimation.ValueRO.m_AnimationType == AnimationDataSO.AnimationType.SoldierAttack)
            {
                continue;
            }

            if (activeAnimation.ValueRO.m_AnimationType == AnimationDataSO.AnimationType.ZombieMeleeAttack)
            {
                continue;
            }

            if (activeAnimation.ValueRO.m_AnimationType != activeAnimation.ValueRO.m_NextAnimationType)
            {
                activeAnimation.ValueRW.m_Frame = 0;
                activeAnimation.ValueRW.m_FrameTimer = 0f;
                activeAnimation.ValueRW.m_AnimationType = activeAnimation.ValueRO.m_NextAnimationType;

                //改mesh
                ref AnimationData animationData = ref animationDataSet.m_AnimationDataBlobArrayAssetReference.Value[(int)activeAnimation.ValueRW.m_AnimationType];
                materialMeshInfo.ValueRW.MeshID = animationData.m_BatchMeshIdBlobArray[0];

            }
        }
    }


}
