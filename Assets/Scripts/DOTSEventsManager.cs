using System;
using UnityEngine;

public class DOTSEventsManager : MonoBehaviour
{
    public static DOTSEventsManager instance { get; private set; }

    public event EventHandler OnHQDead;
    private void Awake()
    {
        instance = this;
    }

    public void TriggerOnHQDead()
    {
        OnHQDead?.Invoke(this, EventArgs.Empty);
    }

    
}
