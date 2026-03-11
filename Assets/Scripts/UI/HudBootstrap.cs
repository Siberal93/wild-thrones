using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System;

public class HudBootstrap : MonoBehaviour
{
    [Header("HUD Layout")]
    [SerializeField] float hudHeight = 220f;

    const int StartingHandSize = 7;
    const int RefreshBaseCd = 4;

    float pileWidth = 180f;
    float refreshWidth = 140f;

    public DeckRuntime deck;

    public Action<int> onBeatSpent; // hook futuro verso CombatController / tempo

    HandBarView handView;
    PileWidgetView deckWidget;
    PileWidgetView discardWidget;
    PileWidgetView refreshWidget;
    PileViewerView viewer;

    int refreshCd = RefreshBaseCd;

    void Awake()
    {
        EnsureDeck();
        BuildHud();

        deck.Draw(StartingHandSize);
        RefreshAll();
    }

    void EnsureDeck()
    {
        if (deck != null) return;

        var defs = new List<CardDef>()
        {
            new CardDef{ id="cm1", code="pis01", name="Pistola", desc="Danno a un nemico", cost=1, effect="DMG_ENEMY", amount=2 },
            new CardDef{ id="cm2", code="ben01", name="Bende",   desc="Cura un alleato",   cost=1, effect="HEAL_ALLY", amount=2 },
            new CardDef{ id="cm3", code="ord01", name="Ordine",  desc="Riduci CD",         cost=1, effect="REDUCE_CD", amount=1 },
            new CardDef{ id="cm4", code="fum01", name="Fumo",    desc="Debuff",            cost=0, effect="NONE", amount=0 },
            new CardDef{ id="cm5", code="col01", name="Colpo",   desc="Danno a un nemico", cost=1, effect="DMG_ENEMY", amount=3 },
            new CardDef{ id="cm6", code="tat01", name="Tattica", desc="Riduci CD",         cost=1, effect="REDUCE_CD", amount=2 },
            new CardDef{ id="cm7", code="pis01", name="Pistola", desc="Danno a un nemico", cost=1, effect="DMG_ENEMY", amount=2 },
            new CardDef{ id="cm8", code="ben01", name="Bende",   desc="Cura un alleato",   cost=1, effect="HEAL_ALLY", amount=2 },
            new CardDef{ id="cm9", code="ord01", name="Ordine",  desc="Riduci CD",         cost=1, effect="REDUCE_CD", amount=1 },
            new CardDef{ id="cm10", code="fum01", name="Fumo",    desc="Debuff",            cost=0, effect="NONE", amount=0 },
            new CardDef{ id="cm11", code="col01", name="Colpo",   desc="Danno a un nemico", cost=1, effect="DMG_ENEMY", amount=3 },
            new CardDef{ id="cm12", code="tat01", name="Tattica", desc="Riduci CD",         cost=1, effect="REDUCE_CD", amount=2 },
            new CardDef{ id="cm13", code="pis01", name="Pistola", desc="Danno a un nemico", cost=1, effect="DMG_ENEMY", amount=2 },
            new CardDef{ id="cm14", code="pis01", name="Pistola", desc="Danno a un nemico", cost=1, effect="DMG_ENEMY", amount=2 },
        };

        deck = new DeckRuntime();
        deck.InitWith(defs);
    }

