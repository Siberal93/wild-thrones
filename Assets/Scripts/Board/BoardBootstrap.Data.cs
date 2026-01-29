using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;

public partial class BoardBootstrap
{
    void LoadData()
    {
        string basePath = Path.Combine(Application.streamingAssetsPath, "data");
        string manifestPath = Path.Combine(basePath, "manifest.json");

        manifest = JObject.Parse(File.ReadAllText(manifestPath));

        string unitsFile = manifest["files"]?["units"]?.ToString();
        string encountersFile = manifest["files"]?["encounters_act1"]?.ToString();

        unitsJson = JObject.Parse(File.ReadAllText(Path.Combine(basePath, unitsFile)));
        encountersJson = JObject.Parse(File.ReadAllText(Path.Combine(basePath, encountersFile)));
    }

    void SpawnWave(string encounterId, int waveIndex)
    {
        var encArr = encountersJson["encounters"] as JArray;
        var encounter = FindById(encArr, encounterId);
        if (encounter == null)
        {
            Debug.LogError($"[BoardBootstrap] Encounter not found: {encounterId}");
            return;
        }

        var waves = encounter["waves"] as JArray;
        var wave = waves?[waveIndex] as JObject;
        var spawns = wave?["spawn"] as JArray;

        if (spawns == null)
        {
            Debug.LogError("[BoardBootstrap] Wave spawn list missing.");
            return;
        }

        int cursor = 0;

        foreach (var unitIdTok in spawns)
        {
            string unitId = unitIdTok.ToString();
            var unit = FindUnit(unitId);

            if (unit == null)
            {
                Debug.LogError($"[BoardBootstrap] Unit not found: {unitId}");
                continue;
            }

            // 0..2 => EN_FRONT, 3..5 => EN_BACK
            bool front = cursor < 3;
            int col = front ? 2 : 3;      // 2=EN_FRONT, 3=EN_BACK
            int lane = cursor % 3;        // 0=LEFT, 1=CENTER, 2=RIGHT
            cursor++;

            string slotKey = SlotKey12(lane, col);

            if (!slotMap.TryGetValue(slotKey, out var slotTr))
            {
                Debug.LogError($"[BoardBootstrap] Slot not found in slotMap: {slotKey}");
                continue;
            }

            Vector2 pos = slotTr.position;
            CreateUnitToken(unit, pos, isEnemy: true, slotKey: slotKey);
        }
    }

    JObject FindUnit(string unitId)
    {
        var arr = unitsJson["units"] as JArray;
        return FindById(arr, unitId);
    }

    JObject FindById(JArray arr, string id)
    {
        if (arr == null) return null;

        foreach (var item in arr)
        {
            if (item is JObject obj && obj["id"]?.ToString() == id)
                return obj;
        }

        return null;
    }
}
