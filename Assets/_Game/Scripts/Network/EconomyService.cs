using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// Server-authoritative economy. All building upgrades go through PlayFab CloudScript.
/// Replaces LocalCityState's local resource tracking after Phase 3 integration.
/// </summary>
public class EconomyService : MonoBehaviour
{
    private PlayFabService _playFab;
    private Dictionary<string, int> _currencies = new();

    private void Awake()
    {
        ServiceLocator.Instance.Register<EconomyService>(this);
    }

    private void Start()
    {
        _playFab = ServiceLocator.Instance.Get<PlayFabService>();
        EventBus.Subscribe<AuthStateChangedEvent>(OnAuthStateChanged);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<AuthStateChangedEvent>(OnAuthStateChanged);
    }

    private async void OnAuthStateChanged(AuthStateChangedEvent evt)
    {
        if (evt.IsLoggedIn)
            await RefreshCurrenciesAsync();
    }

    // ── Currency ────────────────────────────────────────────────────────────

    public async UniTask RefreshCurrenciesAsync()
    {
        _currencies = await _playFab.GetAllCurrenciesAsync();
        foreach (var kv in _currencies)
            EventBus.Publish(new ResourceChangedEvent(kv.Key, kv.Value));
    }

    public int GetCurrency(string currencyCode) =>
        _currencies.TryGetValue(currencyCode, out var val) ? val : 0;

    // ── Building Upgrades ───────────────────────────────────────────────────

    public async UniTask<(bool success, long finishTimeUnix, string error)>
        StartBuildingUpgradeAsync(string buildingId, int slotIndex, int targetLevel)
    {
        var args = new { buildingId, slotIndex, targetLevel };
        var (success, result, error) = await _playFab.CallCloudScriptAsync<StartUpgradeResult>(
            "StartBuildingUpgrade", args);

        if (!success || result == null)
            return (false, 0, error);

        if (!result.success)
            return (false, 0, result.error);

        // Deduct from local cache optimistically (server already deducted)
        await RefreshCurrenciesAsync();

        EventBus.Publish(new BuildingUpgradeStartedEvent(buildingId, targetLevel, result.finishTime / 1000));
        return (true, result.finishTime / 1000, null);
    }

    public async UniTask<(bool success, string error)> CompleteUpgradeAsync(int slotIndex)
    {
        var args = new { slotIndex };
        var (success, result, error) = await _playFab.CallCloudScriptAsync<CompleteUpgradeResult>(
            "CompleteUpgrade", args);

        if (!success || result == null)
            return (false, error);

        if (!result.success)
            return (false, result.error);

        EventBus.Publish(new BuildingUpgradeCompleteEvent(result.buildingId, result.newLevel));
        return (true, null);
    }

    public async UniTask<Dictionary<int, BuildingInstance>> GetPlayerBuildingsAsync()
    {
        var (success, result, error) = await _playFab.CallCloudScriptAsync<GetBuildingsResult>(
            "GetPlayerBuildings", new { });

        if (!success || result == null || !result.success)
        {
            Debug.LogWarning($"[EconomyService] GetPlayerBuildings failed: {error}");
            return new Dictionary<int, BuildingInstance>();
        }

        var buildings = new Dictionary<int, BuildingInstance>();
        foreach (var kv in result.buildings)
        {
            var raw = kv.Value;
            var inst = new BuildingInstance
            {
                buildingId = raw.buildingId,
                slotIndex = kv.Key,
                currentLevel = raw.level,
                state = raw.state == "upgrading" ? BuildingState.Upgrading :
                        raw.level > 0 ? BuildingState.Ready : BuildingState.Available,
                upgradeFinishTime = raw.upgradeFinishTime / 1000
            };
            buildings[kv.Key] = inst;
        }
        return buildings;
    }

    // ── Chests ──────────────────────────────────────────────────────────────

    public async UniTask<(bool success, List<string> grantedItems, string error)>
        OpenChestAsync(string chestItemId)
    {
        var args = new { chestItemId };
        var (success, result, error) = await _playFab.CallCloudScriptAsync<OpenChestResult>(
            "OpenChest", args);

        if (!success || result == null)
            return (false, null, error);

        if (!result.success)
            return (false, null, result.error);

        await RefreshCurrenciesAsync();
        return (true, result.grantedItems, null);
    }

    // ── Response Types ──────────────────────────────────────────────────────

    [Serializable]
    private class StartUpgradeResult
    {
        public bool success;
        public long finishTime;   // Unix ms
        public int costPG;
        public string error;
    }

    [Serializable]
    private class CompleteUpgradeResult
    {
        public bool success;
        public string buildingId;
        public int newLevel;
        public string error;
    }

    [Serializable]
    private class GetBuildingsResult
    {
        public bool success;
        public Dictionary<int, RawBuildingData> buildings;
    }

    [Serializable]
    private class RawBuildingData
    {
        public string buildingId;
        public int level;
        public string state;
        public long upgradeFinishTime;
        public int targetLevel;
    }

    [Serializable]
    private class OpenChestResult
    {
        public bool success;
        public List<string> grantedItems;
        public string error;
    }
}
