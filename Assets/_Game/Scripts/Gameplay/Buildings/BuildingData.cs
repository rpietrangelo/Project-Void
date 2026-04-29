using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewBuilding", menuName = "ElDom/Building Data")]
public class BuildingData : ScriptableObject
{
    [Header("Identity")]
    public string buildingId;
    public string displayName;
    [TextArea(2, 4)] public string loreText;
    public FactionType faction;          // Order | Cult | Neutral (both factions, use Unset)

    [Header("Unlock")]
    public int cityHallLevelRequired;
    public bool isUniquePerCity;

    [Header("Levels")]
    public List<BuildingLevel> levels;   // index 0 = level 1

    [Header("Visuals")]
    public Sprite[] levelSprites;
    public GameObject worldPrefab;
}

[System.Serializable]
public class BuildingLevel
{
    public int level;
    public float buildTimeSeconds;
    public List<ResourceCost> buildCost;
    public List<ResourceProduction> hourlyProduction;
    public float defenseBonus;
    public int powerContribution;
    public int troopCapacityBonus;
}

[System.Serializable]
public class ResourceCost
{
    public string currencyCode;   // matches GameConstants currency codes
    public long amount;
}

[System.Serializable]
public class ResourceProduction
{
    public string currencyCode;
    public long amountPerHour;
}
