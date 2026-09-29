using UnityEngine;

public class GameAssets : MonoBehaviour
{
    public static GameAssets Instance { get; private set; }

    public const int UNITS_LAYER = 6;
    public const int BUILDINGS_LAYER = 7;
    public const int PATHFINDING_WALL = 8;

    private void Awake()
    {
        Instance = this;
    }

    public UnitTypeSOList m_UnitTypeSOList;
    public BuildingTypeSOList m_BuildingTypeSOList;
}
