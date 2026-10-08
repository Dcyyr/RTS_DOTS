using System.Collections.Generic;
using UnityEngine;

public class ResourceManagerUI : MonoBehaviour
{
    [SerializeField]
    private Transform m_Container;
    [SerializeField]
    private Transform m_Template;
    [SerializeField]
    private ResourceTypeSOList m_ResourceTypeSOList;
    public Dictionary<ResourceTypeSO.ResourceType, ResourceManagerUI_Single> m_ResourceManagerUISingleDictionary;
    private void Awake()
    {
        m_Template.gameObject.SetActive(false);
    }

    private void Start()
    {
        ResourceManager.Instance.OnResourceAmountChanged += OnResourceAmountChanged; ;
        Setup();
        UpdateAmounts();
    }

    private void OnResourceAmountChanged(object sender, System.EventArgs e)
    {
        UpdateAmounts();
    }

    private void Setup()
    {
        foreach(Transform child in m_Container)
        {
            if(child == m_Template)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
        m_ResourceManagerUISingleDictionary = new Dictionary<ResourceTypeSO.ResourceType, ResourceManagerUI_Single>();
        foreach (ResourceTypeSO resourceTypeSO in m_ResourceTypeSOList.m_ResourceTypeSOList)
        {
            Transform resourceTypeTransform = Instantiate(m_Template, m_Container);
            resourceTypeTransform.gameObject.SetActive(true);

            ResourceManagerUI_Single resourceManagerUISingle = resourceTypeTransform.GetComponent<ResourceManagerUI_Single>();
            resourceManagerUISingle.Setup(resourceTypeSO);

            m_ResourceManagerUISingleDictionary[resourceTypeSO.m_ResourceType] = resourceManagerUISingle;
        }
    }

    private void UpdateAmounts()
    {
        foreach (ResourceTypeSO resourceTypeSO in m_ResourceTypeSOList.m_ResourceTypeSOList)
        {
            m_ResourceManagerUISingleDictionary[resourceTypeSO.m_ResourceType]
                .UpdateAmount(ResourceManager.Instance.GetResourceAmount(resourceTypeSO.m_ResourceType));

        }
    }
}
