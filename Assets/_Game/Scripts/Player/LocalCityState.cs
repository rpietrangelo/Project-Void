using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// Manages city building slot state.
/// Phase 2: fully local (PlayerPrefs-backed).
/// Phase 3: wires EconomyService — call SetEconomyService() after login.
/// </summary>
public class LocalCityState : MonoBehaviour
{
    private const string PREFS_KEY = "city_buildings";

    private Dictionary<int, BuildingInstance> _slots = new();
    private Dictionary<string, BuildingData> _buildingDataMap = new();
    private Dictionary<string, int> _localCurrencies = new();
    private EconomyService _economyService; // null in Phase 2, set in Phase 3

    public void Initialize(BuildingData[] allBuildings)
    {
        foreach (var bd in allBuildings)
            _buildingDataMap[bd.buildingId] = bd;

        LoadFromPrefs();

        // Seed local resources for Phase 2 testing
        if (_economyService == null)
        {
            _localCurrencies[GameConstants.CURRENCY_PALE_GOLD] = 5000;
            _localCurrencies[GameConstants.CURRENCY_VOID_CRYSTALS] = 0;
        }
    }

    public void SetEconomyService(EconomyService svc) => _economyService = svc;

    // ── Building Operations ──────────────────────────────────────────────────

    public void PlaceBuilding(string buildingId, int slotIndex)
    {
        if (_slots.ContainsKey(slotIndex))
        {
            Debug.LogWarning($"[LocalCityState] Slot {slotIndex} already occupied.");
            return;
        }
        _slots[slotIndex] = new BuildingInstance
        {
            buildingId = buildingId,
            slotIndex = slotIndex,
            currentLevel = 0,
            state = BuildingState.Available,
            upgradeFinishTime = 0
        };
        SaveToPrefs();
    }

    public async UniTask<bool> StartUpgradeAsync(int slotIndex)
    {
        if (!_slots.TryGetValue(slotIndex, out var inst))
        {
            Debug.LogWarning($"[LocalCityState] No building at slot {slotIndex}");
            return false;
        }

        if (inst.state == BuildingState.Upgrading)
        {
            Debug.LogWarning($"[LocalCityState] Already upgrading slot {slotIndex}");
            return false;
        }

        int targetLevel = inst.currentLevel + 1;

        if (!_buildingDataMap.TryGetValue(inst.buildingId, out var data))
        {
            Debug.LogError($"[LocalCityState] BuildingData not found for {inst.buildingId}");
            return false;
        }

        if (targetLevel > data.levels.Count)
        {
            Debug.LogWarning($"[LocalCityState] {inst.buildingId} already at max level");
            return false;
        }

        var levelData = data.levels[targetLevel - 1];

        if (_economyService != null)
        {
            // Phase 3: server-authoritative path
            var (success, finishTimeUnix, error) = await _economyService.StartBuildingUpgradeAsync(
                inst.buildingId, slotIndex, targetLevel);

            if (!success)
            {
                Debug.LogError($"[LocalCityState] Server upgrade failed: {error}");
                return false;
            }

            inst.state = BuildingState.Upgrading;
            inst.upgradeFinishTime = finishTimeUnix;
            SaveToPrefs();
            WatchUpgradeTimer(slotIndex, finishTimeUnix).Forget();
            return true;
        }
        else
        {
            // Phase 2: local path
            if (!CanAfford(levelData))
            {
                Debug.LogWarning("[LocalCityState] Cannot afford upgrade.");
                return false;
            }

            DeductLocalCost(levelData);

            long finishTimeUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (long)levelData.buildTimeSeconds;
            inst.state = BuildingState.Upgrading;
            inst.upgradeFinishTime = finishTimeUnix;
            SaveToPrefs();

            EventBus.Publish(new BuildingUpgradeStartedEvent(inst.buildingId, targetLevel, finishTimeUnix));
            WatchUpgradeTimer(slotIndex, finishTimeUnix).Forget();
            return true;
        }
    }

    private async UniTaskVoid WatchUpgradeTimer(int slotIndex, long finishTimeUnix)
    {
        long nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long remainMs = (finishTimeUnix - nowUnix) * 1000;

        if (remainMs > 0)
            await UniTask.Delay((int)remainMs);

        await CollectUpgradeAsync(slotIndex);
    }

