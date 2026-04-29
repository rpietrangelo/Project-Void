using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;

/// <summary>
/// Controls the World Map scene: tilemap rendering, camera, city markers, tile selection.
/// Hierarchy required:
///   Grid > Tilemap_Ground, Tilemap_Overlay, Tilemap_Objects
///   Layer_Cities (parent for CityMarker prefabs)
///   CinemachineCamera with CinemachineConfiner
/// </summary>
public class WorldMapController : MonoBehaviour
{
    [Header("Tilemap")]
    [SerializeField] private Tilemap tilemapGround;
    [SerializeField] private Tilemap tilemapOverlay;
    [SerializeField] private Tilemap tilemapObjects;

    [Header("Tile Assets")]
    [SerializeField] private TileBase tilePlains;
    [SerializeField] private TileBase tileForest;
    [SerializeField] private TileBase tileMountain;
    [SerializeField] private TileBase tileRuins;
    [SerializeField] private TileBase tileVoidRift;

    [Header("City Markers")]
    [SerializeField] private Transform cityMarkersParent;
    [SerializeField] private GameObject cityMarkerPrefab;

    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera vcam;
    [SerializeField] private float zoomMin = 3f;
    [SerializeField] private float zoomMax = 15f;

    [Header("UI")]
    [SerializeField] private WorldMapUIController uiController;

    private Camera _mainCam;
    private FirebaseFirestore _db;
    private Vector3 _lastQueryPosition = Vector3.one * float.MaxValue;
    private const float QUERY_TRIGGER_DISTANCE = 10f;
    private Dictionary<string, GameObject> _cityMarkers = new();
    private const int MAP_GEN_WIDTH = 100;
    private const int MAP_GEN_HEIGHT = 100;

    private void Start()
    {
        _mainCam = Camera.main;
        _db = FirebaseFirestore.DefaultInstance;

        GenerateProceduralMap();
        LoadNearbyCitiesAsync().Forget();
    }

    private void Update()
    {
        // Re-query cities if camera moved significantly
        var camPos = vcam.transform.position;
        if (Vector3.Distance(camPos, _lastQueryPosition) > QUERY_TRIGGER_DISTANCE)
        {
            _lastQueryPosition = camPos;
            LoadNearbyCitiesAsync().Forget();
        }
    }

    // ── Map Generation ────────────────────────────────────────────────────────

