using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{

    public static ResourceManager Instance { get; private set; }
    [SerializeField]
    private ResourceTypeSOList m_ResourceTypeListSO;

    public event EventHandler OnResourceAmountChanged;


    public Dictionary<ResourceTypeSO.ResourceType, int> m_ResourceTypeAmountDictionary;
    private void Awake()
    {
        Instance = this;
        m_ResourceTypeAmountDictionary = new Dictionary<ResourceTypeSO.ResourceType, int>();

        if (m_ResourceTypeListSO == null)
        {
            return;
        }

        foreach(ResourceTypeSO resourceTypeSO in m_ResourceTypeListSO.m_ResourceTypeSOList)
        {   //初始化
            m_ResourceTypeAmountDictionary[resourceTypeSO.m_ResourceType] = 0;
        }

        AddResourceAmount(ResourceTypeSO.ResourceType.Gold, 200);
        AddResourceAmount(ResourceTypeSO.ResourceType.Iron, 150);
        AddResourceAmount(ResourceTypeSO.ResourceType.Oil, 50);

    }

    public void AddResourceAmount(ResourceTypeSO.ResourceType resourceTypeSO,int amount)
    {
        if (m_ResourceTypeAmountDictionary == null)
        {
            m_ResourceTypeAmountDictionary = new Dictionary<ResourceTypeSO.ResourceType, int>();
        }

        m_ResourceTypeAmountDictionary[resourceTypeSO] += amount;

        OnResourceAmountChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetResourceAmount(ResourceTypeSO.ResourceType resourceTypeSO)
    {
        if (m_ResourceTypeAmountDictionary == null || !m_ResourceTypeAmountDictionary.ContainsKey(resourceTypeSO))
        {
            return 0;
        }
        return m_ResourceTypeAmountDictionary[resourceTypeSO];
    }

    public bool CanSpendResourceAmount(ResourceAmount resourceAmount)
    {
        return m_ResourceTypeAmountDictionary[resourceAmount.m_ResourceType] >= resourceAmount.m_Amount;
    }

    public bool CanSpendResourceAmount(ResourceAmount[] resourceAmountArray)
    {
        foreach (ResourceAmount resourceAmount in resourceAmountArray)
        {
            if(m_ResourceTypeAmountDictionary[resourceAmount.m_ResourceType] < resourceAmount.m_Amount)
            {
                return false;
            }
        }

        return true;
    }

    public void SpendResourceAmount(ResourceAmount resourceAmount)
    {
        m_ResourceTypeAmountDictionary[resourceAmount.m_ResourceType] -= resourceAmount.m_Amount;
        OnResourceAmountChanged?.Invoke(this, EventArgs.Empty);
    }


    public void SpendResourceAmount(ResourceAmount[] resourceAmountArray)
    {
        foreach(ResourceAmount resourceAmount in resourceAmountArray)
        {
            m_ResourceTypeAmountDictionary[resourceAmount.m_ResourceType] -= resourceAmount.m_Amount;

        }
        OnResourceAmountChanged?.Invoke(this, EventArgs.Empty);
    }
}