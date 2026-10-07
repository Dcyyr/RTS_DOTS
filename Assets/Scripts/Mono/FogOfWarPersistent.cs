using UnityEngine;

public class FogOfWarPersistent : MonoBehaviour
{
    [SerializeField]
    private RenderTexture m_FogOfWarRenderTexture;
    [SerializeField]
    private RenderTexture m_FogOfWarRenderPersistentTexture;
    [SerializeField]
    private RenderTexture m_FogOfWarRenderPersistent2Texture;
    [SerializeField]
    private Material m_FogOfWarPersistentMaterial;

    private bool m_IsValid;

    private void Start()
    {
        Graphics.Blit(m_FogOfWarRenderTexture, m_FogOfWarRenderPersistentTexture);
        Graphics.Blit(m_FogOfWarRenderTexture, m_FogOfWarRenderPersistent2Texture);
    }

    private void Update()
    {
        
        Graphics.Blit(m_FogOfWarRenderTexture, m_FogOfWarRenderPersistentTexture, m_FogOfWarPersistentMaterial, 0);
        Graphics.CopyTexture(m_FogOfWarRenderPersistentTexture, m_FogOfWarRenderPersistent2Texture);
    }
}