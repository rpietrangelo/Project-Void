public enum FactionType
{
    Unset = 0,
    Order = 1,
    Cult = 2,
    Wanderer = 3
}

public enum HorrorEventType
{
    UIGlitch,
    CorruptionSpread,
    PatronWhisper,
    PatronAwakening,
    VoidRiftOpened
}

public enum BuildingState
{
    Locked,
    Available,
    Building,
    Upgrading,
    Ready
}

public enum TileType
{
    Plains,
    Forest,
    Mountain,
    Ruins,
    VoidRift,
    ResourceNode_Iron,
    ResourceNode_Stone,
    ResourceNode_VoidStone,
    ResourceNode_AncientText
}

public enum MarchType
{
    Gather,
    AttackPlayer,
    AttackMonster,
    Reinforce,
    Scout
}

public enum ResearchCategory
{
    Military,
    Architecture,
    Arcane,           // Order only
    ForbiddenRituals, // Cult only
    VoidArchitecture, // Cult only
    Survival          // Wanderer + shared
}
