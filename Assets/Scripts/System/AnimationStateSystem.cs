using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateAfter(typeof(ShootingSystem))]
partial struct AnimationStateSystem : ISystem
{
    private ComponentLookup<ActiveAnimation> m_ActiveAnimationComponentLookUp;


    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_ActiveAnimationComponentLookUp = state.GetComponentLookup<ActiveAnimation>(false);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //
        m_ActiveAnimationComponentLookUp.Update(ref state);
        IdleWalkAnimationStateJob idleWalkAnimationStateJob = new IdleWalkAnimationStateJob
        { 
            activeAnimationComponentLookUp = m_ActiveAnimationComponentLookUp,
        };

        idleWalkAnimationStateJob.ScheduleParallel();


        //
        m_ActiveAnimationComponentLookUp.Update(ref state);
        AimShootAnimationStateJob aimShootAnimationStateJob = new AimShootAnimationStateJob
        {
            activeAnimationComponentLookUp = m_ActiveAnimationComponentLookUp,
        };
        aimShootAnimationStateJob.ScheduleParallel();

        //

        m_ActiveAnimationComponentLookUp.Update(ref state);
        MeleeAttackAnimationStateJob meleeAttackAnimationStateJob = new MeleeAttackAnimationStateJob
        {
            activeAnimationComponentLookUp = m_ActiveAnimationComponentLookUp,
        };
        meleeAttackAnimationStateJob.ScheduleParallel();

     
    }


}

//NativeDisableParallelForRestriction对应的animatedMesh.m_MeshEntity，可以各写各的互不干扰
[BurstCompile]
public partial struct IdleWalkAnimationStateJob : IJobEntity
{
    [NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> activeAnimationComponentLookUp;
    public void Execute( in AnimatedMesh animatedMesh,in UnitMover unitMover,in UnitAnimation unitAnimation)
    {


        RefRW<ActiveAnimation> activeAnimation = activeAnimationComponentLookUp.GetRefRW(animatedMesh.m_MeshEntity);
        if (unitMover.m_IsMoving)
        {
            activeAnimation.ValueRW.m_NextAnimationType = unitAnimation.m_WalkAnimation;
        }
        else
        {
            activeAnimation.ValueRW.m_NextAnimationType = unitAnimation.m_IdleAnimation;

        }
    }

}
[BurstCompile]
public partial struct AimShootAnimationStateJob : IJobEntity
{
    [NativeDisableParallelForRestriction]public ComponentLookup<ActiveAnimation> activeAnimationComponentLookUp;

    public void Execute(in AnimatedMesh animatedMesh, in Shooting shooting,in UnitAnimation unitAnimation,in UnitMover unitMover,in Target target)
    {
        if (!unitMover.m_IsMoving && target.m_TargetEntity != Entity.Null)
        {
            RefRW<ActiveAnimation> activeAnimation = activeAnimationComponentLookUp.GetRefRW(animatedMesh.m_MeshEntity);
            activeAnimation.ValueRW.m_NextAnimationType = unitAnimation.m_AimAnimation;
        }

        if (shooting.m_OnShoot.m_IsTriggered)
        {
            RefRW<ActiveAnimation> activeAnimation = activeAnimationComponentLookUp.GetRefRW(animatedMesh.m_MeshEntity);
            activeAnimation.ValueRW.m_NextAnimationType = unitAnimation.m_AttackAnimation;
        }
    }

}

[BurstCompile]
public partial struct MeleeAttackAnimationStateJob : IJobEntity
{
    [NativeDisableParallelForRestriction] public ComponentLookup<ActiveAnimation> activeAnimationComponentLookUp;

    public void Execute(in AnimatedMesh animatedMesh, in MeleeAttack meleeAttack, in UnitAnimation unitAnimation)
    {
        if (meleeAttack.OnAttacked)
        {
            RefRW<ActiveAnimation> activeAnimation = activeAnimationComponentLookUp.GetRefRW(animatedMesh.m_MeshEntity);
            activeAnimation.ValueRW.m_NextAnimationType = unitAnimation.m_MeleeAttackAnimation;

        }

    }

}




