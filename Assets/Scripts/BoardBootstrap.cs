using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TMPro;

public class BoardBootstrap : MonoBehaviour
{
    [Header("Visual")]
    public float cellSize = 1.8f;
    public float rowGap = 1.2f;
    public float colGap = 1.2f;

    [SerializeField] private Canvas worldCanvas;

    private JObject manifest, unitsJson, encountersJson;

    private Dictionary<string, Transform> slotMap = new Dictionary<string, Transform>();


    void Start()
    {
        LoadData();
        // 1) Disegna griglie
        DrawGrid12(anchor: Vector2.zero);
        // 2) Spawna Wave 1 del boss encounter
        SpawnWave(encounterId: "enc_forest_boss_tasso", waveIndex: 0, enemyAnchor: new Vector2(+3.5f, 0f));
    }

    void LoadData()
    {
        string basePath = Path.Combine(Application.streamingAssetsPath, "data");
        string manifestPath = Path.Combine(basePath, "manifest.json");

        manifest = JObject.Parse(File.ReadAllText(manifestPath));

        string unitsFile = manifest["files"]?["units"]?.ToString();
        string encountersFile = manifest["files"]?["encounters_act1"]?.ToString();

        unitsJson = JObject.Parse(File.ReadAllText(Path.Combine(basePath, unitsFile)));
        encountersJson = JObject.Parse(File.ReadAllText(Path.Combine(basePath, encountersFile)));
    }

    void SpawnWave(string encounterId, int waveIndex, Vector2 enemyAnchor)
    {
        var encArr = encountersJson["encounters"] as JArray;
        var encounter = FindById(encArr, encounterId);
        if (encounter == null)
        {
            Debug.LogError($"[BoardBootstrap] Encounter not found: {encounterId}");
            return;
        }

        var waves = encounter["waves"] as JArray;
        var wave = waves?[waveIndex] as JObject;
        var spawns = wave?["spawn"] as JArray;

        if (spawns == null)
        {
            Debug.LogError("[BoardBootstrap] Wave spawn list missing.");
            return;
        }

        int cursor = 0;

        foreach (var unitIdTok in spawns)
        {
            string unitId = unitIdTok.ToString();
            var unit = FindUnit(unitId);

            if (unit == null)
            {
                Debug.LogError($"[BoardBootstrap] Unit not found: {unitId}");
                continue;
            }

            // 0..2 => EN_FRONT, 3..5 => EN_BACK
            bool front = cursor < 3;
            int col = front ? 2 : 3;          // 2=EN_FRONT, 3=EN_BACK
            int lane = cursor % 3;            // 0=LEFT, 1=CENTER, 2=RIGHT
            cursor++;

            string slotKey = SlotKey12(lane, col);

            if (!slotMap.TryGetValue(slotKey, out var slotTr))
            {
                Debug.LogError($"[BoardBootstrap] Slot not found in slotMap: {slotKey}");
                continue;
            }

            Vector2 pos = slotTr.position;
            CreateUnitToken(unit, pos, isEnemy: true, slotKey: slotKey);
        }

    }

    JObject FindUnit(string unitId)
    {
        var arr = unitsJson["units"] as JArray;
        return FindById(arr, unitId);
    }

    JObject FindById(JArray arr, string id)
    {
        if (arr == null) return null;
        foreach (var item in arr)
        {
            if (item is JObject obj && obj["id"]?.ToString() == id)
                return obj;
        }
        return null;
    }

    Vector2 SlotToWorld(Vector2 anchor, int row, int col)
    {
        // row 0 (front) sopra, row 1 (back) sotto
        float x = anchor.x + (col - 1) * (cellSize + colGap);
        float y = anchor.y + (row == 0 ? +(cellSize / 2 + rowGap / 2) : -(cellSize / 2 + rowGap / 2));
        return new Vector2(x, y);
    }
    GameObject CreateQuad(string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        if (go.GetComponent<Collider2D>() == null)
            go.AddComponent<BoxCollider2D>();

        // Sprite 1x1 bianco built-in: creiamolo runtime
        var tex = Texture2D.whiteTexture;
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);
        sr.sprite = sprite;

