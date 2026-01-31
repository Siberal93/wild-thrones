using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public partial class BoardBootstrap : MonoBehaviour
{
    [Header("Layout")]
    public Vector2 boardAnchor = new Vector2(-0.8f, 1.5f);
    public float boardCellSize = 1.55f;
    public float boardRowGap = 0.3f;
    public float boardColGap = 0.85f;


    [Header("Visual")]
    public float cellSize = 1.8f;
    public float rowGap = 0.3f;
    public float colGap = 1.2f;

    // Data JSON
    private JObject manifest, unitsJson, encountersJson;

    // Board
    private readonly Dictionary<string, Transform> slotMap = new Dictionary<string, Transform>();
    private readonly Dictionary<string, UnitRuntime> occupancy = new Dictionary<string, UnitRuntime>();
    private readonly Dictionary<string, LineRenderer> slotOutline = new Dictionary<string, LineRenderer>();
    [SerializeField] private Canvas cardCanvas;
    private UnitCardView cardView;
    public UnitCardView CardView => cardView;
    // Highlight
    private string highlightedSlot = null;

    public static BoardBootstrap Instance;

    void Awake() => Instance = this;

    void Start()
    {
        LoadData();

        cellSize = boardCellSize;
        rowGap = boardRowGap;
        colGap = boardColGap;

        DrawGrid12(anchor: boardAnchor);

        EnsureCardUI();
        // 2) Spawn wave 1 dell'encounter di test
        SpawnWave("enc_forest_boss_tasso", 0);
    }

    UnitCardView EnsureCardUI()
    {
        if (cardView != null) return cardView;

        var canvas = EnsureCardCanvas();

        var go = new GameObject("UnitCardView");
        go.transform.SetParent(canvas.transform, false);

        cardView = go.AddComponent<UnitCardView>();
        cardView.Hide();

        return cardView;
    }


    Canvas EnsureCardCanvas()
    {
        if (cardCanvas != null) return cardCanvas;

        var go = new GameObject("CardCanvas");
        // NON metterlo sotto BoardBootstrap se hai scale strane nel parent
        // go.transform.SetParent(this.transform, false);

        var c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 5000; // sopra a tutto

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();

        cardCanvas = c;
        return cardCanvas;
    }

    public bool IsWorldInsideSlot(string slotKey, Vector2 worldPos)
    {
        if (string.IsNullOrEmpty(slotKey)) return false;
        if (!slotMap.TryGetValue(slotKey, out var t)) return false;

        var c = (Vector2)t.position;
        float half = cellSize * 0.5f;
        return Mathf.Abs(worldPos.x - c.x) <= half && Mathf.Abs(worldPos.y - c.y) <= half;
    }

    public Vector2 GetSideCenterWorld(bool wantEnemySide)
    {
        // wantEnemySide=true => centro EN_*
        // wantEnemySide=false => centro AL_*
        Vector2 sum = Vector2.zero;
        int count = 0;

        foreach (var kv in slotMap)
        {
            string key = kv.Key; // "EN_FRONT_LEFT" ecc
            bool isEnemy = key.StartsWith("EN_");
            if (isEnemy != wantEnemySide) continue;

            sum += (Vector2)kv.Value.position;
            count++;
        }

        if (count == 0) return Vector2.zero;
        return sum / count;
    }

    public Vector2 GetOppositeGridCenterScreen(UnitRuntime u)
    {
        var cam = Camera.main;
        if (cam == null) return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        bool unitIsEnemy = u != null && (u.isEnemy || (u.slotKey != null && u.slotKey.StartsWith("EN_")));
        bool targetEnemySide = !unitIsEnemy; // opposto

        Vector2 world = GetSideCenterWorld(targetEnemySide);
        Vector3 sp = cam.WorldToScreenPoint(new Vector3(world.x, world.y, 0));
        return new Vector2(sp.x, sp.y);
    }

}
