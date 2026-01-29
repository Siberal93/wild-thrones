using UnityEngine;
using Newtonsoft.Json.Linq;

public partial class BoardBootstrap
{
    GameObject CreateUnitToken(JObject unit, Vector2 pos, bool isEnemy, string slotKey)
    {
        string name = unit["name"]?.ToString() ?? unit["id"]?.ToString();
        int hp = unit["stats"]?["hp"]?.Value<int>() ?? 0;
        int cd = unit["stats"]?["cdBase"]?.Value<int>() ?? 0;

        float r = cellSize * 0.45f;

        Color border = isEnemy
            ? new Color(0.75f, 0.12f, 0.12f, 1f)
            : new Color(0.12f, 0.75f, 0.12f, 1f);



        var go = CreateCircleOutline($"UNIT_{name}", pos, r, border, width: 0.08f, segments: 64);
        go.transform.SetParent(this.transform, true);

        // collider SEMPRE
        var cc = go.GetComponent<CircleCollider2D>();
        if (cc == null) cc = go.AddComponent<CircleCollider2D>();
        cc.radius = r;
        cc.isTrigger = true;

        var rt = go.AddComponent<UnitRuntime>();
        rt.InitFromJson(unit, isEnemy);
        rt.EnsureCollider(r);
        rt.slotKey = slotKey;
        rt.slotAbbrev = AbbrevSlot(slotKey);

        if (isEnemy)
        {
            CreateCircleFill(go.transform, r * 0.95f, new Color(0.55f, 0.05f, 0.05f, 0.5f));
        }
        else
        {
            CreateCircleFill(go.transform, r * 0.95f, new Color(0.05f, 0.35f, 0.05f, 0.35f));
        }

        rt.labelTmp = CreateLabelTMP("", go.transform, Vector2.zero, 2f);
        rt.RefreshLabel();

        RegisterUnit(rt, slotKey);

        Debug.Log($"[Spawn] {rt.displayName}#{rt.instanceId} HP:{hp} CD:{cd} SLOT:{slotKey}");
        return go;
    }
}
