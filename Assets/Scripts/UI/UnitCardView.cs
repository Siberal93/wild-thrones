using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitCardView : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private Vector2 cardSize = new Vector2(360, 520);
    [SerializeField] private Vector2 margin = new Vector2(18, 18);

    [Header("Colors")]
    [SerializeField] private Color panelColor = new Color(0.10f, 0.12f, 0.16f, 0.95f);
    [SerializeField] private Color badgeColor = new Color(0.65f, 0.85f, 1.00f, 1.00f);

    RectTransform root;
    Image panel;

    Image art;
    TMP_Text title;
    TMP_Text body;

    RectTransform cdBadgeRt;
    Image cdBadgeBg;
    TMP_Text cdBadgeText;

    RectTransform hpBadgeRt;
    Image hpBadgeBg;
    TMP_Text hpBadgeText;

    RectTransform atkBadgeRt;
    Image atkBadgeBg;
    TMP_Text atkBadgeText;

    Canvas canvas;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        BuildIfNeeded();
        Hide();
    }

    void BuildIfNeeded()
    {
        if (root != null) return;

        // Root panel
        var go = new GameObject("CardRoot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform, false);

        root = go.GetComponent<RectTransform>();
        root.sizeDelta = cardSize;
        root.pivot = new Vector2(0, 1); // top-left by default

        panel = go.GetComponent<Image>();
        panel.color = panelColor;

        panel.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        panel.type = Image.Type.Sliced; // opzionale, anche Simple va bene

        // Art (top half)
        art = CreateImage("Art", root, anchorMin: new Vector2(0, 0.52f), anchorMax: new Vector2(1, 1), padding: new Vector4(12, 12, 12, 12));
        art.color = new Color(1, 1, 1, 0.12f); // placeholder
        art.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);

        // Title
        title = CreateTMP("Title", root, new Vector2(0, 0.44f), new Vector2(1, 0.52f), new Vector4(14, 6, 14, 4), 26, FontStyles.Bold);

        // Body
        body = CreateTMP("Body", root, new Vector2(0, 0.10f), new Vector2(1, 0.44f), new Vector4(14, 8, 14, 8), 18, FontStyles.Normal);
        body.alignment = TextAlignmentOptions.TopLeft;

        // CD badge (top-right)
        (cdBadgeRt, cdBadgeBg, cdBadgeText) = CreateBadge("CD", root, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-54, -34), 44, badgeColor);

        // HP badge (bottom-right)
        (hpBadgeRt, hpBadgeBg, hpBadgeText) = CreateBadge("HP", root, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-54, 54), 52, new Color(1.0f, 0.35f, 0.35f, 1f));

        // ATK badge (bottom-left)  (tu avevi scritto “basso a destra” anche per attacco, ma li separo: ATK a sx, HP a dx)
        (atkBadgeRt, atkBadgeBg, atkBadgeText) = CreateBadge("ATK", root, new Vector2(0, 0), new Vector2(0, 0), new Vector2(54, 54), 52, new Color(1.0f, 0.80f, 0.35f, 1f));
    }


    public void Show(UnitRuntime u, Vector2 screenPoint)
    {
        if (u == null) return;
        BuildIfNeeded();
        root.gameObject.SetActive(true);

        title.text = u.displayName;
        cdBadgeText.text = u.cd.ToString();
        hpBadgeText.text = u.hp.ToString();
        atkBadgeText.text = u.atk.ToString();
        body.text = BuildBody(u);

        ShowCenteredOn(screenPoint);
    }

    string BuildBody(UnitRuntime u)
    {
        // per ora “placeholder”, poi lo alimenti da json (skills/descrizione)
        string traits = (u.traits != null && u.traits.Count > 0) ? string.Join(", ", u.traits) : "-";
        return $"Special: {traits}\n\nDescrizione:\n(placeholder testo unità)";
    }

    void PositionNear(Vector2 screenPoint)
    {
        var canvasRt = (RectTransform)canvas.transform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRt, screenPoint, null, out var localPoint
        );

        // card centrata sul canvas (anchor 0.5,0.5)
        root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f);

        // pivot centro, così non “spara” in alto
        root.pivot = new Vector2(0.5f, 0.5f);

        // offset base (a destra e un po’ su)
        Vector2 offset = new Vector2(220f, 40f);

        // se non c’è spazio a destra -> mettila a sinistra
        float halfW = root.rect.width * 0.5f;
        float canvasHalfW = canvasRt.rect.width * 0.5f;

        bool canGoRight = (localPoint.x + offset.x + halfW) <= canvasHalfW;
        if (!canGoRight) offset.x = -offset.x;

        root.anchoredPosition = localPoint + offset;

        ClampInsideCanvas(root, canvasRt, 10f);
    }

    static void ClampInsideCanvas(RectTransform panel, RectTransform canvas, float padding)
    {
        Vector2 pos = panel.anchoredPosition;

        float halfW = panel.rect.width * 0.5f;
        float halfH = panel.rect.height * 0.5f;

        float cHalfW = canvas.rect.width * 0.5f;
        float cHalfH = canvas.rect.height * 0.5f;

        pos.x = Mathf.Clamp(pos.x, -cHalfW + halfW + padding, cHalfW - halfW - padding);
        pos.y = Mathf.Clamp(pos.y, -cHalfH + halfH + padding, cHalfH - halfH - padding);

        panel.anchoredPosition = pos;
    }

    // --- helpers UI ---
    Image CreateImage(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector4 padding)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(padding.x, padding.w);
        rt.offsetMax = new Vector2(-padding.z, -padding.y);
        return go.GetComponent<Image>();
    }

    TMP_Text CreateTMP(string name, RectTransform parent, Vector2 aMin, Vector2 aMax, Vector4 padding, float size, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.offsetMin = new Vector2(padding.x, padding.w);
        rt.offsetMax = new Vector2(-padding.z, -padding.y);

        var t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize = size;
        t.fontStyle = style;
        t.color = Color.white;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        return t;
    }

    (RectTransform, Image, TMP_Text) CreateBadge(string name, RectTransform parent, Vector2 aMin, Vector2 aMax, Vector2 anchored, float size, Color bg)
    {
        var go = new GameObject($"{name}Badge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.sizeDelta = new Vector2(size, size);
        rt.anchoredPosition = anchored;

        var img = go.GetComponent<Image>();
        img.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        img.color = bg;

        var txtGo = new GameObject("Text", typeof(RectTransform));
        txtGo.transform.SetParent(go.transform, false);
        var trt = txtGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        var t = txtGo.AddComponent<TextMeshProUGUI>();
        t.alignment = TextAlignmentOptions.Center;
        t.fontSize = size * 0.5f;
        t.fontStyle = FontStyles.Bold;
        t.color = Color.black;

        return (rt, img, t);
    }

    public void ShowCenteredOn(Vector2 screenPoint)
    {
        BuildIfNeeded();
        root.gameObject.SetActive(true);

        var canvasRt = (RectTransform)canvas.transform;

        // Overlay: camera null
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPoint, null, out var local);

        // centro esatto
        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);

        root.anchoredPosition = local;
    }

    public void Hide()
    {
        BuildIfNeeded();
        root.gameObject.SetActive(false);
    }
}
