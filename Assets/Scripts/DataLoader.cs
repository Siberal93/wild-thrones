using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class DataLoader : MonoBehaviour
{
    void Start()
    {
        string basePath = Path.Combine(Application.streamingAssetsPath, "data");
        string manifestPath = Path.Combine(basePath, "manifest.json");

        Debug.Log($"[DataLoader] StreamingAssets path: {Application.streamingAssetsPath}");
        Debug.Log($"[DataLoader] Loading manifest: {manifestPath}");

        string manifestText = File.ReadAllText(manifestPath);
        JObject manifest = JObject.Parse(manifestText);

        string unitsFile = manifest["files"]?["units"]?.ToString();
        string cardsFile = manifest["files"]?["cards"]?.ToString();
        string encountersFile = manifest["files"]?["encounters_act1"]?.ToString();

        JObject units = JObject.Parse(File.ReadAllText(Path.Combine(basePath, unitsFile)));
        JObject cards = JObject.Parse(File.ReadAllText(Path.Combine(basePath, cardsFile)));
        JObject encounters = JObject.Parse(File.ReadAllText(Path.Combine(basePath, encountersFile)));

        var unitsArr = units["units"] as JArray;
        var cardsArr = cards["cards"] as JArray;
        var encArr   = encounters["encounters"] as JArray;

        Debug.Log($"[DataLoader] Units: {unitsArr?.Count ?? 0}");
        Debug.Log($"[DataLoader] Cards: {cardsArr?.Count ?? 0}");
        Debug.Log($"[DataLoader] Encounters: {encArr?.Count ?? 0}");
    }
}
