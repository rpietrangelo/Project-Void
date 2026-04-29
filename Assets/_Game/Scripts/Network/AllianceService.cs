using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;

public class AllianceService : MonoBehaviour
{
    public string CurrentAllianceId { get; private set; }
    public AllianceDocument CurrentAlliance { get; private set; }

    private FirebaseService _firebase;
    private FirebaseFirestore _db;
    private ListenerRegistration _chatListener;
    private Action<ChatMessage> _onNewMessage;

    private void Awake()
    {
        ServiceLocator.Instance.Register<AllianceService>(this);
    }

    private void Start()
    {
        _firebase = ServiceLocator.Instance.Get<FirebaseService>();
        _db = FirebaseFirestore.DefaultInstance;

        EventBus.Subscribe<AuthStateChangedEvent>(OnAuthStateChanged);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<AuthStateChangedEvent>(OnAuthStateChanged);
        UnsubscribeFromAllianceChat();
    }

    private async void OnAuthStateChanged(AuthStateChangedEvent evt)
    {
        if (evt.IsLoggedIn)
        {
            var playerDoc = await _firebase.GetPlayerDocumentAsync();
            if (!string.IsNullOrEmpty(playerDoc?.allianceId))
            {
                CurrentAllianceId = playerDoc.allianceId;
                CurrentAlliance = await GetAllianceDocumentAsync(CurrentAllianceId);
            }
        }
        else
        {
            CurrentAllianceId = null;
            CurrentAlliance = null;
            UnsubscribeFromAllianceChat();
        }
    }

    // ── Create ────────────────────────────────────────────────────────────────

    public async UniTask<(bool success, string allianceId, string error)>
        CreateAllianceAsync(string name, string description, bool isOpen, int minPower)
    {
        // Check name uniqueness
        var query = await _db.Collection(GameConstants.COL_ALLIANCES)
                             .WhereEqualTo("name", name)
                             .GetSnapshotAsync()
                             .AsUniTask();
        if (query.Count > 0)
            return (false, null, LocalizationConstants.ERR_ALLIANCE_NAME_TAKEN);

        var faction = GameManager.Instance.CurrentFaction;
        var allianceId = System.Guid.NewGuid().ToString("N");
        var doc = new AllianceDocument
        {
            id = allianceId,
            name = name,
            description = description,
            faction = faction.ToString().ToLower(),
            leaderId = _firebase.UserId,
            officerIds = new List<string>(),
            memberIds = new List<string> { _firebase.UserId },
            memberCount = 1,
            totalPower = 0,
            score = 0,
            patronId = null,
            ritualProgress = 0f,
            emblemId = "default",
            isOpen = isOpen,
            minPowerRequired = minPower
        };

        try
        {
            await _db.Collection(GameConstants.COL_ALLIANCES)
                     .Document(allianceId)
                     .SetAsync(doc)
                     .AsUniTask();

            // Update player document
            await _db.Collection(GameConstants.COL_PLAYERS)
                     .Document(_firebase.UserId)
                     .UpdateAsync("allianceId", allianceId)
                     .AsUniTask();

            CurrentAllianceId = allianceId;
            CurrentAlliance = doc;
            EventBus.Publish(new AllianceJoinedEvent(allianceId, name));
            return (true, allianceId, null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AllianceService] CreateAlliance error: {ex.Message}");
            return (false, null, ex.Message);
        }
    }

    // ── Join ──────────────────────────────────────────────────────────────────

    public async UniTask<(bool success, string error)> JoinAllianceAsync(string allianceId)
    {
        try
        {
            var snap = await _db.Collection(GameConstants.COL_ALLIANCES)
                                .Document(allianceId)
                                .GetSnapshotAsync()
                                .AsUniTask();
            if (!snap.Exists) return (false, "Alliance not found.");

            var alliance = snap.ConvertTo<AllianceDocument>();

            if (alliance.memberCount >= GameConstants.MAX_ALLIANCE_SIZE)
                return (false, LocalizationConstants.ERR_ALLIANCE_FULL);

            var playerDoc = await _firebase.GetPlayerDocumentAsync();
            if (playerDoc.power < alliance.minPowerRequired)
                return (false, LocalizationConstants.ERR_POWER_TOO_LOW);

            var factionMatch = string.Equals(alliance.faction,
                GameManager.Instance.CurrentFaction.ToString(), StringComparison.OrdinalIgnoreCase);
            if (!factionMatch) return (false, "Faction mismatch.");

            // Add member (Firestore ArrayUnion)
            await _db.Collection(GameConstants.COL_ALLIANCES)
                     .Document(allianceId)
                     .UpdateAsync(new Dictionary<string, object>
                     {
                         ["memberIds"]   = FieldValue.ArrayUnion(_firebase.UserId),
                         ["memberCount"] = FieldValue.Increment(1)
                     })
                     .AsUniTask();

            await _db.Collection(GameConstants.COL_PLAYERS)
                     .Document(_firebase.UserId)
                     .UpdateAsync("allianceId", allianceId)
                     .AsUniTask();

            CurrentAllianceId = allianceId;
            CurrentAlliance = alliance;
            EventBus.Publish(new AllianceJoinedEvent(allianceId, alliance.name));
            return (true, null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AllianceService] JoinAlliance error: {ex.Message}");
            return (false, ex.Message);
        }
    }

    // ── Leave ─────────────────────────────────────────────────────────────────

