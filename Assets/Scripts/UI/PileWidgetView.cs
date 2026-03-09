using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PileWidgetView : MonoBehaviour
{
    TMP_Text title;
    TMP_Text count;
    Action onClick;

    public static PileWidgetView Create(Transform parent, string label, Action onClick)
    {
        var go = new GameObject(label, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.10f, 0.12f, 0.16f, 0.75f);

        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(() => onClick?.Invoke());

        var v = go.AddComponent<PileWidgetView>();
        v.onClick = onClick;

        v.title = CreateTMP(go.transform, "Title", 18, FontStyles.Bold);
        v.title.text = label;

        v.count = CreateTMP(go.transform, "Count", 28, FontStyles.Bold);
        v.count.alignment = TextAlignmentOptions.BottomRight;
        v.count.rectTransform.anchoredPosition = new Vector2(-10, 8);

        v.title.alignment = TextAlignmentOptions.TopLeft;
        v.title.rectTransform.anchoredPosition = new Vector2(10, -8);

        return v;
    }

    public void SetWidth(float w)
    {
        var rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(w, rt.sizeDelta.y);
    }

    public void SetCount(int n)
    {
        count.text = (n <= 0) ? "" : n.ToString();
    }

    public void SetCountText(string text)
    {
        count.text = text;
    }

    public void SetTitle(string text)
    {
        title.text = text;
    }

    static TMP_Text CreateTMP(Transform parent, string name, float size, FontStyles style)
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