    private void GenerateProceduralMap()
    {
        var rand = new System.Random(42); // fixed seed for determinism

        for (int x = 0; x < MAP_GEN_WIDTH; x++)
        {
            for (int y = 0; y < MAP_GEN_HEIGHT; y++)
            {
                var tile = PickTile(rand, x, y);
                tilemapGround.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }

    private TileBase PickTile(System.Random rand, int x, int y)
    {
        float r = (float)rand.NextDouble();
        if (r < 0.03f) return tileVoidRift;
        if (r < 0.10f) return tileRuins;
        if (r < 0.22f) return tileMountain;
        if (r < 0.40f) return tileForest;
        return tilePlains;
    }

    // ── City Markers ─────────────────────────────────────────────────────────

    private async UniTaskVoid LoadNearbyCitiesAsync()
    {
        var camPos = vcam.transform.position;
        var orthoSize = vcam.m_Lens.OrthographicSize;

        // Don't load markers when very zoomed out (performance)
        if (orthoSize > 10f)
        {
            foreach (var m in _cityMarkers.Values) m.SetActive(false);
            return;
        }
        foreach (var m in _cityMarkers.Values) m.SetActive(true);

        float halfW = orthoSize * _mainCam.aspect + 5f;
        float halfH = orthoSize + 5f;
        int minX = Mathf.FloorToInt(camPos.x - halfW);
        int maxX = Mathf.CeilToInt(camPos.x + halfW);
        int minY = Mathf.FloorToInt(camPos.y - halfH);
        int maxY = Mathf.CeilToInt(camPos.y + halfH);

        // Clamp to map bounds
        minX = Mathf.Max(0, minX); maxX = Mathf.Min(GameConstants.MAP_WIDTH, maxX);
        minY = Mathf.Max(0, minY); maxY = Mathf.Min(GameConstants.MAP_HEIGHT, maxY);

        try
        {
            // Requires composite Firestore index: worldX + worldY
            var query = await _db.Collection(GameConstants.COL_CITIES)
                                 .WhereGreaterThanOrEqualTo("worldX", minX)
                                 .WhereLessThanOrEqualTo("worldX", maxX)
                                 .GetSnapshotAsync()
                                 .AsUniTask();

            foreach (var doc in query.Documents)
            {
                var cityDoc = doc.ConvertTo<CityDocument>();
                if (cityDoc.worldY < minY || cityDoc.worldY > maxY) continue;

                if (!_cityMarkers.ContainsKey(doc.Id))
                    SpawnCityMarker(doc.Id, cityDoc);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[WorldMap] LoadNearbyCities error: {ex.Message}");
        }
    }

    private void SpawnCityMarker(string cityId, CityDocument city)
    {
        var markerGO = Instantiate(cityMarkerPrefab, cityMarkersParent);
        markerGO.transform.position = new Vector3(city.worldX, city.worldY, 0f);

        var marker = markerGO.GetComponent<CityMarker>();
        if (marker != null)
        {
            bool isOwn = city.ownerId == ServiceLocator.Instance.Get<FirebaseService>()?.UserId;
            marker.Setup(city, isOwn, OnCityMarkerTapped);
        }
        _cityMarkers[cityId] = markerGO;
    }

    private void OnCityMarkerTapped(CityDocument city)
    {
        uiController?.ShowTileInfo(city);
    }

    // ── Tile Tap ──────────────────────────────────────────────────────────────

    private void OnMouseDown()
    {
        // Handled per-tile via raycast in WorldMapInputHandler (see below)
    }
}

/// <summary>
/// Handles world map input: tap tile, pinch zoom, drag pan.
/// Attach to the same GameObject as WorldMapController.
/// </summary>
public class WorldMapInputHandler : MonoBehaviour
{
    [SerializeField] private WorldMapController mapController;
    [SerializeField] private CinemachineVirtualCamera vcam;
    [SerializeField] private WorldMapUIController uiController;
    [SerializeField] private float zoomMin = 3f;
    [SerializeField] private float zoomMax = 15f;
    [SerializeField] private Vector2 mapMin;
    [SerializeField] private Vector2 mapMax;

    private Camera _cam;
    private bool _isPanning;
    private Vector3 _panOrigin;
    private Vector3 _panCamOrigin;

    private void Start() => _cam = Camera.main;

    private void Update()
    {
        HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                _isPanning = true;
                _panOrigin = _cam.ScreenToWorldPoint(touch.position);
                _panCamOrigin = vcam.transform.position;
            }
            else if (touch.phase == TouchPhase.Moved && _isPanning)
            {
                var delta = _cam.ScreenToWorldPoint(touch.position) - _panOrigin;
                var newPos = _panCamOrigin - delta;
                newPos.x = Mathf.Clamp(newPos.x, mapMin.x, mapMax.x);
                newPos.y = Mathf.Clamp(newPos.y, mapMin.y, mapMax.y);
                newPos.z = vcam.transform.position.z;
                vcam.transform.position = newPos;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                _isPanning = false;
            }
        }
        else if (Input.touchCount == 2)
        {
            HandlePinchZoom();
        }

#if UNITY_EDITOR
        HandleMouseInput();
#endif
    }

    private void HandlePinchZoom()
    {
        var t0 = Input.GetTouch(0);
        var t1 = Input.GetTouch(1);
        var prevT0 = t0.position - t0.deltaPosition;
        var prevT1 = t1.position - t1.deltaPosition;
        float prevDist = (prevT0 - prevT1).magnitude;
        float currDist = (t0.position - t1.position).magnitude;
        float delta = prevDist - currDist;

        var lens = vcam.m_Lens;
        lens.OrthographicSize = Mathf.Clamp(lens.OrthographicSize + delta * 0.01f, zoomMin, zoomMax);
        vcam.m_Lens = lens;
    }

#if UNITY_EDITOR
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isPanning = true;
            _panOrigin = _cam.ScreenToWorldPoint(Input.mousePosition);
            _panCamOrigin = vcam.transform.position;
        }
        else if (Input.GetMouseButton(0) && _isPanning)
        {
            var delta = _cam.ScreenToWorldPoint(Input.mousePosition) - _panOrigin;
            var newPos = _panCamOrigin - delta;
            newPos.x = Mathf.Clamp(newPos.x, mapMin.x, mapMax.x);
            newPos.y = Mathf.Clamp(newPos.y, mapMin.y, mapMax.y);
            newPos.z = vcam.transform.position.z;
            vcam.transform.position = newPos;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _isPanning = false;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            var lens = vcam.m_Lens;
            lens.OrthographicSize = Mathf.Clamp(lens.OrthographicSize - scroll * 3f, zoomMin, zoomMax);
            vcam.m_Lens = lens;
        }
    }
#endif
}

[FirestoreData]
public class CityDocument
{
    [FirestoreProperty] public string ownerId { get; set; }
    [FirestoreProperty] public string ownerName { get; set; }
    [FirestoreProperty] public string faction { get; set; }
    [FirestoreProperty] public string allianceId { get; set; }
    [FirestoreProperty] public string allianceName { get; set; }
    [FirestoreProperty] public int worldX { get; set; }
    [FirestoreProperty] public int worldY { get; set; }
    [FirestoreProperty] public long power { get; set; }
    [FirestoreProperty] public string cityName { get; set; }
}
