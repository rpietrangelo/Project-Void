using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attached to each city marker prefab on the world map.
/// Prefab hierarchy:
///   CityMarker
///     SpriteRenderer (faction-colored base)
///     FlagSprite (faction icon)
///     Canvas (World Space)
///       NameLabel (TMP)
///       PowerLabel (TMP)
///     Collider2D (for tap detection)
/// </summary>
public class CityMarker : MonoBehaviour
{
    [SerializeField] private SpriteRenderer baseSprite;
    [SerializeField] private SpriteRenderer flagSprite;
    [SerializeField] private TextMeshPro lblName;
    [SerializeField] private TextMeshPro lblPower;
    [SerializeField] private GameObject homeIndicator;   // visible only on player's own city

    [Header("Faction Colors")]
    [SerializeField] private Color colorOrder    = new Color(0.9f, 0.85f, 0.4f);
    [SerializeField] private Color colorCult     = new Color(0.5f, 0.2f, 0.7f);
    [SerializeField] private Color colorWanderer = new Color(0.3f, 0.7f, 0.4f);

    [Header("Faction Sprites")]
    [SerializeField] private Sprite spriteOrderFlag;
    [SerializeField] private Sprite spriteCultFlag;
    [SerializeField] private Sprite spriteWandererFlag;

    private CityDocument _cityData;
    private Action<CityDocument> _onTapped;
    private bool _isOwn;

    public void Setup(CityDocument city, bool isOwn, Action<CityDocument> onTapped)
    {
        _cityData = city;
        _isOwn = isOwn;
        _onTapped = onTapped;

        if (lblName) lblName.SetText(city.ownerName ?? "Unknown");
        if (lblPower) lblPower.SetText($"{city.power:N0}");
        if (homeIndicator) homeIndicator.SetActive(isOwn);

        ApplyFactionVisuals(city.faction);
    }

    private void ApplyFactionVisuals(string faction)
    {
        Color color;
        Sprite flag;

        switch (faction?.ToLower())
        {
            case "order":
                color = colorOrder;
                flag = spriteOrderFlag;
                break;
            case "cult":
                color = colorCult;
                flag = spriteCultFlag;
                break;
            case "wanderer":
                color = colorWanderer;
                flag = spriteWandererFlag;
                break;
            default:
                color = Color.grey;
                flag = null;
                break;
        }

        if (baseSprite) baseSprite.color = color;
        if (flagSprite && flag != null) flagSprite.sprite = flag;
    }

    private void OnMouseDown()
    {
        _onTapped?.Invoke(_cityData);
    }

    /// <summary>
    /// Hide labels when zoomed out (called by WorldMapController based on ortho size).
    /// </summary>
    public void SetLabelsVisible(bool visible)
    {
        if (lblName) lblName.gameObject.SetActive(visible);
        if (lblPower) lblPower.gameObject.SetActive(visible);
    }
}
