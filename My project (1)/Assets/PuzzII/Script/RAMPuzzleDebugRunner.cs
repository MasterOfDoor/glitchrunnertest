using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Debug tool for the RAM Overload puzzle.
/// Attach to any GameObject in the puzzle scene.
/// Gives instant keyboard + on-screen controls to test everything.
///
/// KEYBOARD SHORTCUTS (Play Mode only):
///   F1  — Start / Restart puzzle
///   F2  — Force next wave
///   F3  — Spawn corrupted cell
///   F4  — Spawn overflow cell
///   F5  — Spawn chain group
///   F6  — Add +20 pressure
///   F7  — Remove -20 pressure
///   F8  — Lose 1 integrity
///   F9  — Toggle slow motion (0.25x) for inspecting state
///   F10 — Toggle debug overlay visibility
/// </summary>
public class RAMPuzzleDebugRunner : MonoBehaviour
{
    [Header("Required")]
    [SerializeField] private RAMPuzzleManager puzzleManager;
    [SerializeField] private PuzzleSessionManager sessionManager;

    [Header("Debug Overlay (auto-created if left null)")]
    [SerializeField] private GameObject debugOverlayRoot;

    // ── Private ──────────────────────────────────────────────────────────
    private bool   _overlayVisible  = true;
    private bool   _slowMo          = false;
    private float  _normalTimeScale = 1f;   // saved before timeScale=0 pause
    private bool   _puzzleOpen      = false;

    // Reflection helpers — lets us poke private fields for debugging
    private System.Reflection.FieldInfo _pressureField;
    private System.Reflection.FieldInfo _integrityField;
    private System.Reflection.FieldInfo _waveField;
    private System.Reflection.FieldInfo _scoreField;
    private System.Reflection.FieldInfo _isPlayingField;

    // On-screen labels
    private TextMeshProUGUI _lblPressure;
    private TextMeshProUGUI _lblIntegrity;
    private TextMeshProUGUI _lblWave;
    private TextMeshProUGUI _lblScore;
    private TextMeshProUGUI _lblStatus;
    private TextMeshProUGUI _lblSlowMo;

    // ─────────────────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        CacheReflectionFields();
        BuildDebugOverlay();

