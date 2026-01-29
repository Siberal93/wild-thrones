using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TMPro;

public partial class BoardBootstrap : MonoBehaviour
{
    [Header("Visual")]
    public float cellSize = 1.8f;
    public float rowGap = 1.2f;
    public float colGap = 1.2f;

    // Data JSON
    private JObject manifest, unitsJson, encountersJson;

    // Board
    private readonly Dictionary<string, Transform> slotMap = new Dictionary<string, Transform>();
    private readonly Dictionary<string, UnitRuntime> occupancy = new Dictionary<string, UnitRuntime>();
    private readonly Dictionary<string, LineRenderer> slotOutline = new Dictionary<string, LineRenderer>();

    // Highlight
    private string highlightedSlot = null;

    public static BoardBootstrap Instance;

    void Awake() => Instance = this;

    void Start()
    {
        LoadData();

        // 1) Disegna griglia 3x4 (AL_BACK, AL_FRONT, EN_FRONT, EN_BACK) x (L/C/R)
        DrawGrid12(Vector2.zero);

        // 2) Spawn wave 1 dell'encounter di test
        SpawnWave("enc_forest_boss_tasso", 0);
    }
}