    void BuildHud()
    {
        var root = GetComponent<RectTransform>();
        if (root == null) root = gameObject.AddComponent<RectTransform>();
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;

        // Bottom bar
        var barGo = new GameObject("BottomBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        barGo.transform.SetParent(transform, false);

        var barRt = barGo.GetComponent<RectTransform>();
        barRt.anchorMin = new Vector2(0, 0);
        barRt.anchorMax = new Vector2(1, 0);
        barRt.pivot = new Vector2(0.5f, 0);
        barRt.sizeDelta = new Vector2(0, hudHeight);
        barRt.anchoredPosition = Vector2.zero;

        var barImg = barGo.GetComponent<Image>();
        barImg.color = new Color(0.05f, 0.07f, 0.10f, 0.92f);

        // Inner 80%
        var innerGo = new GameObject("BottomBarInner", typeof(RectTransform));
        innerGo.transform.SetParent(barGo.transform, false);

        var innerRt = innerGo.GetComponent<RectTransform>();
        innerRt.anchorMin = new Vector2(0.10f, 0f);
        innerRt.anchorMax = new Vector2(0.90f, 1f);
        innerRt.pivot = new Vector2(0.5f, 0.5f);
        innerRt.offsetMin = Vector2.zero;
        innerRt.offsetMax = Vector2.zero;

        var innerLayout = innerGo.AddComponent<HorizontalLayoutGroup>();
        innerLayout.padding = new RectOffset(0, 0, 14, 14);
        innerLayout.spacing = 18;
        innerLayout.childAlignment = TextAnchor.MiddleCenter;
        innerLayout.childControlWidth = true;
        innerLayout.childControlHeight = true;
        innerLayout.childForceExpandWidth = false;
        innerLayout.childForceExpandHeight = true;

        // 4 colonne: deck - hand - discard - refresh
        var deckCol = CreateHudColumn(innerGo.transform, "DeckCol", pileWidth);
        var handCol = CreateHudColumn(innerGo.transform, "HandCol", 0f);
        var discardCol = CreateHudColumn(innerGo.transform, "DiscardCol", pileWidth);
        var refreshCol = CreateHudColumn(innerGo.transform, "RefreshCol", refreshWidth);

        deckWidget = PileWidgetView.Create(deckCol, "MAZZO", onClick: () =>
        {
            viewer.Show("MAZZO", deck.drawPile.Select(x => x).ToList(), sort: false);
        });

        handView = HandBarView.Create(handCol, onCardClicked: OnHandCardClicked);

        discardWidget = PileWidgetView.Create(discardCol, "SCARTI", onClick: () =>
        {
            viewer.Show("SCARTI", deck.discardPile.Select(x => x).ToList(), sort: true);
        });

        refreshWidget = PileWidgetView.Create(refreshCol, "RINFRESCA", onClick: OnRefreshClicked);

        viewer = PileViewerView.Create(transform, onClose: () => viewer.Hide());
        viewer.Hide();

        UpdateRefreshWidget();
    }

    void OnHandCardClicked(CardInstance c)
    {
        if (c == null) return;

        Debug.Log($"[PLAY] {c}");

        ApplyEffect(c);
        deck.DiscardFromHand(c);

        deck.Draw(1);
        RefreshAll();
    }

    void OnRefreshClicked()
    {
        bool paidBeat = refreshCd > 0;

        if (paidBeat)
        {
            AdvanceBeats(1);
            Debug.Log("[REFRESH] premuto prima di CD 0 -> +1 beat");
        }
        else
        {
            Debug.Log("[REFRESH] premuto a CD 0 -> refresh gratuito");
        }

        // scarta tutta la mano e pesca 7
        deck.discardPile.AddRange(deck.hand);
        deck.hand.Clear();
        deck.Draw(StartingHandSize);

        // reset del bottone
        refreshCd = RefreshBaseCd;

        RefreshAll();
    }

    public void AdvanceBeats(int beats)
    {
        if (beats <= 0) return;

        refreshCd = Mathf.Max(0, refreshCd - beats);
        UpdateRefreshWidget();

        onBeatSpent?.Invoke(beats);
    }

    void ApplyEffect(CardInstance c)
    {
        Debug.Log($"[EFFECT] {c.def.effect} amount={c.def.amount}");
    }

    void RefreshAll()
    {
        deckWidget.SetCount(deck.drawPile.Count);
        discardWidget.SetCount(deck.discardPile.Count);

        UpdateRefreshWidget();
        handView.SetHand(deck.hand);
    }

    void UpdateRefreshWidget()
    {
        if (refreshWidget == null) return;
        refreshWidget.SetCount(refreshCd); // già nasconde lo 0
    }

    Transform CreateHudColumn(Transform parent, string name, float fixedWidth)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;

        var le = go.AddComponent<LayoutElement>();
        le.flexibleHeight = 1f;

        if (fixedWidth > 0f)
        {
            le.minWidth = fixedWidth;
            le.preferredWidth = fixedWidth;
            le.flexibleWidth = 0f;
        }
        else
        {
            le.minWidth = 0f;
            le.preferredWidth = 0f;
            le.flexibleWidth = 1f;
        }

        return go.transform;
    }
}