        // Auto-find managers if not assigned
        if (puzzleManager  == null) puzzleManager  = FindObjectOfType<RAMPuzzleManager>();
        if (sessionManager == null) sessionManager = FindObjectOfType<PuzzleSessionManager>();
    }

    private void Start()
    {
        Log("Debug Runner ready. Press F1 to start puzzle.");
    }

    private void Update()
    {
        HandleHotkeys();
        RefreshOverlay();
    }

    // ─────────────────────────────────────────────────────────────────────
    // HOTKEYS
    // ─────────────────────────────────────────────────────────────────────
    private void HandleHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.F1))  StartOrRestart();
        if (Input.GetKeyDown(KeyCode.F2))  ForceNextWave();
        if (Input.GetKeyDown(KeyCode.F3))  ForceSpawn("corrupted");
        if (Input.GetKeyDown(KeyCode.F4))  ForceSpawn("overflow");
        if (Input.GetKeyDown(KeyCode.F5))  ForceSpawn("chain");
        if (Input.GetKeyDown(KeyCode.F6))  AdjustPressure(+20f);
        if (Input.GetKeyDown(KeyCode.F7))  AdjustPressure(-20f);
        if (Input.GetKeyDown(KeyCode.F8))  LoseIntegrity();
        if (Input.GetKeyDown(KeyCode.F9))  ToggleSlowMo();
        if (Input.GetKeyDown(KeyCode.F10)) ToggleOverlay();
    }

    // ─────────────────────────────────────────────────────────────────────
    // DEBUG ACTIONS
    // ─────────────────────────────────────────────────────────────────────
    private void StartOrRestart()
    {
        if (sessionManager != null)
        {
            sessionManager.StartPuzzle();
            _puzzleOpen = true;
            Log("Puzzle started via SessionManager.");
        }
        else if (puzzleManager != null)
        {
            // Fallback: start manager directly without session pausing
            puzzleManager.BeginPuzzle();
            _puzzleOpen = true;
            Log("Puzzle started directly (no SessionManager found).");
        }
        else
        {
            Log("ERROR: No RAMPuzzleManager or PuzzleSessionManager found!");
        }
    }

    private void ForceNextWave()
    {
        if (!EnsureRunning()) return;

        // Call AdvanceWave via reflection (it's private)
        var method = typeof(RAMPuzzleManager)
            .GetMethod("AdvanceWave", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (method != null)
        {
            method.Invoke(puzzleManager, null);
            Log($"Force-advanced to next wave.");
        }
        else
        {
            Log("Could not find AdvanceWave method via reflection.");
        }
    }

    private void ForceSpawn(string type)
    {
        if (!EnsureRunning()) return;

        int wave = GetPrivateInt("_currentWave");
        var cfgMethod = typeof(RAMPuzzleManager)
            .GetMethod("GetWaveConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var cfg = cfgMethod?.Invoke(puzzleManager, new object[] { wave }) as WaveConfig;
        if (cfg == null) { Log("Could not retrieve WaveConfig."); return; }

        var spawnMethod = type switch
        {
            "corrupted" => "SpawnCorruption",
            "overflow"  => "SpawnCorruption",
            "chain"     => "SpawnCorruption",
            _           => "SpawnCorruption"
        };

        // For chain/overflow we temporarily override chances via a temp WaveConfig clone
        if (type == "overflow")
        {
            ForceSpawnByMethod("SpawnOverflowDebug", cfg.overflowDuration);
            return;
        }
        if (type == "chain")
        {
            ForceSpawnChainDebug(cfg.chainLength);
            return;
        }

        // Plain corrupted
        var method = typeof(RAMPuzzleManager)
            .GetMethod(spawnMethod, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method?.Invoke(puzzleManager, new object[] { cfg });
        Log($"Force-spawned: {type}");
    }

    private void ForceSpawnByMethod(string _, float overflowDuration)
    {
        // Use SpawnCorruption but with a rigged config that guarantees overflow
        var tempCfg = ScriptableObject.CreateInstance<WaveConfig>();
        tempCfg.overflowChance  = 1f;
        tempCfg.chainChance     = 0f;
        tempCfg.overflowDuration = overflowDuration;
        tempCfg.chainLength     = 2;
        tempCfg.spawnInterval   = 99f;

        var method = typeof(RAMPuzzleManager)
            .GetMethod("SpawnCorruption", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method?.Invoke(puzzleManager, new object[] { tempCfg });
        Destroy(tempCfg, 1f);
        Log("Force-spawned: overflow");
    }

    private void ForceSpawnChainDebug(int length)
    {
        var tempCfg = ScriptableObject.CreateInstance<WaveConfig>();
        tempCfg.overflowChance = 0f;
        tempCfg.chainChance    = 1f;
        tempCfg.chainLength    = Mathf.Max(2, length);
        tempCfg.spawnInterval  = 99f;

        var method = typeof(RAMPuzzleManager)
            .GetMethod("SpawnCorruption", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method?.Invoke(puzzleManager, new object[] { tempCfg });
        Destroy(tempCfg, 1f);
        Log($"Force-spawned: chain (length {length})");
    }

    private void AdjustPressure(float delta)
    {
        if (!EnsureRunning()) return;
        if (_pressureField == null) { Log("Pressure field not found."); return; }

        float current = (float)_pressureField.GetValue(puzzleManager);
        float next    = Mathf.Clamp(current + delta, 0f, 100f);
        _pressureField.SetValue(puzzleManager, next);

        // Force HUD refresh
        var method = typeof(RAMPuzzleManager)
            .GetMethod("UpdatePressureBar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method?.Invoke(puzzleManager, null);

        Log($"Pressure adjusted: {current:F0}% → {next:F0}%");
    }

    private void LoseIntegrity()
    {
        if (!EnsureRunning()) return;
        if (_integrityField == null) { Log("Integrity field not found."); return; }

        int current = (int)_integrityField.GetValue(puzzleManager);
        int next    = Mathf.Max(0, current - 1);
        _integrityField.SetValue(puzzleManager, next);

        var method = typeof(RAMPuzzleManager)
            .GetMethod("UpdateAllHUD", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method?.Invoke(puzzleManager, null);

        if (next <= 0)
        {
            var crash = typeof(RAMPuzzleManager)
                .GetMethod("TriggerCrash", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            crash?.Invoke(puzzleManager, new object[] { "DEBUG INTEGRITY DRAIN" });
        }

        Log($"Integrity: {current} → {next}");
    }

    private void ToggleSlowMo()
    {
        _slowMo = !_slowMo;
        // Only change timeScale if puzzle is NOT pausing the game (timeScale == 0 means puzzle is running)
        // During puzzle, game is already at 0 — slow-mo applies OUTSIDE the puzzle
        if (Time.timeScale > 0f || _slowMo)
        {
            Time.timeScale = _slowMo ? 0.25f : 1f;
        }
        if (_lblSlowMo != null)
            _lblSlowMo.text = _slowMo ? "⏱ SLOW-MO 0.25x" : "";
        Log($"Slow-mo: {(_slowMo ? "ON (0.25x)" : "OFF")}");
    }

    private void ToggleOverlay()
    {
        _overlayVisible = !_overlayVisible;
        if (debugOverlayRoot != null)
            debugOverlayRoot.SetActive(_overlayVisible);
    }

    // ─────────────────────────────────────────────────────────────────────
    // OVERLAY — build at runtime so no scene setup needed
    // ─────────────────────────────────────────────────────────────────────
    private void BuildDebugOverlay()
    {
        if (debugOverlayRoot != null) return; // already assigned in Inspector

        // Find or create a canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var go = new GameObject("DebugCanvas");
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
        }

        // Root panel — top-left corner, larger for readability
        debugOverlayRoot = new GameObject("RAMDebugOverlay");
        debugOverlayRoot.transform.SetParent(canvas.transform, false);

        var panel = debugOverlayRoot.AddComponent<Image>();
        panel.color = new Color(0f, 0.02f, 0.06f, 0.92f);

        var rt = debugOverlayRoot.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(12f, -12f);
        rt.sizeDelta = new Vector2(420f, 480f);   // was 270 x 280

        // Title
        MakeLabel(debugOverlayRoot, "[ RAM PUZZLE DEBUG ]", 0f,   -8f, 16, Color.cyan, true, out _);

        // Stats — bigger font, more spacing
        MakeLabel(debugOverlayRoot, "PRESSURE: --",  0f,  -40f, 14, Color.white, false, out _lblPressure);
        MakeLabel(debugOverlayRoot, "INTEGRITY: --", 0f,  -68f, 14, Color.white, false, out _lblIntegrity);
        MakeLabel(debugOverlayRoot, "WAVE: --",      0f,  -96f, 14, Color.white, false, out _lblWave);
        MakeLabel(debugOverlayRoot, "SCORE: --",     0f, -124f, 14, Color.white, false, out _lblScore);

        // Divider
        MakeLabel(debugOverlayRoot, "────────────────────────────", 0f, -152f, 11, new Color(0.2f, 0.6f, 0.6f), false, out _);

        // Hotkey hints — bigger font, more row spacing
        string[] hints =
        {
            "F1   Start / Restart",
            "F2   Force next wave",
            "F3   Spawn corrupted",
            "F4   Spawn overflow",
            "F5   Spawn chain",
            "F6   +20 pressure",
            "F7   -20 pressure",
            "F8   -1 integrity",
            "F9   Slow-mo toggle",
            "F10  Hide overlay",
        };
        for (int i = 0; i < hints.Length; i++)
            MakeLabel(debugOverlayRoot, hints[i], 0f, -172f - i * 22f, 12, new Color(0.55f, 0.9f, 0.55f), false, out _);

        // Status / log line at bottom
        MakeLabel(debugOverlayRoot, "", 0f, -402f, 11, new Color(1f, 0.8f, 0.2f), false, out _lblStatus);

        // Slow-mo badge
        MakeLabel(debugOverlayRoot, "", 0f, -426f, 12, new Color(1f, 0.5f, 0f), false, out _lblSlowMo);
    }

    private void MakeLabel(GameObject parent, string text, float x, float y,
                           int size, Color color, bool bold, out TextMeshProUGUI result)
    {
        var go  = new GameObject("lbl_" + text.Replace(" ", "_").Substring(0, Mathf.Min(12, text.Length)));
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.color     = color;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(x + 12f, y);
        rt.sizeDelta = new Vector2(396f, 22f);   // was 258 x 16

        result = tmp;
    }

    // ─────────────────────────────────────────────────────────────────────
    // OVERLAY REFRESH
    // ─────────────────────────────────────────────────────────────────────
    private void RefreshOverlay()
    {
        if (!_overlayVisible || puzzleManager == null) return;

        float pressure  = _pressureField  != null ? (float)_pressureField.GetValue(puzzleManager)  : -1f;
        int   integrity = _integrityField != null ? (int)_integrityField.GetValue(puzzleManager)    : -1;
        int   wave      = _waveField      != null ? (int)_waveField.GetValue(puzzleManager)         : -1;
        int   score     = _scoreField     != null ? (int)_scoreField.GetValue(puzzleManager)        : -1;
        bool  playing   = _isPlayingField != null ? (bool)_isPlayingField.GetValue(puzzleManager)   : false;

        // Color-code pressure
        Color pColor = pressure > 75f ? Color.red : pressure > 50f ? new Color(1f, 0.5f, 0f) : Color.green;
        if (_lblPressure  != null) { _lblPressure.text  = $"PRESSURE:  {pressure:F1}%"; _lblPressure.color = pColor; }
        if (_lblIntegrity != null)   _lblIntegrity.text  = $"INTEGRITY: {integrity}/3  {(integrity <= 1 ? "⚠" : "")}";
        if (_lblWave      != null)   _lblWave.text       = $"WAVE:      {wave:00}  {(playing ? "● RUNNING" : "○ IDLE")}";
        if (_lblScore     != null)   _lblScore.text      = $"SCORE:     {score:D6}";
    }

    // ─────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────
    private void CacheReflectionFields()
    {
        if (puzzleManager == null) return;
        var t = typeof(RAMPuzzleManager);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        _pressureField  = t.GetField("_pressure",     flags);
        _integrityField = t.GetField("_integrity",    flags);
        _waveField      = t.GetField("_currentWave",  flags);
        _scoreField     = t.GetField("_score",        flags);
        _isPlayingField = t.GetField("_isPlaying",    flags);
    }

    private bool EnsureRunning()
    {
        if (puzzleManager == null) { Log("No RAMPuzzleManager assigned!"); return false; }
        bool playing = _isPlayingField != null && (bool)_isPlayingField.GetValue(puzzleManager);
        if (!playing) { Log("Puzzle not running — press F1 first."); return false; }
        return true;
    }

    private int GetPrivateInt(string fieldName)
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var f = typeof(RAMPuzzleManager).GetField(fieldName, flags);
        return f != null ? (int)f.GetValue(puzzleManager) : 0;
    }

    private void Log(string msg)
    {
        Debug.Log($"[RAMDebug] {msg}");
        if (_lblStatus != null) _lblStatus.text = $"> {msg}";
    }

    // ─────────────────────────────────────────────────────────────────────
    // EDITOR: show hotkey reminder in Scene view gizmo label
    // ─────────────────────────────────────────────────────────────────────
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.color = Color.cyan;
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.5f,
            "RAM DEBUG RUNNER\nF1=Start F2=NextWave\nF3=Corrupt F4=Overflow F5=Chain");
    }
#endif
}