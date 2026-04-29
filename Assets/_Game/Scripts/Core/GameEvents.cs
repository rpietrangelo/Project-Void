// All EventBus event types defined here. Add new events in this file only.

public record AuthStateChangedEvent(bool IsLoggedIn, string PlayFabId, string FirebaseUid);
public record FactionChosenEvent(FactionType Faction);
public record ResourceChangedEvent(string CurrencyCode, int NewAmount);
public record BuildingUpgradeStartedEvent(string BuildingId, int NewLevel, long FinishTimeUnix);
public record BuildingUpgradeCompleteEvent(string BuildingId, int NewLevel);
public record TileOccupiedEvent(int X, int Y, string OwnerId, string AllianceId);
public record CorruptionChangedEvent(int TileX, int TileY, float NewLevel, float ServerGlobalLevel);
public record PatronProgressChangedEvent(string PatronId, float ProgressPercent);
public record PatronAwakenedEvent(string PatronId);
public record AllianceJoinedEvent(string AllianceId, string AllianceName);
public record AllianceLeftEvent(string AllianceId);
public record MarchStartedEvent(string MarchId, int DestX, int DestY, long ArrivalTimeUnix);
public record MarchCompletedEvent(string MarchId, bool WasAttack, bool Victory);
public record HorrorEventTriggeredEvent(HorrorEventType EventType, float Intensity);
public record PvEMonsterSpawnedEvent(int TileX, int TileY, string MonsterId, int MonsterLevel);
public record PlayerPowerChangedEvent(long NewPower);
public record KvKStartedEvent(string OpponentServerId);
public record SeasonEndedEvent(int SeasonNumber, FactionType WinningFaction);
