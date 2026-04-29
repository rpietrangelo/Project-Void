using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("State")]
    public bool IsInitialized { get; private set; }
    public string CurrentPlayFabId { get; private set; }
    public string CurrentFirebaseUid { get; private set; }
    public FactionType CurrentFaction { get; private set; }
    public string CurrentServerId { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        await InitializeServicesAsync();
    }

    private async UniTask InitializeServicesAsync()
    {
        Debug.Log("[GameManager] Initializing services...");

        // Services register themselves with ServiceLocator in their own Awake()
        // GameManager waits for them to be ready
        await UniTask.WaitUntil(() => ServiceLocator.Instance.IsRegistered<FirebaseService>());
        await UniTask.WaitUntil(() => ServiceLocator.Instance.IsRegistered<PlayFabService>());

        IsInitialized = true;
        Debug.Log("[GameManager] All services initialized.");

        // Route to login or main game based on auth state
        var auth = ServiceLocator.Instance.Get<FirebaseService>();
        if (auth.IsLoggedIn)
            await LoadSceneAsync("WorldMap");
        else
            await LoadSceneAsync("Login");
    }

    public void SetPlayerState(string playfabId, string firebaseUid, FactionType faction, string serverId)
    {
        CurrentPlayFabId = playfabId;
        CurrentFirebaseUid = firebaseUid;
        CurrentFaction = faction;
        CurrentServerId = serverId;
    }

    public async UniTask LoadSceneAsync(string sceneName)
    {
        Debug.Log($"[GameManager] Loading scene: {sceneName}");
        await SceneManager.LoadSceneAsync(sceneName);
    }
}
