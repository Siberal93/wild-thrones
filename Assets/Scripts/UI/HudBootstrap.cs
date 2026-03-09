using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class HudBootstrap : MonoBehaviour
{
    [Header("HUD Layout")]
    [SerializeField] float hudHeight = 220f;
    float pileWidth = 180f; // <-- qui giochi con la larghezza MAZZO/SCARTI

    public DeckRuntime deck; // assegnato da BoardBootstrap o creato qui

    HandBarView handView;
    PileWidgetView deckWidget;
    PileWidgetView discardWidget;
    PileViewerView viewer;

    void Awake()
    {
        EnsureDeck();
        BuildHud();
        RefreshAll();

        // start: pesca 5
        deck.Draw(10);
        RefreshAll();
    }

    void EnsureDeck()
    {
        if (deck != null) return;

        // Stub deck (poi lo sostituiamo con cards.json)
        var defs = new List<CardDef>()
        {
            new CardDef{ id="c1", name="Pistola", desc="Danno a un nemico", cost=1, effect="DMG_ENEMY", amount=2 },
            new CardDef{ id="c2", name="Bende", desc="Cura un alleato", cost=1, effect="HEAL_ALLY", amount=2 },
            new CardDef{ id="c3", name="Ordine", desc="Riduci CD", cost=1, effect="REDUCE_CD", amount=1 },
            new CardDef{ id="c4", name="Fumo", desc="Debuff (placeholder)", cost=0, effect="NONE", amount=0 },
            new CardDef{ id="c5", name="Colpo", desc="Danno a un nemico", cost=1, effect="DMG_ENEMY", amount=3 },
            new CardDef{ id="c6", name="Tattica", desc="Riduci CD", cost=2, effect="REDUCE_CD", amount=2 },
            new CardDef{ id="c1", name="Pistola", desc="Danno a un nemico", cost=1, effect="DMG_ENEMY", amount=2 },
            new CardDef{ id="c2", name="Bende", desc="Cura un alleato", cost=1, effect="HEAL_ALLY", amount=2 },
            new CardDef{ id="c3", name="Ordine", desc="Riduci CD", cost=1, effect="REDUCE_CD", amount=1 },
            new CardDef{ id="c4", name="Fumo", desc="Debuff (placeholder)", cost=0, effect="NONE", amount=0 },
            new CardDef{ id="c5", name="Colpo", desc="Danno a un nemico", cost=1, effect="DMG_ENEMY", amount=3 },
            new CardDef{ id="c6", name="Tattica", desc="Riduci CD", cost=2, effect="REDUCE_CD", amount=2 },
        };

        deck = new DeckRuntime();
        deck.InitWith(defs);
    }

    void BuildHud()
    {
        // Root full-screen
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

        // --- Inner container centrato (80%) ---
        var innerGo = new GameObject("BottomBarInner", typeof(RectTransform));
        innerGo.transform.SetParent(barGo.transform, false);

        var innerRt = innerGo.GetComponent<RectTransform>();
        innerRt.anchorMin = new Vector2(0.10f, 0f); // 10% da sinistra
        innerRt.anchorMax = new Vector2(0.90f, 1f); // 90% -> quindi 80% totale
        innerRt.pivot = new Vector2(0.5f, 0.5f);
        innerRt.offsetMin = Vector2.zero;
        innerRt.offsetMax = Vector2.zero;

        var innerLayout = innerGo.AddComponent<HorizontalLayoutGroup>();
        innerLayout.padding = new RectOffset(0, 0, 14, 14);
        innerLayout.spacing = 18;
        innerLayout.childAlignment = TextAnchor.MiddleCenter;

        innerLayout.childControlWidth = true;
        innerLayout.childControlHeight = true;

        // ✅ QUI: NON espandere tutti i figli, altrimenti deck/scarti ignorano la width fissa
        innerLayout.childForceExpandWidth = false;
        innerLayout.childForceExpandHeight = true;

        var deckCol = CreateHudColumn(innerGo.transform, "DeckCol", fixedWidth: pileWidth); // larghezza fissa
        var handCol = CreateHudColumn(innerGo.transform, "HandCol", fixedWidth: 0f); // flex
        var discardCol = CreateHudColumn(innerGo.transform, "DiscardCol", fixedWidth: pileWidth); // larghezza fissa

        deckWidget = PileWidgetView.Create(deckCol, "MAZZO", onClick: () =>
        {
            viewer.Show("MAZZO", deck.drawPile.Select(x => x).ToList(), sort: false);
        });

        handView = HandBarView.Create(handCol, onCardClicked: OnHandCardClicked);

        // Discard widget (right)
        discardWidget = PileWidgetView.Create(discardCol, "SCARTI", onClick: () =>
        {
            viewer.Show("SCARTI", deck.discardPile.Select(x => x).ToList(), sort: true);
        });

        // Viewer popup (overlay)
        viewer = PileViewerView.Create(transform, onClose: () => viewer.Hide());
        viewer.Hide();

    }

    void OnHandCardClicked(CardInstance c)
    {
        if (c == null) return;

        Debug.Log($"[PLAY] {c}");

        // TEST: per ora “gioca e scarta”
        ApplyEffect(c);
        deck.DiscardFromHand(c);

        // pesca 1 per rimpiazzare
        deck.Draw(1);

        RefreshAll();
    }

    void ApplyEffect(CardInstance c)
    {
        // Collegamento a BoardBootstrap/CombatController lo facciamo nello step 5.
        // Per ora log.
        Debug.Log($"[EFFECT] {c.def.effect} amount={c.def.amount}");
    }

    void RefreshAll()
    {
        deckWidget.SetCount(deck.drawPile.Count);
        discardWidget.SetCount(deck.discardPile.Count);
        handView.SetHand(deck.hand);
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
            // ✅ QUI è la WIDTH FISSA
            le.minWidth = fixedWidth;
            le.preferredWidth = fixedWidth;
            le.flexibleWidth = 0f;
        }
        else
        {
            // ✅ QUI è la COLONNA FLEX (mano)
            le.minWidth = 0f;
            le.preferredWidth = 0f;
            le.flexibleWidth = 1f;
        }

        return go.transform;
    }

}
