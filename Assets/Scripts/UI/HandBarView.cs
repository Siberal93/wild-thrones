using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class HandBarView : MonoBehaviour
{
    const int MaxCardsPerRow = 6;
    const int MaxHandCards = 12;

    Action<CardInstance> onCardClicked;
    readonly List<CardMiniView> views = new();

    RectTransform rowTop;
    RectTransform rowBottom;

    public static HandBarView Create(Transform parent, Action<CardInstance> onCardClicked)
    {
        var go = new GameObject("HandBar", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // layout verticale: riga sopra + riga sotto
        var vlg = go.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.padding = new RectOffset(0, 0, 0, 0);
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = true;

        var v = go.AddComponent<HandBarView>();
        v.onCardClicked = onCardClicked;

        v.rowTop = v.CreateRow(go.transform, "RowTop");
        v.rowBottom = v.CreateRow(go.transform, "RowBottom");

        return v;
    }

    RectTransform CreateRow(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
        le.flexibleHeight = 1f;
        le.minHeight = 0f;

        var hlg = go.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.padding = new RectOffset(0, 0, 0, 0);
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        return rt;
    }

    public void SetFlexible()
    {
        // non serve più fare nulla qui, la colonna è già flex
    }

    public void SetHand(List<CardInstance> hand)
    {
        foreach (var c in views)
            if (c != null) Destroy(c.gameObject);
        views.Clear();

        if (hand == null) return;

        int count = Mathf.Min(hand.Count, MaxHandCards);

        for (int i = 0; i < count; i++)
        {
            var parent = (i < MaxCardsPerRow) ? rowTop : rowBottom;
            var mv = CardMiniView.Create(parent, hand[i], onCardClicked);
            views.Add(mv);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(rowTop);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rowBottom);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }
}