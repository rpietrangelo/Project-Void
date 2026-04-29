using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.RemoteConfig;

public class FirebaseService : MonoBehaviour
{
    public bool IsInitialized { get; private set; }
    public bool IsLoggedIn { get; private set; }
    public string UserId { get; private set; }     // Firebase UID
    public string Email { get; private set; }

    private FirebaseAuth _auth;
    private FirebaseFirestore _db;
    private FirebaseRemoteConfig _remoteConfig;
    private PlayerDocument _cachedPlayerDoc;

    private void Awake()
    {
        ServiceLocator.Instance.Register<FirebaseService>(this);
    }

    private async void Start()
    {
        await InitializeFirebaseAsync();
    }

    private async UniTask InitializeFirebaseAsync()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask();
        if (dependencyStatus != DependencyStatus.Available)
        {
            Debug.LogError($"[FirebaseService] Could not resolve Firebase dependencies: {dependencyStatus}");
            return;
        }

        _auth = FirebaseAuth.DefaultInstance;
        _db = FirebaseFirestore.DefaultInstance;
        _remoteConfig = FirebaseRemoteConfig.DefaultInstance;

        _auth.StateChanged += OnAuthStateChanged;

        if (_auth.CurrentUser != null)
        {
            UserId = _auth.CurrentUser.UserId;
            Email = _auth.CurrentUser.Email;
            IsLoggedIn = true;
        }

        await FetchRemoteConfigAsync();

