using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;

/// <summary>
/// Controls the Alliance screen (accessed from HUD Alliance button).
/// Tabs: Overview, Members, Chat, Search
/// Attach to a persistent UI panel GameObject.
/// </summary>
public class AllianceUIController : MonoBehaviour
{
    [Header("Root Panel")]
    [SerializeField] private RectTransform rootPanel;
    [SerializeField] private Button btnClose;

    [Header("Tabs")]
    [SerializeField] private Button btnTabOverview;
    [SerializeField] private Button btnTabMembers;
    [SerializeField] private Button btnTabChat;
    [SerializeField] private Button btnTabSearch;
    [SerializeField] private GameObject tabOverview;
    [SerializeField] private GameObject tabMembers;
    [SerializeField] private GameObject tabChat;
    [SerializeField] private GameObject tabSearch;

    // ── Overview Tab ─────────────────────────────────────────────────────────
    [Header("Overview")]
    [SerializeField] private TextMeshProUGUI lblAllianceName;
    [SerializeField] private TextMeshProUGUI lblMemberCount;
    [SerializeField] private TextMeshProUGUI lblTotalPower;
    [SerializeField] private TextMeshProUGUI lblScore;
    [SerializeField] private GameObject panelPatron;              // Cult only
    [SerializeField] private TextMeshProUGUI lblPatronName;
    [SerializeField] private Slider sliderRitualProgress;
    [SerializeField] private TextMeshProUGUI lblRitualProgress;
    [SerializeField] private Button btnLeave;
    [SerializeField] private Button btnDisband;

    // ── Members Tab ──────────────────────────────────────────────────────────
    [Header("Members")]
    [SerializeField] private Transform membersListParent;
    [SerializeField] private GameObject memberRowPrefab;

    // ── Chat Tab ─────────────────────────────────────────────────────────────
    [Header("Chat")]
    [SerializeField] private Transform chatListParent;
    [SerializeField] private GameObject chatMessagePrefab;
    [SerializeField] private ScrollRect chatScroll;
    [SerializeField] private TMP_InputField inputChatMessage;
    [SerializeField] private Button btnSendChat;

    // ── Search Tab ───────────────────────────────────────────────────────────
    [Header("Search")]
    [SerializeField] private TMP_InputField inputSearch;
    [SerializeField] private Button btnSearch;
    [SerializeField] private Transform searchResultsParent;
    [SerializeField] private GameObject searchResultPrefab;
    [SerializeField] private Button btnCreateAlliance;
    [SerializeField] private GameObject panelCreateForm;
    [SerializeField] private TMP_InputField inputAllianceName;
    [SerializeField] private TMP_InputField inputAllianceDesc;
    [SerializeField] private Toggle toggleOpenAlliance;
    [SerializeField] private TMP_InputField inputMinPower;
    [SerializeField] private Button btnConfirmCreate;
    [SerializeField] private TextMeshProUGUI lblSearchError;

    private AllianceService _allianceService;
    private const float PANEL_ANIM_DURATION = 0.3f;

    private void Awake()
    {
        btnClose.onClick.AddListener(Hide);
        btnTabOverview.onClick.AddListener(() => ShowTab(tabOverview));
        btnTabMembers.onClick.AddListener(() => ShowTab(tabMembers));
        btnTabChat.onClick.AddListener(() => ShowTab(tabChat));
        btnTabSearch.onClick.AddListener(() => ShowTab(tabSearch));
        btnLeave.onClick.AddListener(() => OnLeaveAsync().Forget());
        btnDisband.onClick.AddListener(() => Debug.Log("[Alliance] Disband not yet implemented."));
        btnSendChat.onClick.AddListener(() => OnSendChatAsync().Forget());
        btnSearch.onClick.AddListener(() => OnSearchAsync().Forget());
        btnCreateAlliance.onClick.AddListener(() => panelCreateForm.SetActive(true));
        btnConfirmCreate.onClick.AddListener(() => OnCreateAllianceAsync().Forget());

        inputSearch.placeholder.GetComponent<TextMeshProUGUI>()?.SetText(LocalizationConstants.PLACEHOLDER_SEARCH);
        inputChatMessage.placeholder.GetComponent<TextMeshProUGUI>()?.SetText(LocalizationConstants.PLACEHOLDER_MESSAGE);

        gameObject.SetActive(false);
    }