    public async UniTask<(bool success, string error)> LeaveAllianceAsync()
    {
        if (string.IsNullOrEmpty(CurrentAllianceId))
            return (false, "Not in an alliance.");

        if (CurrentAlliance?.leaderId == _firebase.UserId)
            return (false, "Leader must transfer leadership before leaving.");

        try
        {
            await _db.Collection(GameConstants.COL_ALLIANCES)
                     .Document(CurrentAllianceId)
                     .UpdateAsync(new Dictionary<string, object>
                     {
                         ["memberIds"]   = FieldValue.ArrayRemove(_firebase.UserId),
                         ["memberCount"] = FieldValue.Increment(-1)
                     })
                     .AsUniTask();

            await _db.Collection(GameConstants.COL_PLAYERS)
                     .Document(_firebase.UserId)
                     .UpdateAsync("allianceId", (string)null)
                     .AsUniTask();

            var leftId = CurrentAllianceId;
            CurrentAllianceId = null;
            CurrentAlliance = null;
            UnsubscribeFromAllianceChat();
            EventBus.Publish(new AllianceLeftEvent(leftId));
            return (true, null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AllianceService] LeaveAlliance error: {ex.Message}");
            return (false, ex.Message);
        }
    }

    // ── Search ────────────────────────────────────────────────────────────────

    public async UniTask<List<AllianceDocument>> SearchAlliancesAsync(string nameQuery, FactionType faction)
    {
        try
        {
            var query = _db.Collection(GameConstants.COL_ALLIANCES)
                           .WhereEqualTo("faction", faction.ToString().ToLower())
                           .Limit(20);

            var snap = await query.GetSnapshotAsync().AsUniTask();
            var results = new List<AllianceDocument>();
            foreach (var doc in snap.Documents)
            {
                var a = doc.ConvertTo<AllianceDocument>();
                if (string.IsNullOrEmpty(nameQuery) ||
                    a.name.IndexOf(nameQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                    results.Add(a);
            }
            return results;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AllianceService] SearchAlliances error: {ex.Message}");
            return new List<AllianceDocument>();
        }
    }

    // ── Chat ──────────────────────────────────────────────────────────────────

    public async UniTask<(bool success, string error)> SendChatMessageAsync(string message)
    {
        if (string.IsNullOrEmpty(CurrentAllianceId)) return (false, "Not in an alliance.");
        if (string.IsNullOrWhiteSpace(message)) return (false, "Message is empty.");

        try
        {
            var playerDoc = await _firebase.GetPlayerDocumentAsync();
            var chatMsg = new ChatMessage
            {
                senderId = _firebase.UserId,
                senderName = playerDoc?.displayName ?? "Unknown",
                content = message.Substring(0, Mathf.Min(message.Length, 500)),
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            await _db.Collection(GameConstants.COL_ALLIANCES)
                     .Document(CurrentAllianceId)
                     .Collection("chat")
                     .AddAsync(chatMsg)
                     .AsUniTask();
            return (true, null);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AllianceService] SendChat error: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public void SubscribeToAllianceChat(Action<ChatMessage> onNewMessage)
    {
        if (string.IsNullOrEmpty(CurrentAllianceId)) return;
        _onNewMessage = onNewMessage;

        _chatListener = _db.Collection(GameConstants.COL_ALLIANCES)
                           .Document(CurrentAllianceId)
                           .Collection("chat")
                           .OrderBy("timestamp")
                           .Limit(100)
                           .Listen(snap =>
                           {
                               foreach (var change in snap.GetChanges())
                               {
                                   if (change.ChangeType == DocumentChange.Type.Added)
                                   {
                                       var msg = change.Document.ConvertTo<ChatMessage>();
                                       _onNewMessage?.Invoke(msg);
                                   }
                               }
                           });
    }

    public void UnsubscribeFromAllianceChat()
    {
        _chatListener?.Stop();
        _chatListener = null;
        _onNewMessage = null;
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private async UniTask<AllianceDocument> GetAllianceDocumentAsync(string allianceId)
    {
        try
        {
            var snap = await _db.Collection(GameConstants.COL_ALLIANCES)
                                .Document(allianceId)
                                .GetSnapshotAsync()
                                .AsUniTask();
            return snap.Exists ? snap.ConvertTo<AllianceDocument>() : null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AllianceService] GetAllianceDocument error: {ex.Message}");
            return null;
        }
    }
}

[Serializable]
[FirestoreData]
public class AllianceDocument
{
    [FirestoreProperty] public string id { get; set; }
    [FirestoreProperty] public string name { get; set; }
    [FirestoreProperty] public string description { get; set; }
    [FirestoreProperty] public string faction { get; set; }
    [FirestoreProperty] public string leaderId { get; set; }
    [FirestoreProperty] public List<string> officerIds { get; set; }
    [FirestoreProperty] public List<string> memberIds { get; set; }
    [FirestoreProperty] public int memberCount { get; set; }
    [FirestoreProperty] public long totalPower { get; set; }
    [FirestoreProperty] public int score { get; set; }
    [FirestoreProperty] public string patronId { get; set; }           // Cult only
    [FirestoreProperty] public float ritualProgress { get; set; }      // Cult only
    [FirestoreProperty] public string emblemId { get; set; }
    [FirestoreProperty] public bool isOpen { get; set; }
    [FirestoreProperty] public int minPowerRequired { get; set; }
}

[Serializable]
[FirestoreData]
public class ChatMessage
{
    [FirestoreProperty] public string senderId { get; set; }
    [FirestoreProperty] public string senderName { get; set; }
    [FirestoreProperty] public string content { get; set; }
    [FirestoreProperty] public long timestamp { get; set; }
}
