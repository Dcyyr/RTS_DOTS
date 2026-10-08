using UnityEngine;

public class GameAssets : MonoBehaviour
{
    public static GameAssets Instance { get; private set; }
    public const int DEFAULT_LAYER = 0;

    public const int UNITS_LAYER = 6;
    public const int BUILDINGS_LAYER = 7;
    public const int PATHFINDING_WALL = 8;
    public const int PATHFINDING_HEAVY = 9;
    public const int FOG_OF_WAR = 11;

    private void Awake()
    {
        Instance = this;
    }

    public UnitTypeSOList m_UnitTypeSOList;
    public BuildingTypeSOList m_BuildingTypeSOList;
}
