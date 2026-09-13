using UnityEngine;

[CreateAssetMenu()]
public class AnimationDataSO : ScriptableObject
{
    public enum AnimationType
    {
        None = 0,
        SoldierIdle,
        SoldierWalk,
        ZombieIdle,
        ZombieWalk,
        SoldierAim,
        SoldierShoot,
        ZombieMeleeAttack,
        ScoutIdle,
        ScoutWalk,
        ScoutShoot,
        ScoutAim,
        
    }

    public AnimationType m_AnimationType;
    public Mesh[] m_MeshArray;

    public float m_FrameTimerMax;

    public static bool IsAnimationUninterruptible(AnimationType animationType)
    {
        switch (animationType)
        {
            default:return false;
            case AnimationType.SoldierShoot:
            case AnimationType.ScoutShoot:
            case AnimationType.ZombieMeleeAttack:
            return true;
        }
    }
}
