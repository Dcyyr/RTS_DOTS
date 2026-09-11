using Unity.Burst;
using Unity.Entities;

partial struct AnimationStateSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRW<AnimatedMesh> animatedMesh, RefRW<UnitMover> unitMover,RefRO<UnitAnimation> unitAnimation) in
            SystemAPI.Query<RefRW<AnimatedMesh>, RefRW<UnitMover>,RefRO<UnitAnimation>>())
        {

            RefRW<ActiveAnimation> activeAnimation = SystemAPI.GetComponentRW<ActiveAnimation>(animatedMesh.ValueRO.m_MeshEntity);
            if(unitMover.ValueRO.m_IsMoving)
            {
                activeAnimation.ValueRW.m_NextAnimationType = unitAnimation.ValueRO.m_WalkAnimation;
            }
            else
            {
                activeAnimation.ValueRW.m_NextAnimationType = unitAnimation.ValueRO.m_IdleAnimation;

            }
        }
    }

    
}