    private void Start()
    {
        _allianceService = ServiceLocator.Instance.Get<AllianceService>();
    }

    // ── Show / Hide ────────────────────────────────────────────────────────────

    public void Show()
    {
        gameObject.SetActive(true);
        rootPanel.localScale = Vector3.zero;
        rootPanel.DOScale(Vector3.one, PANEL_ANIM_DURATION).SetEase(Ease.OutBack);

        bool inAlliance = !string.IsNullOrEmpty(_allianceService.CurrentAllianceId);
        btnTabOverview.gameObject.SetActive(inAlliance);
        btnTabMembers.gameObject.SetActive(inAlliance);
        btnTabChat.gameObject.SetActive(inAlliance);
        btnTabSearch.gameObject.SetActive(!inAlliance);

        if (inAlliance)
        {
            ShowTab(tabOverview);
            PopulateOverview();
            _allianceService.SubscribeToAllianceChat(OnChatMessageReceived);
        }
        else
        {
            ShowTab(tabSearch);
        }
    }

    public void Hide()
    {
        _allianceService.UnsubscribeFromAllianceChat();
        rootPanel.DOScale(Vector3.zero, PANEL_ANIM_DURATION)
                 .SetEase(Ease.InBack)
                 .OnComplete(() => gameObject.SetActive(false));
    }

    // ── Overview ──────────────────────────────────────────────────────────────

    private void PopulateOverview()
    {
        var a = _allianceService.CurrentAlliance;
        if (a == null) return;

        lblAllianceName.SetText(a.name);
        lblMemberCount.SetText($"{LocalizationConstants.LABEL_MEMBERS}: {a.memberCount}/{GameConstants.MAX_ALLIANCE_SIZE}");
        lblTotalPower.SetText($"{LocalizationConstants.LABEL_TOTAL_POWER}: {a.totalPower:N0}");
        lblScore.SetText($"Score: {a.score:N0}");

        bool isCult = string.Equals(a.faction, "cult", StringComparison.OrdinalIgnoreCase);
        panelPatron.SetActive(isCult);
        if (isCult)
        {
            lblPatronName.SetText(a.patronId ?? "No Patron Bound");
            sliderRitualProgress.value = a.ritualProgress / 100f;
            lblRitualProgress.SetText($"{LocalizationConstants.LABEL_RITUAL_PROG}: {a.ritualProgress:F1}%");
        }

        bool isLeader = a.leaderId == ServiceLocator.Instance.Get<FirebaseService>()?.UserId;
        btnDisband.gameObject.SetActive(isLeader);
        btnLeave.gameObject.SetActive(!isLeader);
    }

    // ── Members ────────────────────────────────────────────────────────────────

