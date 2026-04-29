using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;

/// <summary>
/// Controls all panels in the Login scene.
/// Hierarchy expected:
///   Panel_Welcome, Panel_SignIn, Panel_CreateAccount, Panel_FactionChoice
///   (toggle active state between them)
/// </summary>
public class AuthUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private RectTransform panelWelcome;
    [SerializeField] private RectTransform panelSignIn;
    [SerializeField] private RectTransform panelCreateAccount;
    [SerializeField] private RectTransform panelFactionChoice;

    [Header("Welcome")]
    [SerializeField] private Button btnSignIn;
    [SerializeField] private Button btnCreateAccount;

    [Header("Sign In")]
    [SerializeField] private TMP_InputField inputSignInEmail;
    [SerializeField] private TMP_InputField inputSignInPassword;
    [SerializeField] private Button btnDoSignIn;
    [SerializeField] private Button btnSignInBack;
    [SerializeField] private TextMeshProUGUI lblSignInError;

    [Header("Create Account")]
    [SerializeField] private TMP_InputField inputCreateDisplayName;
    [SerializeField] private TMP_InputField inputCreateEmail;
    [SerializeField] private TMP_InputField inputCreatePassword;
    [SerializeField] private TMP_InputField inputCreateConfirmPw;
    [SerializeField] private Button btnDoCreate;
    [SerializeField] private Button btnCreateBack;
    [SerializeField] private TextMeshProUGUI lblCreateError;

    [Header("Faction Choice")]
    [SerializeField] private Button btnChooseOrder;
    [SerializeField] private Button btnChooseCult;
    [SerializeField] private Button btnChooseWanderer;
    [SerializeField] private TextMeshProUGUI lblFactionError;

    [Header("Loading")]
    [SerializeField] private GameObject panelLoading;
    [SerializeField] private TextMeshProUGUI lblLoadingDots;

    private FirebaseService _firebase;
    private PlayFabService _playFab;
    private const float ANIM_DURATION = 0.25f;
    private const float SLIDE_OFFSET = 800f;

    private void Awake()
    {
        btnSignIn.onClick.AddListener(() => ShowPanel(panelSignIn));
        btnCreateAccount.onClick.AddListener(() => ShowPanel(panelCreateAccount));
        btnSignInBack.onClick.AddListener(() => ShowPanel(panelWelcome));
        btnCreateBack.onClick.AddListener(() => ShowPanel(panelWelcome));
        btnDoSignIn.onClick.AddListener(() => OnSignInAsync().Forget());
        btnDoCreate.onClick.AddListener(() => OnCreateAccountAsync().Forget());
        btnChooseOrder.onClick.AddListener(() => OnFactionChosenAsync(FactionType.Order).Forget());
        btnChooseCult.onClick.AddListener(() => OnFactionChosenAsync(FactionType.Cult).Forget());
        btnChooseWanderer.onClick.AddListener(() => OnFactionChosenAsync(FactionType.Wanderer).Forget());

        SetPanelLabels();
    }

    private void Start()
    {
        _firebase = ServiceLocator.Instance.Get<FirebaseService>();
        _playFab = ServiceLocator.Instance.Get<PlayFabService>();

        ShowPanel(panelWelcome, animate: false);
        SetLoadingVisible(false);
    }

    private void SetPanelLabels()
    {
        if (btnSignIn) btnSignIn.GetComponentInChildren<TextMeshProUGUI>()?.SetText(LocalizationConstants.BTN_SIGN_IN);
        if (btnCreateAccount) btnCreateAccount.GetComponentInChildren<TextMeshProUGUI>()?.SetText(LocalizationConstants.BTN_CREATE_ACCOUNT);
        if (btnChooseOrder) btnChooseOrder.GetComponentInChildren<TextMeshProUGUI>()?.SetText(LocalizationConstants.BTN_CHOOSE);
        if (btnChooseCult) btnChooseCult.GetComponentInChildren<TextMeshProUGUI>()?.SetText(LocalizationConstants.BTN_CHOOSE);
        if (btnChooseWanderer) btnChooseWanderer.GetComponentInChildren<TextMeshProUGUI>()?.SetText(LocalizationConstants.BTN_CHOOSE);
    }

    // ── Sign In ──────────────────────────────────────────────────────────────

    private async UniTaskVoid OnSignInAsync()
    {
        ClearErrors();
        var email = inputSignInEmail.text.Trim();
        var password = inputSignInPassword.text;

        if (string.IsNullOrEmpty(email))
        {
            ShowError(lblSignInError, LocalizationConstants.ERR_INVALID_EMAIL);
            return;
        }
        if (password.Length < 6)
        {
            ShowError(lblSignInError, LocalizationConstants.ERR_WEAK_PASSWORD);
            return;
        }

        SetLoadingVisible(true);
        SetInteractable(false);

        var (success, error) = await _firebase.SignInWithEmailAsync(email, password);

        if (!success)
        {
            SetLoadingVisible(false);
            SetInteractable(true);
            ShowError(lblSignInError, MapFirebaseError(error));
            return;
        }

        // Link PlayFab
        var token = await _firebase.GetIdTokenAsync();
        var (pfSuccess, pfError) = await _playFab.LoginWithFirebaseAsync(token);
        if (!pfSuccess)
        {
            SetLoadingVisible(false);
            SetInteractable(true);
            ShowError(lblSignInError, LocalizationConstants.ERR_NETWORK);
            return;
        }

        // Refresh currencies
        await ServiceLocator.Instance.Get<EconomyService>().RefreshCurrenciesAsync();

        // Check faction — if already set, go to world map
        var playerDoc = await _firebase.GetPlayerDocumentAsync();
        if (playerDoc == null || playerDoc.faction == "unset" || string.IsNullOrEmpty(playerDoc.faction))
        {
            SetLoadingVisible(false);
            SetInteractable(true);
            ShowPanel(panelFactionChoice);
        }
        else
        {
            var faction = Enum.TryParse<FactionType>(playerDoc.faction, true, out var f) ? f : FactionType.Unset;
            GameManager.Instance.SetPlayerState(_playFab.PlayFabId, _firebase.UserId, faction, playerDoc.serverId);
            await GameManager.Instance.LoadSceneAsync("WorldMap");
        }
    }

    // ── Create Account ────────────────────────────────────────────────────────

    private async UniTaskVoid OnCreateAccountAsync()
    {
        ClearErrors();
        var displayName = inputCreateDisplayName.text.Trim();
        var email = inputCreateEmail.text.Trim();
        var password = inputCreatePassword.text;
        var confirm = inputCreateConfirmPw.text;

        if (displayName.Length < 3)
        {
            ShowError(lblCreateError, LocalizationConstants.ERR_DISPLAY_NAME_SHORT);
            return;
        }
        if (string.IsNullOrEmpty(email))
        {
            ShowError(lblCreateError, LocalizationConstants.ERR_INVALID_EMAIL);
            return;
        }
        if (password.Length < 6)
        {
            ShowError(lblCreateError, LocalizationConstants.ERR_WEAK_PASSWORD);
            return;
        }
        if (password != confirm)
        {
            ShowError(lblCreateError, LocalizationConstants.ERR_PASSWORDS_NO_MATCH);
            return;
        }

        SetLoadingVisible(true);
        SetInteractable(false);

        var (success, error) = await _firebase.SignUpWithEmailAsync(email, password, displayName);
        if (!success)
        {
            SetLoadingVisible(false);
            SetInteractable(true);
            ShowError(lblCreateError, MapFirebaseError(error));
            return;
        }

        // Link PlayFab
        var token = await _firebase.GetIdTokenAsync();
        await _playFab.LoginWithFirebaseAsync(token);
        await _playFab.SetDisplayNameAsync(displayName);

        // Create Firestore doc with faction = unset
        await _firebase.CreatePlayerDocumentAsync(displayName, FactionType.Unset);

        SetLoadingVisible(false);
        SetInteractable(true);
        ShowPanel(panelFactionChoice);
    }

    // ── Faction Choice ────────────────────────────────────────────────────────

    private async UniTaskVoid OnFactionChosenAsync(FactionType faction)
    {
        SetLoadingVisible(true);
        SetInteractable(false);

        var success = await _firebase.UpdatePlayerFactionAsync(faction);
        if (!success)
        {
            SetLoadingVisible(false);
            SetInteractable(true);
            ShowError(lblFactionError, LocalizationConstants.ERR_GENERIC);
            return;
        }

        var playerDoc = await _firebase.GetPlayerDocumentAsync();
        GameManager.Instance.SetPlayerState(_playFab.PlayFabId, _firebase.UserId, faction, playerDoc?.serverId);
        await GameManager.Instance.LoadSceneAsync("WorldMap");
    }

    // ── Panel Transitions ─────────────────────────────────────────────────────

    private void ShowPanel(RectTransform target, bool animate = true)
    {
        var panels = new[] { panelWelcome, panelSignIn, panelCreateAccount, panelFactionChoice };
        foreach (var p in panels)
        {
            if (p == target) continue;
            if (animate)
                p.DOFade(0f, ANIM_DURATION).OnComplete(() => p.gameObject.SetActive(false));
            else
                p.gameObject.SetActive(false);
        }

        target.gameObject.SetActive(true);
        if (animate)
        {
            var cg = target.GetComponent<CanvasGroup>() ?? target.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            target.anchoredPosition = new Vector2(0, -SLIDE_OFFSET);
            cg.DOFade(1f, ANIM_DURATION);
            target.DOAnchorPosY(0f, ANIM_DURATION).SetEase(Ease.OutCubic);
        }
    }

    private void SetLoadingVisible(bool visible)
    {
        if (panelLoading) panelLoading.SetActive(visible);
        if (visible && lblLoadingDots) AnimateLoadingDots().Forget();
    }

    private async UniTaskVoid AnimateLoadingDots()
    {
        string[] frames = { ".", "..", "..." };
        int i = 0;
        while (panelLoading != null && panelLoading.activeSelf)
        {
            lblLoadingDots.SetText(LocalizationConstants.LOADING.Replace("...", frames[i % 3]));
            i++;
            await UniTask.Delay(400);
        }
    }

    private void SetInteractable(bool state)
    {
        btnDoSignIn.interactable = state;
        btnDoCreate.interactable = state;
        btnSignIn.interactable = state;
        btnCreateAccount.interactable = state;
    }

    private void ShowError(TextMeshProUGUI label, string msg)
    {
        if (label == null) return;
        label.SetText(msg);
        label.color = Color.red;
        label.gameObject.SetActive(true);
    }

    private void ClearErrors()
    {
        foreach (var lbl in new[] { lblSignInError, lblCreateError, lblFactionError })
            if (lbl != null) lbl.gameObject.SetActive(false);
    }

    private static string MapFirebaseError(string rawError)
    {
        if (rawError == null) return LocalizationConstants.ERR_GENERIC;
        if (rawError.Contains("email-already-in-use")) return LocalizationConstants.ERR_EMAIL_IN_USE;
        if (rawError.Contains("wrong-password") || rawError.Contains("invalid-credential"))
            return LocalizationConstants.ERR_WRONG_CREDENTIALS;
        if (rawError.Contains("network")) return LocalizationConstants.ERR_NETWORK;
        if (rawError.Contains("weak-password")) return LocalizationConstants.ERR_WEAK_PASSWORD;
        if (rawError.Contains("invalid-email")) return LocalizationConstants.ERR_INVALID_EMAIL;
        return LocalizationConstants.ERR_GENERIC;
    }
}