        IsInitialized = true;
        Debug.Log("[FirebaseService] Initialized.");
    }

    private void OnDestroy()
    {
        if (_auth != null)
            _auth.StateChanged -= OnAuthStateChanged;
    }

    private void OnAuthStateChanged(object sender, EventArgs e)
    {
        var user = _auth.CurrentUser;
        if (user != null)
        {
            UserId = user.UserId;
            Email = user.Email;
            IsLoggedIn = true;
        }
        else
        {
            UserId = null;
            Email = null;
            IsLoggedIn = false;
            _cachedPlayerDoc = null;
        }
    }

    // ── Auth ────────────────────────────────────────────────────────────────

    public async UniTask<(bool success, string error)> SignUpWithEmailAsync(string email, string password, string displayName)
    {
        try
        {
            var result = await _auth.CreateUserWithEmailAndPasswordAsync(email, password).AsUniTask();
            var profile = new UserProfile { DisplayName = displayName };
            await result.User.UpdateUserProfileAsync(profile).AsUniTask();

            UserId = result.User.UserId;
            Email = email;
            IsLoggedIn = true;

            EventBus.Publish(new AuthStateChangedEvent(true, null, UserId));
            return (true, null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirebaseService] SignUp error: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async UniTask<(bool success, string error)> SignInWithEmailAsync(string email, string password)
    {
        try
        {
            var result = await _auth.SignInWithEmailAndPasswordAsync(email, password).AsUniTask();
            UserId = result.User.UserId;
            Email = email;
            IsLoggedIn = true;

            EventBus.Publish(new AuthStateChangedEvent(true, null, UserId));
            return (true, null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirebaseService] SignIn error: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async UniTask<(bool success, string error)> SignInWithGoogleAsync()
    {
        // Requires Google Sign-In plugin; token obtained externally then passed to Firebase
        // Placeholder — implement with Google Sign-In SDK on target platform
        await UniTask.CompletedTask;
        return (false, "Google Sign-In not yet configured on this platform.");
    }

    public async UniTask<(bool success, string error)> SignInWithAppleAsync()
    {
        // Requires Apple Sign-In plugin; token obtained externally
        // Placeholder — implement with Sign in with Apple SDK on iOS
        await UniTask.CompletedTask;
        return (false, "Apple Sign-In not yet configured on this platform.");
    }

    public async UniTask SignOutAsync()
    {
        _auth.SignOut();
        _cachedPlayerDoc = null;
        IsLoggedIn = false;
        UserId = null;
        Email = null;
        EventBus.Publish(new AuthStateChangedEvent(false, null, null));
        await UniTask.CompletedTask;
    }

    // ── Firestore — Player ──────────────────────────────────────────────────

    public async UniTask<bool> CreatePlayerDocumentAsync(string displayName, FactionType faction)
    {
        try
        {
            var doc = new PlayerDocument
            {
                displayName = displayName,
                faction = faction.ToString().ToLower(),
                allianceId = null,
                serverId = "server_001",
                cityId = null,
                power = 0,
                lastActive = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                loginStreak = 1,
                loginStreakUpdated = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                profileBadges = new List<string>()
            };

            await _db.Collection(GameConstants.COL_PLAYERS)
                     .Document(UserId)
                     .SetAsync(doc)
                     .AsUniTask();

            _cachedPlayerDoc = doc;
            Debug.Log($"[FirebaseService] Player document created for {UserId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirebaseService] CreatePlayerDocument error: {ex.Message}");
            return false;
        }
    }

    public async UniTask<PlayerDocument> GetPlayerDocumentAsync(string playerId = null)
    {
        try
        {
            var id = playerId ?? UserId;
            var snap = await _db.Collection(GameConstants.COL_PLAYERS)
                                .Document(id)
                                .GetSnapshotAsync()
                                .AsUniTask();

            if (!snap.Exists)
                return null;

            var doc = snap.ConvertTo<PlayerDocument>();
            if (playerId == null || playerId == UserId)
                _cachedPlayerDoc = doc;

            return doc;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirebaseService] GetPlayerDocument error: {ex.Message}");
            return null;
        }
    }

    public async UniTask<bool> UpdatePlayerFactionAsync(FactionType faction)
    {
        try
        {
            await _db.Collection(GameConstants.COL_PLAYERS)
                     .Document(UserId)
                     .UpdateAsync("faction", faction.ToString().ToLower())
                     .AsUniTask();

            if (_cachedPlayerDoc != null)
                _cachedPlayerDoc.faction = faction.ToString().ToLower();

            EventBus.Publish(new FactionChosenEvent(faction));
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirebaseService] UpdateFaction error: {ex.Message}");
            return false;
        }
    }

    // ── Remote Config ───────────────────────────────────────────────────────

    public async UniTask FetchRemoteConfigAsync()
    {
        try
        {
            var defaults = new Dictionary<string, object>
            {
                { GameConstants.RC_RESOURCE_RATE_MULT, 1.0f },
                { GameConstants.RC_BUILD_TIME_MULT,    1.0f },
                { GameConstants.RC_SHIELD_DURATION_H,  8    },
                { GameConstants.RC_CORRUPTION_RATE,    0.05f },
                { GameConstants.RC_PATRON_THRESHOLD,   100000 },
                { GameConstants.RC_HORROR_EVENT_FREQ_H, 24  }
            };
            await _remoteConfig.SetDefaultsAsync(defaults).AsUniTask();
            await _remoteConfig.FetchAndActivateAsync().AsUniTask();
            Debug.Log("[FirebaseService] Remote Config fetched.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[FirebaseService] RemoteConfig fetch failed (using defaults): {ex.Message}");
        }
    }

    public float GetFloat(string key, float defaultValue) =>
        IsInitialized ? (float)_remoteConfig.GetValue(key).DoubleValue : defaultValue;

    public int GetInt(string key, int defaultValue) =>
        IsInitialized ? (int)_remoteConfig.GetValue(key).LongValue : defaultValue;

    // ── Helpers ─────────────────────────────────────────────────────────────

    public async UniTask<string> GetIdTokenAsync()
    {
        if (_auth.CurrentUser == null) return null;
        return await _auth.CurrentUser.TokenAsync(false).AsUniTask();
    }

    public PlayerDocument GetCachedPlayerDocument() => _cachedPlayerDoc;
}

[Serializable]
[FirestoreData]
public class PlayerDocument
{
    [FirestoreProperty] public string displayName { get; set; }
    [FirestoreProperty] public string faction { get; set; }         // "order" | "cult" | "wanderer" | "unset"
    [FirestoreProperty] public string allianceId { get; set; }      // null if none
    [FirestoreProperty] public string serverId { get; set; }
    [FirestoreProperty] public string cityId { get; set; }
    [FirestoreProperty] public long power { get; set; }
    [FirestoreProperty] public long lastActive { get; set; }        // Unix timestamp
    [FirestoreProperty] public long createdAt { get; set; }
    [FirestoreProperty] public int loginStreak { get; set; }
    [FirestoreProperty] public long loginStreakUpdated { get; set; }
    [FirestoreProperty] public List<string> profileBadges { get; set; }
}
