using TMPro;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class UnitRuntime : MonoBehaviour
{
    public string unitId;
    public string displayName;
    public bool isEnemy;
    public string slotKey;

    public int hp;
    public int cdBase;
    public int cd;

    public HashSet<string> traits = new HashSet<string>();

    public string Side => isEnemy ? "EN" : "AL";
    public bool IsFast => traits != null && traits.Contains("RAPIDO");

    private static int _seq = 0;
    public int instanceId;
    private LineRenderer lr;
    private Color baseColor;

    // per evitare Find ogni volta
    [HideInInspector] public TextMeshPro labelTmp;

    // lo settiamo a spawn (così CombatController non deve conoscere AbbrevSlot)
    [HideInInspector] public string slotAbbrev;

    public void InitFromJson(JObject unit, bool enemySide)
    {
        isEnemy = enemySide;
        unitId = unit["id"]?.ToString();
        displayName = unit["name"]?.ToString() ?? unitId;

        hp = unit["stats"]?["hp"]?.Value<int>() ?? 0;
        cdBase = unit["stats"]?["cdBase"]?.Value<int>() ?? 0;
        cd = cdBase;

        instanceId = ++_seq;

        var arr = unit["traits"] as JArray;
        traits.Clear();
        if (arr != null)
        {
            foreach (var t in arr) traits.Add(t.ToString());
        }
    }

    public bool HasTrait(string t) => traits.Contains(t);

    public void Tick(int beats) => cd -= beats;
    public void ResetCD() => cd = cdBase;

    // 1 riga: sigla 6 char + numero, 2 riga: pos, 3 riga: CD
    public void RefreshLabel()
    {
        if (labelTmp == null) labelTmp = GetComponentInChildren<TextMeshPro>();
        if (labelTmp == null) return;

        string code = MakeCode6(displayName);
        labelTmp.text = $"{code}{instanceId}\n{slotAbbrev}\nCD:{cd}";
    }

    // "Cucciolo Lupo" -> "CucLup"
    // fallback: primi 6 caratteri
    private static string MakeCode6(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Unit00";

        var parts = name.Trim().Split(' ');
        if (parts.Length >= 2)
        {
            string a = Take(parts[0], 3);
            string b = Take(parts[1], 3);
            return a + b;
        }
        return Take(name.Replace(" ", ""), 6);
    }

    private static string Take(string s, int n)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return (s.Length <= n) ? s : s.Substring(0, n);
    }

    public void OnTap()
    {
        Debug.Log($"[TAP] {displayName}#{instanceId} slot={slotKey}");
    }

    public void OnLongPress()
    {
        Debug.Log($"[LONG] {displayName}#{instanceId} slot={slotKey}");
    }


    void Start()
    {
        lr = GetComponent<LineRenderer>();
        if (lr != null) baseColor = lr.startColor;
    }

    public void SetHover(bool on)
    {
        if (lr == null) return;

        if (on)
        {
            var c = new Color(1f, 0.9f, 0.2f, 1f);
            lr.startColor = c; lr.endColor = c;
        }
        else
        {
            lr.startColor = baseColor; lr.endColor = baseColor;
        }
    }

    void EnsureCollider(float radius = 1f)
    {
        var cc = GetComponent<CircleCollider2D>();
        if (cc == null) cc = gameObject.AddComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = radius;
    }

}

