/// <summary>
/// All user-facing strings. Never hardcode strings in UI components — reference from here.
/// </summary>
public static class LocalizationConstants
{
    // ── Auth ────────────────────────────────────────────────────────────────
    public const string BTN_SIGN_IN          = "Sign In";
    public const string BTN_CREATE_ACCOUNT   = "Create Account";
    public const string BTN_BACK             = "Back";
    public const string BTN_CREATE           = "Create";
    public const string BTN_CHOOSE           = "Choose";

    public const string LABEL_EMAIL          = "Email";
    public const string LABEL_PASSWORD       = "Password";
    public const string LABEL_CONFIRM_PW     = "Confirm Password";
    public const string LABEL_DISPLAY_NAME   = "Display Name";

    public const string TITLE_CHOOSE_FACTION = "Choose Your Path";
    public const string LOADING              = "Loading...";

    // Faction Names & Descriptions
    public const string FACTION_ORDER_NAME = "Order of the Pale Flame";
    public const string FACTION_ORDER_DESC =
        "Forge a bastion of civilization against the encroaching void. Discipline, faith, " +
        "and sacred fire are your weapons. Build towers of pale gold and hold the darkness at bay.";

    public const string FACTION_CULT_NAME = "The Unbound Cult";
    public const string FACTION_CULT_DESC =
        "Embrace the void. Bargain with eldritch patrons, corrupt the land, and unleash horrors " +
        "upon those who would oppose your ascension. Power is the only truth.";

    public const string FACTION_WANDERER_NAME = "The Wanderers";
    public const string FACTION_WANDERER_DESC =
        "Bound to no lord and no patron. You survive by cunning, moving between the cracks of " +
        "greater powers. Gather ancient knowledge and strike when the moment demands it.";

    // ── Auth Errors ─────────────────────────────────────────────────────────
    public const string ERR_INVALID_EMAIL       = "Please enter a valid email address.";
    public const string ERR_WEAK_PASSWORD       = "Password must be at least 6 characters.";
    public const string ERR_PASSWORDS_NO_MATCH  = "Passwords do not match.";
    public const string ERR_WRONG_CREDENTIALS   = "Incorrect email or password.";
    public const string ERR_NETWORK             = "Network error. Please check your connection.";
    public const string ERR_DISPLAY_NAME_EMPTY  = "Please enter a display name.";
    public const string ERR_DISPLAY_NAME_SHORT  = "Display name must be at least 3 characters.";
    public const string ERR_EMAIL_IN_USE        = "This email is already registered.";
    public const string ERR_GENERIC             = "Something went wrong. Please try again.";

    // ── City Scene ───────────────────────────────────────────────────────────
    public const string BTN_BUILD            = "Build";
    public const string BTN_UPGRADE          = "Upgrade";
    public const string BTN_SPEED_UP         = "Speed Up";
    public const string BTN_WORLD_MAP        = "World";
    public const string BTN_ALLIANCE         = "Alliance";
    public const string LABEL_COST           = "Cost";
    public const string LABEL_BUILD_TIME     = "Build Time";
    public const string LABEL_CURRENT_LEVEL  = "Level";
    public const string LABEL_UPGRADING      = "Upgrading...";
    public const string LABEL_MAX_LEVEL      = "Max Level";
    public const string ERR_CANNOT_AFFORD    = "Not enough resources.";
    public const string ERR_SLOT_OCCUPIED    = "Slot already occupied.";

    // ── Alliance ─────────────────────────────────────────────────────────────
    public const string TAB_OVERVIEW        = "Overview";
    public const string TAB_MEMBERS         = "Members";
    public const string TAB_CHAT            = "Chat";
    public const string TAB_SEARCH          = "Search";
    public const string BTN_JOIN            = "Join";
    public const string BTN_LEAVE           = "Leave";
    public const string BTN_DISBAND         = "Disband";
    public const string BTN_PROMOTE         = "Promote";
    public const string BTN_KICK            = "Kick";
    public const string BTN_SEND            = "Send";
    public const string BTN_CREATE_ALLIANCE = "Create Alliance";
    public const string LABEL_ONLINE        = "Online";
    public const string LABEL_OFFLINE       = "Offline";
    public const string LABEL_TOTAL_POWER   = "Total Power";
    public const string LABEL_MEMBERS       = "Members";
    public const string LABEL_RITUAL_PROG   = "Ritual Progress";
    public const string PLACEHOLDER_SEARCH  = "Search alliances...";
    public const string PLACEHOLDER_MESSAGE = "Type a message...";
    public const string ERR_ALLIANCE_NAME_TAKEN = "Alliance name already taken.";
    public const string ERR_POWER_TOO_LOW       = "Your power is too low to join this alliance.";
    public const string ERR_ALLIANCE_FULL       = "This alliance is full.";

    // ── World Map ────────────────────────────────────────────────────────────
    public const string BTN_VIEW_PROFILE    = "View Profile";
    public const string BTN_SCOUT          = "Scout";
    public const string BTN_ATTACK         = "Attack";
    public const string BTN_SEND_RESOURCES = "Send Resources";
    public const string BTN_MY_CITY        = "My City";
    public const string LABEL_MY_CITY      = "Home";

    // Tile type names
    public const string TILE_PLAINS        = "Plains";
    public const string TILE_FOREST        = "Forest";
    public const string TILE_MOUNTAIN      = "Mountain";
    public const string TILE_RUINS         = "Ancient Ruins";
    public const string TILE_VOID_RIFT     = "Void Rift";
    public const string TILE_IRON          = "Iron Deposit";
    public const string TILE_STONE         = "Stone Quarry";
    public const string TILE_VOID_STONE    = "Void Stone";
    public const string TILE_ANCIENT_TEXT  = "Ancient Texts";
}
