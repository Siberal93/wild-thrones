using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;

public class PileViewerView : MonoBehaviour
{
    RectTransform root;
    TMP_Text title;
    Transform content;
    Action onClose;

    public static PileViewerView Create(Transform parent, Action onClose)
    {
        var go = new GameObject("PileViewer", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var v = go.AddComponent<PileViewerView>();
        v.onClose = onClose;
        v.Build();
        return v;
    }

    void Build()
    {
        root = GetComponent<RectTransform>();
        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.sizeDelta = new Vector2(560, 520);
        root.anchoredPosition = Vector2.zero;

        var bg = gameObject.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.06f, 0.08f, 0.92f);

        // header
        title = CreateTMP("Title", transform, 26, FontStyles.Bold);
        title.alignment = TextAlignmentOptions.TopLeft;
        title.rectTransform.offsetMin = new Vector2(18, 18);
        title.rectTransform.offsetMax = new Vector2(-18, -18);

        // close button
        var closeGo = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
        closeGo.transform.SetParent(transform, false);
        var crt = closeGo.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(1, 1);
        crt.anchorMax = new Vector2(1, 1);
        crt.pivot = new Vector2(1, 1);
        crt.sizeDelta = new Vector2(44, 44);
        crt.anchoredPosition = new Vector2(-12, -12);
        closeGo.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
        closeGo.GetComponent<Button>().onClick.AddListener(() => onClose?.Invoke());

        var x = CreateTMP("X", closeGo.transform, 22, FontStyles.Bold);
        x.text = "X";
        x.alignment = TextAlignmentOptions.Center;

        // scroll
        var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollGo.transform.SetParent(transform, false);
        var srt = scrollGo.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0, 0);
        srt.anchorMax = new Vector2(1, 1);
        srt.offsetMin = new Vector2(18, 18);
        srt.offsetMax = new Vector2(-18, -70);
        scrollGo.GetComponent<Image>().color = new Color(0, 0, 0, 0.2f);

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
        viewport.transform.SetParent(scrollGo.transform, false);
        var vrt = viewport.GetComponent<RectTransform>();
        vrt.anchorMin = Vector2.zero;
        vrt.anchorMax = Vector2.one;
        vrt.offsetMin = Vector2.zero;
        vrt.offsetMax = Vector2.zero;
        viewport.GetComponent<Image>().color = new Color(0, 0, 0, 0.0f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentGo.transform.SetParent(viewport.transform, false);
        content = contentGo.transform;

        var crt2 = contentGo.GetComponent<RectTransform>();
        crt2.anchorMin = new Vector2(0, 1);
        crt2.anchorMax = new Vector2(1, 1);
        crt2.pivot = new Vector2(0.5f, 1);

        var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 10;
        vlg.childAlignment = TextAnchor.UpperLeft;
        vlg.padding = new RectOffset(10, 10, 10, 10);

        var fitter = contentGo.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var sr = scrollGo.GetComponent<ScrollRect>();
        sr.viewport = vrt;
        sr.content = crt2;
        sr.horizontal = false;
    }

    public void Show(string pileName, List<CardInstance> cards, bool sort)
    {
        gameObject.SetActive(true);

        title.text = pileName;

        foreach (Transform ch in content) Destroy(ch.gameObject);

        IEnumerable<CardInstance> seq = cards ?? Enumerable.Empty<CardInstance>();
        if (sort)
            seq = seq.OrderBy(c => c.def.cost).ThenBy(c => c.def.name);

        foreach (var c in seq)
        {
            var row = new GameObject("Row", typeof(RectTransform));
            row.transform.SetParent(content, false);

            var t = row.AddComponent<TextMeshProUGUI>();
            t.fontSize = 18;
            t.color = Color.white;
            t.text = $"{c.def.name}  (C:{c.def.cost})  - {c.def.desc}";
        }
    }

    public void Hide() => gameObject.SetActive(false);

    static TMP_Text CreateTMP(string name, Transform parent, float size, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize = size;
        t.fontStyle = style;
        t.color = Color.white;
        var rt = t.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return t;
    }
}