    public async UniTask CollectUpgradeAsync(int slotIndex)
    {
        if (!_slots.TryGetValue(slotIndex, out var inst))
            return;

        if (inst.state != BuildingState.Upgrading)
            return;

        if (_economyService != null)
        {
            var (success, error) = await _economyService.CompleteUpgradeAsync(slotIndex);
            if (!success)
            {
                Debug.LogError($"[LocalCityState] Server complete upgrade failed: {error}");
                return;
            }
        }

        inst.currentLevel += 1;
        inst.state = BuildingState.Ready;
        inst.upgradeFinishTime = 0;
        SaveToPrefs();

        EventBus.Publish(new BuildingUpgradeCompleteEvent(inst.buildingId, inst.currentLevel));
    }

    // ── Queries ──────────────────────────────────────────────────────────────

    public BuildingInstance GetBuildingAt(int slotIndex) =>
        _slots.TryGetValue(slotIndex, out var inst) ? inst : null;

    public List<BuildingInstance> GetAllBuildings() => _slots.Values.ToList();

    public bool CanAfford(BuildingLevel level)
    {
        if (_economyService != null)
        {
            return level.buildCost.All(c =>
                _economyService.GetCurrency(c.currencyCode) >= c.amount);
        }
        return level.buildCost.All(c =>
            _localCurrencies.TryGetValue(c.currencyCode, out var have) && have >= c.amount);
    }

    // ── Local Currency (Phase 2 only) ─────────────────────────────────────────

    public int GetLocalCurrency(string code) =>
        _localCurrencies.TryGetValue(code, out var v) ? v : 0;

    private void DeductLocalCost(BuildingLevel level)
    {
        foreach (var cost in level.buildCost)
        {
            if (_localCurrencies.ContainsKey(cost.currencyCode))
                _localCurrencies[cost.currencyCode] -= (int)cost.amount;
            EventBus.Publish(new ResourceChangedEvent(cost.currencyCode,
                GetLocalCurrency(cost.currencyCode)));
        }
    }

    // ── Persistence ──────────────────────────────────────────────────────────

    private void SaveToPrefs()
    {
        var list = _slots.Values.ToList();
        PlayerPrefs.SetString(PREFS_KEY, UnityEngine.JsonUtility.ToJson(new SerializableSlotList { slots = list }));
        PlayerPrefs.Save();
    }

    private void LoadFromPrefs()
    {
        var json = PlayerPrefs.GetString(PREFS_KEY, null);
        if (string.IsNullOrEmpty(json)) return;

        try
        {
            var parsed = UnityEngine.JsonUtility.FromJson<SerializableSlotList>(json);
            if (parsed?.slots == null) return;
            foreach (var inst in parsed.slots)
                _slots[inst.slotIndex] = inst;

            // Resume any in-progress timers
            foreach (var inst in _slots.Values)
            {
                if (inst.state == BuildingState.Upgrading && inst.upgradeFinishTime > 0)
                    WatchUpgradeTimer(inst.slotIndex, inst.upgradeFinishTime).Forget();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LocalCityState] Failed to load from prefs: {ex.Message}");
        }
    }

    // ── Override with server data (Phase 3) ──────────────────────────────────

    public void OverrideWithServerData(Dictionary<int, BuildingInstance> serverSlots)
    {
        _slots = serverSlots;
        SaveToPrefs();

        foreach (var inst in _slots.Values)
        {
            if (inst.state == BuildingState.Upgrading && inst.upgradeFinishTime > 0)
                WatchUpgradeTimer(inst.slotIndex, inst.upgradeFinishTime).Forget();
        }
    }

    [Serializable]
    private class SerializableSlotList
    {
        public List<BuildingInstance> slots;
    }
}

[Serializable]
public class BuildingInstance
{
    public string buildingId;
    public int slotIndex;
    public int currentLevel;       // 0 = placed but not built
    public BuildingState state;
    public long upgradeFinishTime; // Unix seconds, 0 if not upgrading
}
