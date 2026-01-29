using UnityEngine;

public partial class BoardBootstrap
{
    public void RegisterUnit(UnitRuntime u, string slotKey)
    {
        // remove-safe: non modificare Dictionary durante foreach
        string foundKey = null;
        foreach (var kv in occupancy)
        {
            if (kv.Value == u) { foundKey = kv.Key; break; }
        }
        if (foundKey != null) occupancy.Remove(foundKey);

        occupancy[slotKey] = u;

        u.slotKey = slotKey;
        u.slotAbbrev = AbbrevSlot(slotKey);
        u.RefreshLabel();

        if (slotMap.TryGetValue(slotKey, out var t))
            u.transform.position = new Vector3(t.position.x, t.position.y, -1f);
    }

    public UnitRuntime GetUnitAt(string slotKey)
    {
        return occupancy.TryGetValue(slotKey, out var u) ? u : null;
    }

    public string FindNearestSlotKey(Vector2 worldPos, float maxDist = 0.9f)
    {
        string best = null;
        float bestD = float.MaxValue;

        foreach (var kv in slotMap)
        {
            var p = (Vector2)kv.Value.position;
            float d = Vector2.Distance(worldPos, p);
            if (d < bestD)
            {
                bestD = d;
                best = kv.Key;
            }
        }

        return (bestD <= maxDist) ? best : null;
    }

    Color SlotDefaultColor(string slotKey)
    {
        bool isEnemy = slotKey.StartsWith("EN_");
        return isEnemy ? new Color(1f, 0.4f, 0.4f, 1f) : new Color(0.4f, 1f, 0.4f, 1f);
    }

    public void HighlightSlot(string slotKey)
    {
        if (highlightedSlot == slotKey) return;

        if (highlightedSlot != null && slotOutline.TryGetValue(highlightedSlot, out var oldLr))
        {
            var c0 = SlotDefaultColor(highlightedSlot);
            oldLr.startColor = c0;
            oldLr.endColor = c0;
        }

        highlightedSlot = slotKey;

        if (slotKey != null && slotOutline.TryGetValue(slotKey, out var lr))
        {
            var c = new Color(1f, 0.9f, 0.2f, 1f);
            lr.startColor = c;
            lr.endColor = c;
        }
    }

    public void ClearHighlightSlot() => HighlightSlot(null);

    public void MoveOrSwap(UnitRuntime dragged, string targetSlotKey)
    {
        if (dragged == null || string.IsNullOrEmpty(targetSlotKey)) return;

        string from = dragged.slotKey;
        if (from == targetSlotKey)
        {
            RegisterUnit(dragged, from);
            return;
        }

        var other = GetUnitAt(targetSlotKey);

        if (other == null)
        {
            occupancy.Remove(from);
            RegisterUnit(dragged, targetSlotKey);
        }
        else
        {
            // swap
            occupancy[from] = other;
            RegisterUnit(other, from);

            occupancy[targetSlotKey] = dragged;
            RegisterUnit(dragged, targetSlotKey);
        }
    }
}
