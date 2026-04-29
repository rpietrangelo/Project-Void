public static class GameConstants
{
    // PlayFab Currency Codes
    public const string CURRENCY_PALE_GOLD        = "PG";
    public const string CURRENCY_VOID_CRYSTALS    = "VC";
    public const string CURRENCY_ELDRITCH_ESSENCE = "EE";
    public const string CURRENCY_CORRUPTION_PTS   = "CP";
    public const string CURRENCY_FLAME_TOKENS     = "FT";
    public const string CURRENCY_WANDERER_MARKS   = "WM";

    // Firestore Collection Names
    public const string COL_PLAYERS     = "players";
    public const string COL_CITIES      = "cities";
    public const string COL_ALLIANCES   = "alliances";
    public const string COL_WORLD_TILES = "worldTiles";
    public const string COL_SERVERS     = "servers";
    public const string COL_CHAT        = "allianceChat";

    // Remote Config Keys
    public const string RC_RESOURCE_RATE_MULT  = "resource_gen_rate_multiplier";
    public const string RC_BUILD_TIME_MULT      = "building_time_multiplier";
    public const string RC_SHIELD_DURATION_H    = "pvp_shield_duration_hours";
    public const string RC_CORRUPTION_RATE      = "corruption_spread_rate";
    public const string RC_PATRON_THRESHOLD     = "patron_awakening_threshold";
    public const string RC_HORROR_EVENT_FREQ_H  = "horror_event_frequency_hours";

    // Game Design Constants (override via Remote Config in production)
    public const int   MAX_ALLIANCE_SIZE        = 50;
    public const int   CITY_BUILDING_SLOTS      = 25;
    public const int   MAX_MARCH_SLOTS          = 5;
    public const int   DAILY_LOGIN_CHAIN_LENGTH = 30;
    public const int   VOID_CHEST_PITY_COUNTER  = 60;
    public const float HORROR_TRIGGER_THRESHOLD = 0.8f;
    public const int   MAP_WIDTH               = 500;
    public const int   MAP_HEIGHT              = 500;
}
