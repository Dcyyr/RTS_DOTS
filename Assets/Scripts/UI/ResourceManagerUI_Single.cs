using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceManagerUI_Single : MonoBehaviour
{
    [SerializeField]
    private Image m_Image;
    [SerializeField]
    private TextMeshProUGUI m_TextMesh;

    private void Awake()
    {
        
    }

    public void Setup(ResourceTypeSO resourceTypeSO)
    {
        m_Image.sprite = resourceTypeSO.m_Sprite;
        m_TextMesh.text = "0";
    
    }

    public void UpdateAmount(int amount)
    {
        m_TextMesh.text = amount.ToString();
    }

}
