using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;

/// <summary>
/// Controls all UI in the City scene.
/// Requires: LocalCityState, EconomyService (optional for Phase 2), BuildingData[] ref
/// </summary>
public class CityUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LocalCityState cityState;
    [SerializeField] private BuildingData[] allBuildings;
    [SerializeField] private Transform cityGridParent;
    [SerializeField] private GameObject buildingSlotPrefab;   // placeholder colored square

    [Header("HUD Top")]
    [SerializeField] private TextMeshProUGUI lblPaleGold;
    [SerializeField] private TextMeshProUGUI lblVoidCrystals;
    [SerializeField] private TextMeshProUGUI lblCityName;

    [Header("HUD Bottom")]
    [SerializeField] private Button btnWorldMap;
    [SerializeField] private Button btnAlliance;

    [Header("Panel — Building Slot (empty)")]
    [SerializeField] private RectTransform panelBuildingSlot;
    [SerializeField] private Transform buildingListParent;
    [SerializeField] private GameObject buildingCardPrefab;

    [Header("Panel — Building Detail (occupied)")]
    [SerializeField] private RectTransform panelBuildingDetail;
    [SerializeField] private TextMeshProUGUI lblDetailName;
    [SerializeField] private TextMeshProUGUI lblDetailLevel;
    [SerializeField] private TextMeshProUGUI lblDetailUpgradeCost;
    [SerializeField] private TextMeshProUGUI lblDetailUpgradeTime;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI lblDetailProgress;
    [SerializeField] private Button btnUpgrade;
    [SerializeField] private Button btnSpeedUp;

    private List<GameObject> _slotObjects = new();
    private int _selectedSlot = -1;
    private const float PANEL_SLIDE_DURATION = 0.3f;
    private const float PANEL_OFFSCREEN_Y = -600f;

    private void Awake()
    {
        btnWorldMap.onClick.AddListener(() => GameManager.Instance.LoadSceneAsync("WorldMap").Forget());
        btnAlliance.onClick.AddListener(() => Debug.Log("[CityUI] Alliance button pressed (Phase 5)"));
        btnUpgrade.onClick.AddListener(() => OnUpgradeAsync().Forget());
        btnSpeedUp.onClick.AddListener(() => Debug.Log("[CityUI] Speed Up pressed (future feature)"));

        HidePanel(panelBuildingSlot, animate: false);
        HidePanel(panelBuildingDetail, animate: false);
    }

    private void Start()
    {
        var faction = GameManager.Instance.CurrentFaction;
        cityState.Initialize(allBuildings);

        // Phase 3: wire economy service if available
        var economy = ServiceLocator.Instance.Get<EconomyService>();
        if (economy != null)
        {
            cityState.SetEconomyService(economy);
            LoadServerBuildingsAsync(economy).Forget();
        }

        BuildSlotGrid();
        RefreshResourceBar();
        lblCityName.SetText(GameManager.Instance.CurrentPlayFabId ?? "Your City");

        EventBus.Subscribe<ResourceChangedEvent>(OnResourceChanged);
        EventBus.Subscribe<BuildingUpgradeCompleteEvent>(OnUpgradeComplete);
        EventBus.Subscribe<BuildingUpgradeStartedEvent>(OnUpgradeStarted);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<ResourceChangedEvent>(OnResourceChanged);
        EventBus.Unsubscribe<BuildingUpgradeCompleteEvent>(OnUpgradeComplete);
        EventBus.Unsubscribe<BuildingUpgradeStartedEvent>(OnUpgradeStarted);
    }

    // ── Phase 3 Server Load ───────────────────────────────────────────────────

    private async UniTaskVoid LoadServerBuildingsAsync(EconomyService economy)
    {
        var serverSlots = await economy.GetPlayerBuildingsAsync();
        cityState.OverrideWithServerData(serverSlots);
        RefreshAllSlotVisuals();
    }

    // ── Grid ──────────────────────────────────────────────────────────────────

    private void BuildSlotGrid()
    {
        for (int i = 0; i < GameConstants.CITY_BUILDING_SLOTS; i++)
        {
            var slotGO = Instantiate(buildingSlotPrefab, cityGridParent);
            slotGO.name = $"Slot_{i}";
            int slotIndex = i;

            var btn = slotGO.GetComponent<Button>() ?? slotGO.AddComponent<Button>();
            btn.onClick.AddListener(() => OnSlotTapped(slotIndex));
            _slotObjects.Add(slotGO);
        }
        RefreshAllSlotVisuals();
    }

    private void RefreshAllSlotVisuals()
    {
        for (int i = 0; i < _slotObjects.Count; i++)
        {
            var inst = cityState.GetBuildingAt(i);
            var img = _slotObjects[i].GetComponent<Image>();
            if (img == null) continue;

            if (inst == null)
                img.color = new Color(0.3f, 0.3f, 0.3f); // empty = dark grey
            else if (inst.state == BuildingState.Upgrading)
                img.color = new Color(1f, 0.8f, 0f);      // upgrading = yellow
            else
                img.color = GetFactionColor(GameManager.Instance.CurrentFaction); // built
        }
    }

    private Color GetFactionColor(FactionType faction) => faction switch
    {
        FactionType.Order    => new Color(0.9f, 0.85f, 0.4f),
        FactionType.Cult     => new Color(0.5f, 0.2f, 0.7f),
        FactionType.Wanderer => new Color(0.3f, 0.7f, 0.4f),
        _                    => Color.white
    };

    // ── Slot Interaction ─────────────────────────────────────────────────────

    private void OnSlotTapped(int slotIndex)
    {
        _selectedSlot = slotIndex;
        var inst = cityState.GetBuildingAt(slotIndex);

        if (inst == null)
            ShowBuildingSelectPanel(slotIndex);
        else
            ShowBuildingDetailPanel(inst);
    }

    private void ShowBuildingSelectPanel(int slotIndex)
    {
        HidePanel(panelBuildingDetail, animate: true);

        // Clear and repopulate building list
        foreach (Transform child in buildingListParent)
            Destroy(child.gameObject);

        var faction = GameManager.Instance.CurrentFaction;
        foreach (var bd in allBuildings)
        {
            if (bd.faction != FactionType.Unset && bd.faction != faction) continue;

            var card = Instantiate(buildingCardPrefab, buildingListParent);
            card.GetComponentInChildren<TextMeshProUGUI>()?.SetText($"{bd.displayName}\n{FormatCost(bd.levels[0].buildCost)}");
            var capturedBd = bd;
            card.GetComponentInChildren<Button>()?.onClick.AddListener(() =>
            {
                cityState.PlaceBuilding(capturedBd.buildingId, slotIndex);
                cityState.StartUpgradeAsync(slotIndex).Forget();
                HidePanel(panelBuildingSlot);
                RefreshAllSlotVisuals();
            });
        }

        ShowPanel(panelBuildingSlot);
    }

    private void ShowBuildingDetailPanel(BuildingInstance inst)
    {
        HidePanel(panelBuildingSlot, animate: true);

        var bd = Array.Find(allBuildings, x => x.buildingId == inst.buildingId);
        lblDetailName.SetText(bd != null ? bd.displayName : inst.buildingId);
        lblDetailLevel.SetText($"{LocalizationConstants.LABEL_CURRENT_LEVEL} {inst.currentLevel}");

        bool isUpgrading = inst.state == BuildingState.Upgrading;
        btnUpgrade.gameObject.SetActive(!isUpgrading);
        progressBar.gameObject.SetActive(isUpgrading);
        lblDetailProgress.gameObject.SetActive(isUpgrading);

        if (!isUpgrading && bd != null && inst.currentLevel < bd.levels.Count)
        {
            var nextLevel = bd.levels[inst.currentLevel];
            lblDetailUpgradeCost.SetText($"{LocalizationConstants.LABEL_COST}: {FormatCost(nextLevel.buildCost)}");
            lblDetailUpgradeTime.SetText($"{LocalizationConstants.LABEL_BUILD_TIME}: {FormatTime(nextLevel.buildTimeSeconds)}");
            btnUpgrade.interactable = cityState.CanAfford(nextLevel);
        }
        else if (inst.currentLevel >= (bd?.levels.Count ?? 0))
        {
            lblDetailUpgradeCost.SetText(LocalizationConstants.LABEL_MAX_LEVEL);
            lblDetailUpgradeTime.SetText("");
            btnUpgrade.gameObject.SetActive(false);
        }

        if (isUpgrading)
            TrackProgress(inst).Forget();

        ShowPanel(panelBuildingDetail);
    }

    private async UniTaskVoid TrackProgress(BuildingInstance inst)
    {
        while (inst.state == BuildingState.Upgrading)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            float remaining = inst.upgradeFinishTime - now;
            if (remaining <= 0) break;

            if (progressBar != null)
                progressBar.value = 1f - Mathf.Clamp01(remaining / 60f);
            if (lblDetailProgress != null)
                lblDetailProgress.SetText(FormatTime(remaining));

            await UniTask.Delay(1000);
        }
        if (progressBar != null) progressBar.value = 1f;
        if (lblDetailProgress != null) lblDetailProgress.SetText("Done!");
        RefreshAllSlotVisuals();
    }

    // ── Upgrade ───────────────────────────────────────────────────────────────

    private async UniTaskVoid OnUpgradeAsync()
    {
        if (_selectedSlot < 0) return;
        btnUpgrade.interactable = false;

        var result = await cityState.StartUpgradeAsync(_selectedSlot);
        if (!result)
        {
            btnUpgrade.interactable = true;
            return;
        }

        var inst = cityState.GetBuildingAt(_selectedSlot);
        if (inst != null)
            ShowBuildingDetailPanel(inst);
        RefreshAllSlotVisuals();
    }

    // ── Events ────────────────────────────────────────────────────────────────

    private void OnResourceChanged(ResourceChangedEvent evt) => RefreshResourceBar();

    private void OnUpgradeComplete(BuildingUpgradeCompleteEvent evt)
    {
        RefreshAllSlotVisuals();
        if (_selectedSlot >= 0)
        {
            var inst = cityState.GetBuildingAt(_selectedSlot);
            if (inst != null && inst.buildingId == evt.BuildingId)
                ShowBuildingDetailPanel(inst);
        }
    }

    private void OnUpgradeStarted(BuildingUpgradeStartedEvent evt) => RefreshAllSlotVisuals();

    // ── Resource Bar ──────────────────────────────────────────────────────────

    private void RefreshResourceBar()
    {
        var economy = ServiceLocator.Instance.Get<EconomyService>();
        if (economy != null)
        {
            lblPaleGold.SetText($"PG: {economy.GetCurrency(GameConstants.CURRENCY_PALE_GOLD):N0}");
            lblVoidCrystals.SetText($"VC: {economy.GetCurrency(GameConstants.CURRENCY_VOID_CRYSTALS):N0}");
        }
        else
        {
            lblPaleGold.SetText($"PG: {cityState.GetLocalCurrency(GameConstants.CURRENCY_PALE_GOLD):N0}");
            lblVoidCrystals.SetText("VC: 0");
        }
    }

    // ── Panel Animation ───────────────────────────────────────────────────────

    private void ShowPanel(RectTransform panel)
    {
        panel.gameObject.SetActive(true);
        panel.anchoredPosition = new Vector2(0, PANEL_OFFSCREEN_Y);
        panel.DOAnchorPosY(0f, PANEL_SLIDE_DURATION).SetEase(Ease.OutCubic);
    }

    private void HidePanel(RectTransform panel, bool animate = true)
    {
        if (!panel.gameObject.activeSelf) return;
        if (animate)
            panel.DOAnchorPosY(PANEL_OFFSCREEN_Y, PANEL_SLIDE_DURATION)
                 .SetEase(Ease.InCubic)
                 .OnComplete(() => panel.gameObject.SetActive(false));
        else
            panel.gameObject.SetActive(false);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static string FormatCost(System.Collections.Generic.List<ResourceCost> costs)
    {
        if (costs == null || costs.Count == 0) return "Free";
        var parts = new System.Collections.Generic.List<string>();
        foreach (var c in costs)
            parts.Add($"{c.amount:N0} {c.currencyCode}");
        return string.Join(", ", parts);
    }

    private static string FormatTime(float seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        if (ts.TotalHours >= 1) return $"{(int)ts.TotalHours}h {ts.Minutes}m";
        if (ts.TotalMinutes >= 1) return $"{(int)ts.TotalMinutes}m {ts.Seconds}s";
        return $"{(int)ts.TotalSeconds}s";
    }
}
