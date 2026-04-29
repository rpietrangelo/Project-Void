using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Handles the World Map HUD and tile/city info panel.
/// </summary>
public class WorldMapUIController : MonoBehaviour
{
    [Header("HUD Top")]
    [SerializeField] private TextMeshProUGUI lblPaleGold;
    [SerializeField] private TextMeshProUGUI lblVoidCrystals;

    [Header("HUD Bottom")]
    [SerializeField] private Button btnMyCity;
    [SerializeField] private Button btnAlliance;

    [Header("Panel — Tile / City Info")]
    [SerializeField] private RectTransform panelTileInfo;
    [SerializeField] private TextMeshProUGUI lblTileTitle;
    [SerializeField] private TextMeshProUGUI lblTileOwner;
    [SerializeField] private TextMeshProUGUI lblTileFaction;
    [SerializeField] private TextMeshProUGUI lblTilePower;
    [SerializeField] private TextMeshProUGUI lblTileAlliance;
    [SerializeField] private Button btnViewProfile;
    [SerializeField] private Button btnScout;
    [SerializeField] private Button btnAttack;
    [SerializeField] private Button btnSendResources;
    [SerializeField] private Button btnCloseTileInfo;

    [Header("Alliance Panel")]
    [SerializeField] private AllianceUIController allianceUI;

    private const float PANEL_OFFSCREEN_Y = -500f;
    private const float PANEL_SLIDE_DURATION = 0.3f;

    private void Awake()
    {
        btnMyCity.onClick.AddListener(() => GameManager.Instance.LoadSceneAsync("City").Forget());
        btnAlliance.onClick.AddListener(() => allianceUI?.Show());
        btnCloseTileInfo.onClick.AddListener(() => HideTileInfo());
        btnViewProfile.onClick.AddListener(() => Debug.Log("[WorldMapUI] View Profile (Phase future)"));
        btnScout.onClick.AddListener(() => Debug.Log("[WorldMapUI] Scout (Phase 6)"));
        btnAttack.onClick.AddListener(() => Debug.Log("[WorldMapUI] Attack (Phase 6)"));
        btnSendResources.onClick.AddListener(() => Debug.Log("[WorldMapUI] Send Resources (Phase 6)"));

        HideTileInfo(animate: false);
        RefreshResourceBar();
    }

    private void Start()
    {
        EventBus.Subscribe<ResourceChangedEvent>(OnResourceChanged);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<ResourceChangedEvent>(OnResourceChanged);
    }

    // ── Tile Info Panel ───────────────────────────────────────────────────────

    public void ShowTileInfo(CityDocument city)
    {
        lblTileTitle.SetText(city.cityName ?? city.ownerName ?? "Unknown City");
        lblTileOwner.SetText($"Owner: {city.ownerName ?? "Unknown"}");
        lblTileFaction.SetText($"Faction: {city.faction ?? "Unknown"}");
        lblTilePower.SetText($"Power: {city.power:N0}");
        lblTileAlliance.SetText(string.IsNullOrEmpty(city.allianceName)
            ? "No Alliance" : $"Alliance: {city.allianceName}");

        bool isOwn = city.ownerId == ServiceLocator.Instance.Get<FirebaseService>()?.UserId;
        btnAttack.gameObject.SetActive(!isOwn);
        btnSendResources.gameObject.SetActive(!isOwn);
        btnScout.gameObject.SetActive(!isOwn);
        btnViewProfile.gameObject.SetActive(!isOwn);

        panelTileInfo.gameObject.SetActive(true);
        panelTileInfo.anchoredPosition = new Vector2(0, PANEL_OFFSCREEN_Y);
        panelTileInfo.DOAnchorPosY(0f, PANEL_SLIDE_DURATION).SetEase(Ease.OutCubic);
    }

    private void HideTileInfo(bool animate = true)
    {
        if (!panelTileInfo.gameObject.activeSelf) return;
        if (animate)
            panelTileInfo.DOAnchorPosY(PANEL_OFFSCREEN_Y, PANEL_SLIDE_DURATION)
                         .SetEase(Ease.InCubic)
                         .OnComplete(() => panelTileInfo.gameObject.SetActive(false));
        else
            panelTileInfo.gameObject.SetActive(false);
    }

    // ── Resource Bar ──────────────────────────────────────────────────────────

    private void RefreshResourceBar()
    {
        var economy = ServiceLocator.Instance.Get<EconomyService>();
        if (economy == null) return;
        lblPaleGold.SetText($"PG: {economy.GetCurrency(GameConstants.CURRENCY_PALE_GOLD):N0}");
        lblVoidCrystals.SetText($"VC: {economy.GetCurrency(GameConstants.CURRENCY_VOID_CRYSTALS):N0}");
    }

    private void OnResourceChanged(ResourceChangedEvent evt) => RefreshResourceBar();
}