    private async UniTaskVoid PopulateMembersAsync()
    {
        foreach (Transform child in membersListParent) Destroy(child.gameObject);

        var a = _allianceService.CurrentAlliance;
        if (a?.memberIds == null) return;

        var firebase = ServiceLocator.Instance.Get<FirebaseService>();
        long fiveMinAgo = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 300;

        foreach (var memberId in a.memberIds)
        {
            var playerDoc = await firebase.GetPlayerDocumentAsync(memberId);
            if (playerDoc == null) continue;

            var row = Instantiate(memberRowPrefab, membersListParent);
            var texts = row.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 3)
            {
                texts[0].SetText(playerDoc.displayName);
                texts[1].SetText($"Power: {playerDoc.power:N0}");
                bool isOnline = playerDoc.lastActive >= fiveMinAgo;
                texts[2].SetText(isOnline ? LocalizationConstants.LABEL_ONLINE : LocalizationConstants.LABEL_OFFLINE);
                texts[2].color = isOnline ? Color.green : Color.grey;
            }
        }
    }

    // ── Chat ───────────────────────────────────────────────────────────────────

    private void OnChatMessageReceived(ChatMessage msg)
    {
        var msgGO = Instantiate(chatMessagePrefab, chatListParent);
        var texts = msgGO.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length >= 2)
        {
            texts[0].SetText(msg.senderName);
            texts[1].SetText(msg.content);
        }
        Canvas.ForceUpdateCanvases();
        chatScroll.verticalNormalizedPosition = 0f;
    }

    private async UniTaskVoid OnSendChatAsync()
    {
        var text = inputChatMessage.text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        inputChatMessage.text = "";
        var (success, error) = await _allianceService.SendChatMessageAsync(text);
        if (!success)
            Debug.LogWarning($"[AllianceUI] Chat send failed: {error}");
    }

    // ── Search ─────────────────────────────────────────────────────────────────

    private async UniTaskVoid OnSearchAsync()
    {
        foreach (Transform child in searchResultsParent) Destroy(child.gameObject);
        if (lblSearchError) lblSearchError.gameObject.SetActive(false);

        var results = await _allianceService.SearchAlliancesAsync(
            inputSearch.text.Trim(), GameManager.Instance.CurrentFaction);

        foreach (var a in results)
        {
            var row = Instantiate(searchResultPrefab, searchResultsParent);
            var texts = row.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 3)
            {
                texts[0].SetText(a.name);
                texts[1].SetText($"{a.memberCount}/{GameConstants.MAX_ALLIANCE_SIZE} | Power req: {a.minPowerRequired:N0}");
                texts[2].SetText(a.isOpen ? "Open" : "Closed");
            }
            var capturedId = a.id;
            var capturedName = a.name;
            row.GetComponentInChildren<Button>()?.onClick.AddListener(() =>
                OnJoinAllianceAsync(capturedId).Forget());
        }
    }

    private async UniTaskVoid OnJoinAllianceAsync(string allianceId)
    {
        var (success, error) = await _allianceService.JoinAllianceAsync(allianceId);
        if (!success)
        {
            if (lblSearchError)
            {
                lblSearchError.SetText(error);
                lblSearchError.gameObject.SetActive(true);
            }
            return;
        }
        Show(); // refresh panel
    }

    private async UniTaskVoid OnCreateAllianceAsync()
    {
        var name = inputAllianceName.text.Trim();
        var desc = inputAllianceDesc.text.Trim();
        bool isOpen = toggleOpenAlliance.isOn;
        int.TryParse(inputMinPower.text, out int minPower);

        if (name.Length < 3)
        {
            if (lblSearchError) { lblSearchError.SetText("Name must be at least 3 characters."); lblSearchError.gameObject.SetActive(true); }
            return;
        }

        var (success, allianceId, error) = await _allianceService.CreateAllianceAsync(name, desc, isOpen, minPower);
        if (!success)
        {
            if (lblSearchError) { lblSearchError.SetText(error); lblSearchError.gameObject.SetActive(true); }
            return;
        }

        panelCreateForm.SetActive(false);
        Show();
    }

    // ── Leave ──────────────────────────────────────────────────────────────────

    private async UniTaskVoid OnLeaveAsync()
    {
        var (success, error) = await _allianceService.LeaveAllianceAsync();
        if (!success)
        {
            Debug.LogWarning($"[AllianceUI] Leave failed: {error}");
            return;
        }
        Show();
    }

    // ── Tabs ───────────────────────────────────────────────────────────────────

    private void ShowTab(GameObject target)
    {
        foreach (var tab in new[] { tabOverview, tabMembers, tabChat, tabSearch })
            tab.SetActive(tab == target);

        if (target == tabMembers) PopulateMembersAsync().Forget();
    }
}
