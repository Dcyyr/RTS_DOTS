using Unity.Burst;
using Unity.Entities;

[UpdateAfter(typeof(ShootingSystem))]
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




        foreach ((RefRW<AnimatedMesh> animatedMesh, RefRW<Shooting> shooting, RefRO<UnitAnimation> unitAnimation, RefRO<UnitMover> unitMover,RefRO<Target> target) in
           SystemAPI.Query<RefRW<AnimatedMesh>, RefRW<Shooting>, RefRO<UnitAnimation>,RefRO<UnitMover>,RefRO<Target>>())
        {

            if(!unitMover.ValueRO.m_IsMoving && target.ValueRO.m_TargetEntity != Entity.Null)
            {
                RefRW<ActiveAnimation> activeAnimation = SystemAPI.GetComponentRW<ActiveAnimation>(animatedMesh.ValueRO.m_MeshEntity);
                activeAnimation.ValueRW.m_NextAnimationType = unitAnimation.ValueRO.m_AimAnimation;
            }

            if(shooting.ValueRO.m_OnShoot.m_IsTriggered)
            {
                RefRW<ActiveAnimation> activeAnimation = SystemAPI.GetComponentRW<ActiveAnimation>(animatedMesh.ValueRO.m_MeshEntity);
                activeAnimation.ValueRW.m_NextAnimationType = unitAnimation.ValueRO.m_AttackAnimation;
            }
            
            
        }

        foreach ((RefRW<AnimatedMesh> animatedMesh, RefRW<MeleeAttack> meleeAttack, RefRO<UnitAnimation> unitAnimation) in
           SystemAPI.Query<RefRW<AnimatedMesh>, RefRW<MeleeAttack>, RefRO<UnitAnimation>>())
        {
            if(meleeAttack.ValueRO.OnAttacked)
            {
                RefRW<ActiveAnimation> activeAnimation = SystemAPI.GetComponentRW<ActiveAnimation>(animatedMesh.ValueRO.m_MeshEntity);
                activeAnimation.ValueRW.m_NextAnimationType = unitAnimation.ValueRO.m_MeleeAttackAnimation;

            }
            
        }
    }

    
}
