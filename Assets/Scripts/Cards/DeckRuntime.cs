using System.Collections.Generic;
using UnityEngine;

public class DeckRuntime
{
    public readonly List<CardInstance> drawPile = new();
    public readonly List<CardInstance> discardPile = new();
    public readonly List<CardInstance> hand = new();

    System.Random rng = new System.Random();

    public int HandSize => hand.Count;

    public void InitWith(List<CardDef> defs)
    {
        drawPile.Clear(); discardPile.Clear(); hand.Clear();

        foreach (var d in defs)
            drawPile.Add(new CardInstance(d));

        Shuffle(drawPile);
    }

    public void Draw(int n)
    {
        for (int i = 0; i < n; i++)
        {
            if (drawPile.Count == 0)
                RefillFromDiscard();

            if (drawPile.Count == 0) return;

            var c = drawPile[0];
            drawPile.RemoveAt(0);
            hand.Add(c);
        }
    }

    public void DiscardFromHand(CardInstance c)
    {
        if (c == null) return;
        if (hand.Remove(c))
            discardPile.Add(c);
    }

    public void Shuffle(List<CardInstance> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    void RefillFromDiscard()
    {
        if (discardPile.Count == 0) return;
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        Shuffle(drawPile);
    }
}
