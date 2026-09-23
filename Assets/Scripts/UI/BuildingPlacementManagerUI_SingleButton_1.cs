using UnityEngine;
using UnityEngine.UI;

public class BuildingPlacementManagerUI_SingleButton1 : MonoBehaviour
{
    [SerializeField]
    private Image m_Image;
    [SerializeField]
    private Image m_SelectedImage;

    private BuildingTypeSO m_BuildingType;
    public void Setup(BuildingTypeSO buildingTypeSO)
    {
        m_BuildingType = buildingTypeSO;

        GetComponent<Button>().onClick.AddListener(() =>
        {
            BuildingPlacementManager.Instance.SetActiveBuildingTypeSO(buildingTypeSO);
        });

        m_Image.sprite = buildingTypeSO.m_Sprite;
    }

    public void ShowSelected()
    {
        m_SelectedImage.enabled = true;
    }

    public void HideSelected()
    {
        m_SelectedImage.enabled = false;

    }
}
