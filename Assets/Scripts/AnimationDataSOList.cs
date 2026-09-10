using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AnimationDataSOList : ScriptableObject
{
    public List<AnimationDataSO> m_AniamtionDataSOList;

    public AnimationDataSO GetAnimationDataSO(AnimationDataSO.AnimationType animationType)
    {
        foreach (AnimationDataSO animationDataSO in m_AniamtionDataSOList)
        {
            if (animationDataSO.m_AnimationType == animationType)
            {
                return animationDataSO;
            }
        }
        Debug.LogError("没有找到动画类型" + animationType);
        return null;
    }
}
