using UnityEngine;

public partial class BoardBootstrap
{
    Vector2 SlotToWorld12(Vector2 anchor, int row, int col)
    {
        // col 0..3 da sinistra a destra
        float x = anchor.x + (col - 1.5f) * (cellSize + colGap);

        // row 0..2 dall'alto verso il basso
        float y = anchor.y + (1 - row) * (cellSize + rowGap);

        return new Vector2(x, y);
    }

    string SlotKey12(int row, int col)
    {
        // col: 0=AL_BACK, 1=AL_FRONT, 2=EN_FRONT, 3=EN_BACK
        string lane = col switch
        {
            0 => "BACK",
            1 => "FRONT",
            2 => "FRONT",
            3 => "BACK",
            _ => "UNK"
        };

        string side = (col >= 2) ? "EN" : "AL";

        // row: 0=LEFT,1=CENTER,2=RIGHT (come stai usando tu)
        string line = row switch
        {
            0 => "LEFT",
            1 => "CENTER",
            2 => "RIGHT",
            _ => "UNK"
        };

        return $"{side}_{lane}_{line}";
    }

    string AbbrevSlot(string slotKey)
    {
        // slotKey: EN_FRONT_RIGHT -> NFD ecc.
        var p = slotKey.Split('_'); // EN, FRONT, RIGHT
        if (p.Length < 3) return slotKey;

        string side = (p[0] == "AL") ? "G" : "N";
        string lane = (p[1] == "FRONT") ? "F" : "R";
        string col = p[2] == "LEFT" ? "S" : (p[2] == "CENTER" ? "C" : "D");

        return $"{side}{lane}{col}";
    }

    void DrawGrid12(Vector2 anchor)
    {
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

                // (importante per visibilità) sorting del LineRenderer dello slot
                lr.sortingOrder = 0;

                slotOutline[slotKey] = lr;

                if (isEnemy)
                {
                    // fill dentro slot nemici (opaco)
                    CreateSlotFill(go.transform, cellSize * 0.98f, new Color(1f, 0.2f, 0.2f, 1f));
                }

                // label slot
                CreateLabelTMP(AbbrevSlot(slotKey), go.transform, Vector2.zero, 2f);
            }
        }
    }
}
