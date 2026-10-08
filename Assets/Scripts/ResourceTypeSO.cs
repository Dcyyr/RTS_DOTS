using UnityEngine;

[CreateAssetMenu()]
public class ResourceTypeSO : ScriptableObject
{
    public enum ResourceType
    {
        None,
        Gold,
        Iron,
        Oil,
    }

    public ResourceType m_ResourceType;
    public Sprite m_Sprite;
}
