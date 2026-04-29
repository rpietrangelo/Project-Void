using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;

public class PlayFabService : MonoBehaviour
{
    public bool IsLoggedIn { get; private set; }
    public string PlayFabId { get; private set; }

    private Dictionary<string, int> _cachedCurrencies = new();

    private void Awake()
    {
        ServiceLocator.Instance.Register<PlayFabService>(this);
    }

    // ── Auth ────────────────────────────────────────────────────────────────

    public async UniTask<(bool success, string error)> LoginWithFirebaseAsync(string firebaseToken)
    {
        var tcs = new UniTaskCompletionSource<(bool, string)>();

        var request = new LoginWithCustomIDRequest
        {
            CustomId = firebaseToken,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetUserVirtualCurrency = true
            }
        };

        PlayFabClientAPI.LoginWithCustomID(request,
            result =>
            {
                PlayFabId = result.PlayFabId;
                IsLoggedIn = true;

                // Cache initial currency values
                if (result.InfoResultPayload?.UserVirtualCurrency != null)
                {
                    foreach (var kv in result.InfoResultPayload.UserVirtualCurrency)
                    {
                        _cachedCurrencies[kv.Key] = kv.Value;
                        EventBus.Publish(new ResourceChangedEvent(kv.Key, kv.Value));
                    }
                }

                Debug.Log($"[PlayFabService] Logged in: {PlayFabId}");
                tcs.TrySetResult((true, null));
            },
            error =>
            {
                Debug.LogError($"[PlayFabService] Login error: {error.GenerateErrorReport()}");
                tcs.TrySetResult((false, error.GenerateErrorReport()));
            });

        return await tcs.Task;
    }

    // ── Economy ─────────────────────────────────────────────────────────────

    public async UniTask<Dictionary<string, int>> GetAllCurrenciesAsync()
    {
        var tcs = new UniTaskCompletionSource<Dictionary<string, int>>();

        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
            result =>
            {
                _cachedCurrencies.Clear();
                foreach (var kv in result.VirtualCurrency)
                {
                    _cachedCurrencies[kv.Key] = kv.Value;
                    EventBus.Publish(new ResourceChangedEvent(kv.Key, kv.Value));
                }
                tcs.TrySetResult(new Dictionary<string, int>(_cachedCurrencies));
            },
            error =>
            {
                Debug.LogError($"[PlayFabService] GetCurrencies error: {error.GenerateErrorReport()}");
                tcs.TrySetResult(new Dictionary<string, int>(_cachedCurrencies));
            });

        return await tcs.Task;
    }

    public async UniTask<int> GetCurrencyAsync(string currencyCode)
    {
        var all = await GetAllCurrenciesAsync();
        return all.TryGetValue(currencyCode, out var val) ? val : 0;
    }

    public int GetCachedCurrency(string currencyCode) =>
        _cachedCurrencies.TryGetValue(currencyCode, out var val) ? val : 0;

    public async UniTask<(bool success, string error)> CallCloudScriptAsync(string functionName, object args)
    {
        var tcs = new UniTaskCompletionSource<(bool, string)>();

        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = functionName,
            FunctionParameter = args,
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(request,
            result =>
            {
                if (result.Error != null)
                {
                    Debug.LogError($"[PlayFabService] CloudScript error: {result.Error.Message}");
                    tcs.TrySetResult((false, result.Error.Message));
                    return;
                }
                tcs.TrySetResult((true, JsonConvert.SerializeObject(result.FunctionResult)));
            },
            error =>
            {
                Debug.LogError($"[PlayFabService] CloudScript call error: {error.GenerateErrorReport()}");
                tcs.TrySetResult((false, error.GenerateErrorReport()));
            });

        return await tcs.Task;
    }

    // Calls CloudScript and returns the deserialized result object
    public async UniTask<(bool success, T result, string error)> CallCloudScriptAsync<T>(string functionName, object args)
    {
        var tcs = new UniTaskCompletionSource<(bool, T, string)>();

        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = functionName,
            FunctionParameter = args,
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(request,
            result =>
            {
                if (result.Error != null)
                {
                    tcs.TrySetResult((false, default, result.Error.Message));
                    return;
                }
                try
                {
                    var json = JsonConvert.SerializeObject(result.FunctionResult);
                    var parsed = JsonConvert.DeserializeObject<T>(json);
                    tcs.TrySetResult((true, parsed, null));
                }
                catch (Exception ex)
                {
                    tcs.TrySetResult((false, default, ex.Message));
                }
            },
            error => tcs.TrySetResult((false, default, error.GenerateErrorReport())));

        return await tcs.Task;
    }

    // ── Player Data ─────────────────────────────────────────────────────────

    public async UniTask<bool> SetDisplayNameAsync(string name)
    {
        var tcs = new UniTaskCompletionSource<bool>();

        PlayFabClientAPI.UpdateUserTitleDisplayName(
            new UpdateUserTitleDisplayNameRequest { DisplayName = name },
            result => tcs.TrySetResult(true),
            error =>
            {
                Debug.LogError($"[PlayFabService] SetDisplayName error: {error.GenerateErrorReport()}");
                tcs.TrySetResult(false);
            });

        return await tcs.Task;
    }

    public async UniTask UpdatePlayerStatisticAsync(string statisticName, int value)
    {
        var tcs = new UniTaskCompletionSource<bool>();

        PlayFabClientAPI.UpdatePlayerStatistics(
            new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate>
                {
                    new StatisticUpdate { StatisticName = statisticName, Value = value }
                }
            },
            result => tcs.TrySetResult(true),
            error =>
            {
                Debug.LogError($"[PlayFabService] UpdateStatistic error: {error.GenerateErrorReport()}");
                tcs.TrySetResult(false);
            });

        await tcs.Task;
    }
}
