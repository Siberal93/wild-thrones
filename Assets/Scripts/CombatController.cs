using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CombatController : MonoBehaviour
{
    // ordine slot fisso: FRONT_L, BACK_L, FRONT_C, BACK_C, FRONT_R, BACK_R


    private static readonly string[] SlotOrder = new[]
    {
        "FRONT_LEFT","BACK_LEFT","FRONT_CENTER","BACK_CENTER","FRONT_RIGHT","BACK_RIGHT"
    };

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            PlayCommand(beats: 1);
        }
        if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            PlayCommand(beats: 1);
        }

    }


    void PlayCommand(int beats)
    {
        var units = UnityEngine.Object.FindObjectsByType<UnitRuntime>(FindObjectsSortMode.None).ToList();

        units = units
            .Where(u => !string.IsNullOrEmpty(u.unitId) && u.hp > 0 && !string.IsNullOrEmpty(u.slotKey))
            .ToList();

        // 1) tick countdown
        Debug.Log($"--- COMMAND played (beats={beats}) ---");

        Debug.Log($"--- COMMAND played (beats={beats}) ---");

        foreach (var u in units)
        {
            u.Tick(beats);
            u.RefreshLabel();

            Debug.Log($"CD {u.displayName}#{u.instanceId} ({u.slotAbbrev}) = {u.cd}");
        }

        Debug.Log($"--- END COMMAND ---");


        // 2) risolvi chi è a cd <= 0 nell'ordine richiesto
        ResolveActions(units);

        Debug.Log($"--- END COMMAND ---");
    }

    void ResolveActions(List<UnitRuntime> units)
    {
        // filtra pronti
        var ready = units.Where(u => u.cd <= 0).ToList();
        if (ready.Count == 0) return;

        // grouping in 4 blocchi: rapidi EN, rapidi AL, non-rapidi EN, non-rapidi AL
        var ordered = new List<UnitRuntime>();

        ordered.AddRange(OrderGroup(ready.Where(u => u.isEnemy && u.HasTrait("RAPIDO")).ToList()));
        ordered.AddRange(OrderGroup(ready.Where(u => !u.isEnemy && u.HasTrait("RAPIDO")).ToList()));
        ordered.AddRange(OrderGroup(ready.Where(u => u.isEnemy && !u.HasTrait("RAPIDO")).ToList()));
        ordered.AddRange(OrderGroup(ready.Where(u => !u.isEnemy && !u.HasTrait("RAPIDO")).ToList()));

        foreach (var u in ordered)
        {
            // per ora: "agire" = log e reset CD
            Debug.Log(u);
            Debug.Log($"[ACT] {u.Side} {(u.IsFast ? "[RAPIDO]" : "")} {u.displayName} @ {u.slotKey}");
            u.ResetCD();
        }
    }

    static readonly string[] OrderKeys = new[]
    {
        "FRONT_LEFT",
        "BACK_LEFT",
        "FRONT_CENTER",
        "BACK_CENTER",
        "FRONT_RIGHT",
        "BACK_RIGHT"
    };

    List<UnitRuntime> OrderGroup(List<UnitRuntime> group)
    {
        return group.OrderBy(u => SlotRank(u.slotKey)).ToList();
    }

    int SlotRank(string slotKey)
    {
        if (string.IsNullOrEmpty(slotKey)) return 999;

        // EN_FRONT_RIGHT -> parts = [EN, FRONT, RIGHT]
        var parts = slotKey.Split('_');
        if (parts.Length < 3) return 999;

        string pos = parts[1] + "_" + parts[2]; // FRONT_RIGHT
        int idx = System.Array.IndexOf(OrderKeys, pos);
        return idx >= 0 ? idx : 999;
    }

    string AbbrevSlot(string slotKey)
    {
        // slotKey tipo: "AL_BACK_LEFT"
        var parts = slotKey.Split('_');
        if (parts.Length < 3) return slotKey;

        bool enemy = parts[0] == "EN";
        bool front = parts[1] == "FRONT";

        char side = enemy ? 'N' : 'G';   // Nemico/Giocatore
        char line = front ? 'F' : 'R';   // Front/Retro

        char lane = parts[2] switch
        {
            "LEFT" => 'S',
            "CENTER" => 'C',
            "RIGHT" => 'D',
            _ => '?'
        };

        return $"{side}{line}{lane}";
    }
}
