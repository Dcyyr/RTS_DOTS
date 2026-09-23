using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacementManagerUI : MonoBehaviour
{
    [SerializeField]
    private RectTransform m_BuildingContainer;
    [SerializeField]
    private RectTransform m_Button;

    [SerializeField]
    private BuildingTypeSOList m_BuildingTypeSOList;

    private Dictionary<BuildingTypeSO, BuildingPlacementManagerUI_SingleButton1> m_BuildingTypeSODictionary;
    private void Awake()
    {
        m_Button.gameObject.SetActive(false);

        m_BuildingTypeSODictionary = new Dictionary<BuildingTypeSO, BuildingPlacementManagerUI_SingleButton1>();
        foreach (BuildingTypeSO buildingTypeSO in m_BuildingTypeSOList.m_BuildingTypeSOList)
        {
            if (!buildingTypeSO.m_ShowInBuildingPlacementManagerUI)
            {
                continue;
            }

            RectTransform buildingRectTransform = Instantiate(m_Button, m_BuildingContainer);
            buildingRectTransform.gameObject.SetActive(true);

            BuildingPlacementManagerUI_SingleButton1 buttonSingle = buildingRectTransform.GetComponent<BuildingPlacementManagerUI_SingleButton1>();
            m_BuildingTypeSODictionary[buildingTypeSO] = buttonSingle;
            buttonSingle.Setup(buildingTypeSO);
        }
    }

    private void Start()
    {
        BuildingPlacementManager.Instance.OnActiveBuildingTypeSOChanged += OnActiveBuildingTypeSOChanged;
        UpdateSelectedVisual();
    }

    private void OnActiveBuildingTypeSOChanged(object sender, System.EventArgs e)
    {
        UpdateSelectedVisual();
    }

    private void UpdateSelectedVisual()
    {   //hide all through
        foreach(BuildingTypeSO buildingTypeSO in m_BuildingTypeSODictionary.Keys)
        {
            m_BuildingTypeSODictionary[buildingTypeSO].HideSelected();
        }

        m_BuildingTypeSODictionary[BuildingPlacementManager.Instance.GetActiveBuildingTypeSO()].ShowSelected();
    }
}
