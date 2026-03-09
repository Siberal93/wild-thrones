using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CardMiniView : MonoBehaviour
{
    CardInstance card;
    Action<CardInstance> onClick;

    public static CardMiniView Create(Transform parent, CardInstance card, Action<CardInstance> onClick)
    {
        var go = new GameObject($"Card_{card.def.id}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(110, 88);

        var le = go.AddComponent<LayoutElement>();
        le.minWidth = 110;
        le.preferredWidth = 110;
        le.minHeight = 88;
        le.preferredHeight = 88;
        le.flexibleWidth = 0;
        le.flexibleHeight = 0;

        var bg = go.GetComponent<Image>();
        bg.color = new Color(0.12f, 0.15f, 0.20f, 1f);

        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(() => onClick?.Invoke(card));

        // titolo
        var title = CreateTMP(go.transform, "Title", 16, FontStyles.Bold);
        title.text = card.def.name;
        title.alignment = TextAlignmentOptions.TopLeft;
        title.rectTransform.anchorMin = new Vector2(0, 1);
        title.rectTransform.anchorMax = new Vector2(1, 1);
        title.rectTransform.pivot = new Vector2(0.5f, 1);
        title.rectTransform.sizeDelta = new Vector2(0, 24);
        title.rectTransform.anchoredPosition = new Vector2(0, -4);

        // costo
        var cost = CreateTMP(go.transform, "Cost", 16, FontStyles.Bold);
        cost.text = card.def.cost.ToString();
        cost.alignment = TextAlignmentOptions.TopRight;
        cost.rectTransform.anchorMin = new Vector2(0, 1);
        cost.rectTransform.anchorMax = new Vector2(1, 1);
        cost.rectTransform.pivot = new Vector2(0.5f, 1);
        cost.rectTransform.sizeDelta = new Vector2(0, 24);
        cost.rectTransform.anchoredPosition = new Vector2(0, -4);

        // descrizione mini
        var desc = CreateTMP(go.transform, "Desc", 10, FontStyles.Normal);
        desc.text = card.def.desc;
        desc.alignment = TextAlignmentOptions.BottomLeft;
        desc.textWrappingMode = TextWrappingModes.Normal;
        desc.rectTransform.anchorMin = new Vector2(0, 0);
        desc.rectTransform.anchorMax = new Vector2(1, 1);
        desc.rectTransform.offsetMin = new Vector2(6, 6);
        desc.rectTransform.offsetMax = new Vector2(-6, -28);

        var v = go.AddComponent<CardMiniView>();
        v.card = card;
        v.onClick = onClick;
        return v;
    }

    static TMP_Text CreateTMP(Transform parent, string name, float size, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize = size;
        t.fontStyle = style;
        t.color = Color.white;

        return t;
    }
}