        go.transform.position = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        return go;
    }

    Vector2 SlotToWorld12(Vector2 anchor, int row, int col)
    {
        // col 0..3 va da sinistra a destra
        float x = anchor.x + (col - 1.5f) * (cellSize + colGap);

        // row 0..2 dall'alto verso il basso
        float y = anchor.y + (1 - row) * (cellSize + rowGap);

        return new Vector2(x, y);
    }
    GameObject CreateLabelTextMesh(string text, Transform parent, Vector2 localOffset, int fontSize = 40)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, worldPositionStays: false);
        go.transform.localPosition = new Vector3(localOffset.x, localOffset.y, 0);

        var tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = fontSize;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.characterSize = 0.05f; // scala testo nel mondo
        tm.color = Color.white;

        return go;
    }

    GameObject CreateUnitToken(JObject unit, Vector2 pos, bool isEnemy, string slotKey)
    {
        string name = unit["name"]?.ToString() ?? unit["id"]?.ToString();
        int hp = unit["stats"]?["hp"]?.Value<int>() ?? 0;
        int cd = unit["stats"]?["cdBase"]?.Value<int>() ?? 0;

        float r = cellSize * 0.45f;

        // colori “nemico” (bordo/riempimento li sistemiamo sotto)
        Color border = new Color(0.75f, 0.12f, 0.12f, 1f);

        var go = CreateCircleOutline($"UNIT_{name}", pos, r, border, width: 0.08f, segments: 64);
        go.transform.SetParent(this.transform, true);

        var col = go.GetComponent<CircleCollider2D>() ?? go.AddComponent<CircleCollider2D>();
        col.radius = r;
        col.isTrigger = true;

        var rt = go.AddComponent<UnitRuntime>();
        rt.InitFromJson(unit, isEnemy);
        rt.slotKey = slotKey;
        rt.slotAbbrev = AbbrevSlot(slotKey);

        if (isEnemy)
        {
            // fill rosso scuro semi-opaco
            CreateCircleFill(go.transform, r * 0.95f, new Color(0.55f, 0.05f, 0.05f, 0.5f));
        }

        rt.labelTmp = CreateLabelTMP("", go.transform, Vector2.zero, 2f);
        rt.RefreshLabel();

        Debug.Log($"[Spawn] {rt.displayName}#{rt.instanceId} HP:{hp} CD:{cd} SLOT:{slotKey}");
        return go;
    }

    void DrawGrid12(Vector2 anchor)
    {
        EnsureWorldCanvas();

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                Vector2 pos = SlotToWorld12(anchor, row, col);
                string slotKey = SlotKey12(row, col);

                var go = CreateSlotOutline($"SLOT_{slotKey}", pos, cellSize);
                go.transform.SetParent(this.transform, true);

                slotMap[slotKey] = go.transform;

                bool isEnemy = (col >= 2);
                var c = isEnemy ? new Color(1f, 0.4f, 0.4f, 1f) : new Color(0.4f, 1f, 0.4f, 1f);
                var lr = go.GetComponent<LineRenderer>();
                lr.startColor = c;
                lr.endColor = c;
                if (isEnemy)
                {
                    // rosso semi-trasparente dentro lo slot
                    CreateSlotFill(go.transform, cellSize * 0.98f, new Color(1f, 0.2f, 0.2f, 1f));
                }

                // label slot (piccola, in alto)
                CreateLabelTMP(AbbrevSlot(slotKey), go.transform, Vector2.zero, 2f);
            }
        }
    }

    TextMeshPro CreateLabelTMP(string text, Transform parent, Vector2 localOffset, float fontSize)
    {
        var go = new GameObject("LabelTMP");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(localOffset.x, localOffset.y, -0.1f);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.color = Color.white;

        tmp.extraPadding = true;
        tmp.fontStyle = FontStyles.Bold;

        // box “ragionevole” per non tagliare, senza farlo gigante
        tmp.rectTransform.sizeDelta = new Vector2(2.5f, 1.5f);

        return tmp;
    }

    string SlotKey12(int row, int col)
    {
        // lane NON include AL/EN
        string lane = col switch
        {
            0 => "BACK",
            1 => "FRONT",
            2 => "FRONT",
            3 => "BACK",
            _ => "UNK"
        };

        string side = (col >= 2) ? "EN" : "AL";

        string line = row switch
        {
            0 => "LEFT",
            1 => "CENTER",
            2 => "RIGHT",
            _ => "UNK"
        };

        return $"{side}_{lane}_{line}";
    }

    GameObject CreateSlotOutline(string name, Vector2 center, float size)
    {
        var go = new GameObject(name);
        go.transform.position = new Vector3(center.x, center.y, 0);

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = 4;
        lr.startWidth = 0.06f;
        lr.endWidth = 0.06f;
        lr.material = new Material(Shader.Find("Sprites/Default"));

        float h = size * 0.5f;
        lr.SetPosition(0, new Vector3(-h, -h, 0));
        lr.SetPosition(1, new Vector3(-h, h, 0));
        lr.SetPosition(2, new Vector3(h, h, 0));
        lr.SetPosition(3, new Vector3(h, -h, 0));

        return go;
    }

    GameObject CreateSlotFill(Transform slot, float size, Color fill)
    {
        var go = new GameObject("Fill");
        go.transform.SetParent(slot, false);
        go.transform.localPosition = new Vector3(0, 0, 0.2f); // dietro all’unità, davanti allo sfondo
        go.transform.localScale = new Vector3(size, size, 1);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        sr.color = fill;
        sr.sortingOrder = -10; // slot fill dietro alle unità
        return go;
    }


    GameObject CreateCircleOutline(string name, Vector2 center, float radius, Color color, float width = 0.06f, int segments = 48)
    {
        var go = new GameObject(name);

        go.transform.position = new Vector3(center.x, center.y, -1f); // unità davanti

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = segments;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.startColor = color;
        lr.endColor = color;
        lr.material = new Material(Shader.Find("Sprites/Default"));

        for (int i = 0; i < segments; i++)
        {
            float a = (i / (float)segments) * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f));
        }

        // Collider (fondamentale per OnMouseDown)
        var col = go.AddComponent<CircleCollider2D>();
        col.radius = radius;

        return go;
    }

    GameObject CreateCircleFill(Transform parent, float radius, Color fill)
    {
        var go = new GameObject("CircleFill");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(0, 0, 0.1f); // sotto al bordo, sopra al fill slot

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        sr.color = fill;
        sr.sortingOrder = 10;

        // scala in modo che riempia circa come il cerchio
        float d = radius * 2f;
        go.transform.localScale = new Vector3(d, d, 1f);

        // NB: è un quad; visivamente va bene per debug.
        // Se vuoi un vero disco, poi mettiamo uno sprite circolare.
        return go;
    }


    string AbbrevSlot(string slotKey)
    {
        // slotKey: EN_FRONT_RIGHT
        var p = slotKey.Split('_'); // EN, FRONT, RIGHT
        if (p.Length < 3) return slotKey;

        string side = (p[0] == "AL") ? "G" : "N";
        string lane = (p[1] == "FRONT") ? "F" : "R";
        string col = p[2] == "LEFT" ? "S" : (p[2] == "CENTER" ? "C" : "D");

        return $"{side}{lane}{col}";
    }

    Canvas EnsureWorldCanvas()
    {
        if (worldCanvas != null) return worldCanvas;

        var go = new GameObject("WorldCanvas");
        go.transform.SetParent(this.transform, false);

        var c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;

        var scaler = go.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 50f; // nitidezza

        go.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // scala canvas “ragionevole” in world units
        var rt = c.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(2000, 1200);
        rt.localScale = Vector3.one * 0.01f;

        worldCanvas = c;
        return worldCanvas;
    }

}
