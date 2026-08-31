using UnityEngine;

public class GameAssets : MonoBehaviour
{
    public static GameAssets Instance { get; private set; }

    public const int UNITS_LAYER = 6;
    
    private void Awake()
    {
        Instance = this;
    }
}
