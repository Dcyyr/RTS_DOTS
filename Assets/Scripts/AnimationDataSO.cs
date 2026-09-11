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
    }

    public AnimationType m_AnimationType;
    public Mesh[] m_MeshArray;

    public float m_FrameTimerMax;
}
