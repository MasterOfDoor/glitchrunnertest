using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main controller for the RAM Overload puzzle.
///
/// SETUP in Unity:
///   1. Create a UI Canvas with a GridLayoutGroup for the cell grid.
///   2. Create a MemoryCell prefab (Button + MemoryCell component, child UI elements).
///   3. Assign WaveConfig ScriptableObjects for each wave (one per slot in waveConfigs[]).
///   4. Wire up the HUD TextMeshPro references and the pressure bar Image (fillAmount).
///   5. Assign the puzzleCompleted / puzzleFailed UnityEvents in the Inspector
///      so the scene manager knows when the puzzle is over.
/// </summary>
public class RAMPuzzleManager : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────
    [Header("Grid")]
    [SerializeField] private GameObject     cellPrefab;
    [SerializeField] private Transform      gridParent;       // GridLayoutGroup parent
    [SerializeField] private int            gridColumns = 8;
    [SerializeField] private int            gridRows    = 5;

    [Header("Waves")]
    [SerializeField] private WaveConfig[]   waveConfigs;      // assign in Inspector (5 recommended)
    [Tooltip("Loop back to this config index once all explicit waves are exhausted")]
    [SerializeField] private int            waveLoopIndex = 4;

    [Header("Pressure")]
    [SerializeField] private Image          pressureBarFill;  // Horizontal fill image
    [SerializeField] private TextMeshProUGUI pressureText;

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI clearedText;
    [SerializeField] private TextMeshProUGUI chainCountText;

    [Header("Integrity (Hearts)")]
    [SerializeField] private Image[]        heartImages;      // 3 heart UI images
    [SerializeField] private Color          heartActiveColor  = Color.red;
    [SerializeField] private Color          heartDeadColor    = new Color(0.3f, 0.3f, 0.3f, 0.25f);

    [Header("Status Bar")]
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Overlays")]
    [SerializeField] private GameObject     waveBannerPanel;
    [SerializeField] private TextMeshProUGUI waveBannerText;
    [SerializeField] private float          waveBannerDuration = 2f;

    [Header("Score Popup Prefab")]
    [Tooltip("Prefab with TextMeshProUGUI that floats up and fades. Optional.")]
    [SerializeField] private GameObject     scorePopupPrefab;
    [SerializeField] private Canvas         popupCanvas;

    [Header("Audio (optional)")]
    [SerializeField] private AudioClip      sfxFlush;
    [SerializeField] private AudioClip      sfxChainComplete;
    [SerializeField] private AudioClip      sfxOverflowWarning;
    [SerializeField] private AudioClip      sfxOverflowExpired;
    [SerializeField] private AudioClip      sfxWrongOrder;
    [SerializeField] private AudioClip      sfxWaveStart;
    [SerializeField] private AudioSource    audioSource;

    [Header("Events")]
    [Tooltip("Called when the player clears all waves successfully")]
    public UnityEngine.Events.UnityEvent OnPuzzleCompleted;
    [Tooltip("Called when the system crashes (integrity=0 or pressure=100)")]
    public UnityEngine.Events.UnityEvent OnPuzzleFailed;

    // ── Private State ──────────────────────────────────────────────────
    private MemoryCell[] _cells;
    private int          _totalCells;

    // Puzzle state
    private int   _currentWave   = 1;
    private int   _score         = 0;
    private int   _integrity     = 3;
    private float _pressure      = 0f;
    private int   _clearedCount  = 0;
    private bool  _isPlaying     = false;

    // Chain groups: gid → ChainGroup
    private Dictionary<int, ChainGroup> _chainGroups = new();
    private int _nextGid = 0;

    // Timers
    private Coroutine _spawnRoutine;
    private Coroutine _pressureRoutine;

    // ── Inner Types ────────────────────────────────────────────────────
    private class ChainGroup
    {
        public int[]  CellIndices;
        public int    NextStep;
    }

    // ─────────────────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _totalCells = gridColumns * gridRows;
    }

    /// <summary>Call this to begin the puzzle (e.g., from a scene transition or trigger).</summary>
    public void BeginPuzzle()
    {
        _currentWave  = 1;
        _score        = 0;
        _integrity    = 3;
        _pressure     = 0f;
        _clearedCount = 0;
        _chainGroups.Clear();

        BuildGrid();
        UpdateAllHUD();
        StartCoroutine(WaveBannerThenLaunch(_currentWave));
    }

    // ─────────────────────────────────────────────────────────────────────
    // GRID SETUP
    // ─────────────────────────────────────────────────────────────────────
    private void BuildGrid()
    {
        // Clear old cells
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        _cells = new MemoryCell[_totalCells];

        for (int i = 0; i < _totalCells; i++)
        {
            string hexAddr = "0x" + (0xA000 + i * 4).ToString("X4");
            GameObject go  = Instantiate(cellPrefab, gridParent);
            MemoryCell mc  = go.GetComponent<MemoryCell>();

            mc.Initialize(i, hexAddr);
            mc.OnCellClicked    += HandleCellClicked;
            mc.OnOverflowExpired += HandleOverflowExpired;

            _cells[i] = mc;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // WAVE MANAGEMENT
    // ─────────────────────────────────────────────────────────────────────
    private WaveConfig GetWaveConfig(int wave)
    {
        if (waveConfigs == null || waveConfigs.Length == 0)
        {
            Debug.LogError("[RAMPuzzle] No wave configs assigned!");
            return null;
        }
        int idx = wave - 1;
        // Once explicit waves are exhausted, loop from waveLoopIndex
        if (idx >= waveConfigs.Length)
            idx = waveLoopIndex + ((idx - waveLoopIndex) % (waveConfigs.Length - waveLoopIndex));
        idx = Mathf.Clamp(idx, 0, waveConfigs.Length - 1);
        return waveConfigs[idx];
    }

    private IEnumerator WaveBannerThenLaunch(int wave)
    {
        _isPlaying = false;
        StopGameLoops();

        if (waveBannerPanel != null)
        {
            waveBannerPanel.SetActive(true);
            if (waveBannerText != null)
                waveBannerText.text = $"WAVE {wave:00} — INITIATED";
            PlaySfx(sfxWaveStart);
        }

        // WaitForSecondsRealtime: works correctly when Time.timeScale = 0 (puzzle pauses game)
        yield return new WaitForSecondsRealtime(waveBannerDuration);

        if (waveBannerPanel != null) waveBannerPanel.SetActive(false);

        LaunchWave();
    }

    private void LaunchWave()
    {
        _isPlaying = true;
        StopGameLoops();

        WaveConfig cfg = GetWaveConfig(_currentWave);
        if (cfg == null) return;

        _spawnRoutine    = StartCoroutine(SpawnLoop(cfg));
        _pressureRoutine = StartCoroutine(PressureLoop(cfg));

        SetStatus($"// WAVE {_currentWave:00} — CORRUPTION SPREADING THROUGH MEMORY BANKS");
        UpdateAllHUD();
    }

    private void AdvanceWave()
    {
        StopGameLoops();
        _isPlaying = false;
        _currentWave++;
        _clearedCount = 0;
        _pressure     = Mathf.Max(0f, _pressure - 18f);
        AddScore(1000 * (_currentWave - 1));

        // Reset all cells
        _chainGroups.Clear();
        foreach (var cell in _cells) cell.ForceReset();

        // Check if all explicit waves completed (victory condition — adjust as needed)
        if (_currentWave > waveConfigs.Length + 3)   // arbitrary "you escaped" threshold
        {
            OnPuzzleCompleted?.Invoke();
            return;
        }

        UpdateAllHUD();
        StartCoroutine(WaveBannerThenLaunch(_currentWave));
    }

    private void StopGameLoops()
    {
        if (_spawnRoutine    != null) { StopCoroutine(_spawnRoutine);    _spawnRoutine    = null; }
        if (_pressureRoutine != null) { StopCoroutine(_pressureRoutine); _pressureRoutine = null; }
    }

    // ─────────────────────────────────────────────────────────────────────
    // SPAWN LOOP
    // ─────────────────────────────────────────────────────────────────────
    private IEnumerator SpawnLoop(WaveConfig cfg)
    {
        while (_isPlaying)
        {
            // WaitForSecondsRealtime: unaffected by Time.timeScale = 0
            yield return new WaitForSecondsRealtime(cfg.spawnInterval);
            if (_isPlaying) SpawnCorruption(cfg);
        }
    }

    private void SpawnCorruption(WaveConfig cfg)
    {
        int[] freeIndices = GetFreeIndices();
        if (freeIndices.Length < 4) return;

        float roll = UnityEngine.Random.value;

        if (roll < cfg.overflowChance)
        {
            int idx = freeIndices[UnityEngine.Random.Range(0, freeIndices.Length)];
            _cells[idx].SetState(CellState.Overflow, cfg.overflowDuration);
            SetStatus($"// ⚠ OVERFLOW IMMINENT — SECTOR {_cells[idx].HexAddress}");
            PlaySfx(sfxOverflowWarning);
        }
        else if (roll < cfg.overflowChance + cfg.chainChance)
        {
            SpawnChain(freeIndices, cfg.chainLength);
        }
        else
        {
            int idx = freeIndices[UnityEngine.Random.Range(0, freeIndices.Length)];
            _cells[idx].SetState(CellState.Corrupted);
            SetStatus($"// CORRUPTED SECTOR: {_cells[idx].HexAddress}");
        }
    }

    private void SpawnChain(int[] freeIndices, int length)
    {
        int len = Mathf.Min(length, freeIndices.Length);
        if (len < 2) return;

        // Shuffle and pick `len` cells
        int[] shuffled = freeIndices.OrderBy(_ => UnityEngine.Random.value).Take(len).ToArray();
        int gid = _nextGid++;
        var group = new ChainGroup { CellIndices = shuffled, NextStep = 0 };
        _chainGroups[gid] = group;

        for (int i = 0; i < shuffled.Length; i++)
        {
            int idx = shuffled[i];
            _cells[idx].ChainGroupId = gid;
            _cells[idx].SetChainNumber(i + 1);
            _cells[idx].SetState(CellState.Chained);
        }

        ActivateNextChainStep(gid);
        SetStatus($"// CHAIN LOCK DETECTED — {len}-SECTOR SEQUENCE REQUIRED");
    }

    private void ActivateNextChainStep(int gid)
    {
        if (!_chainGroups.TryGetValue(gid, out var group)) return;
        if (group.NextStep >= group.CellIndices.Length) return;

        int idx = group.CellIndices[group.NextStep];
        _cells[idx].SetState(CellState.ChainActive);
    }

    // ─────────────────────────────────────────────────────────────────────
    // PRESSURE LOOP
    // ─────────────────────────────────────────────────────────────────────
    private IEnumerator PressureLoop(WaveConfig cfg)
    {
        while (_isPlaying)
        {
            // WaitForSecondsRealtime + Time.unscaledDeltaTime: both required when timeScale = 0
            yield return new WaitForSecondsRealtime(0.08f);
            if (!_isPlaying) break;

            float increase = cfg.pressureIncreaseRate * 0.08f;

            // Count active corrupted cells and add passive pressure
            int activeCount = _cells.Count(c =>
                c.State == CellState.Corrupted ||
                c.State == CellState.Overflow   ||
                c.State == CellState.Chained     ||
                c.State == CellState.ChainActive);

            increase += activeCount * cfg.pressurePerActiveCell * 0.08f;

            _pressure = Mathf.Min(100f, _pressure + increase);
            UpdatePressureBar();

            if (_pressure >= 100f)
            {
                TriggerCrash("RAM OVERFLOW");
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // CLICK HANDLER
    // ─────────────────────────────────────────────────────────────────────
    private void HandleCellClicked(MemoryCell cell)
    {
        if (!_isPlaying) return;

        WaveConfig cfg = GetWaveConfig(_currentWave);
        int idx = cell.GridIndex;

        switch (cell.State)
        {
            case CellState.Corrupted:
                FlushCell(idx);
                AddScore(100);
                ShowPopup(idx, "+100", new Color(1f, 0.33f, 0f));  // orange
                _pressure = Mathf.Max(0f, _pressure - 7f);
                SetStatus($"// SECTOR {cell.HexAddress} FLUSHED");
                PlaySfx(sfxFlush);
                RegisterCleared(1);
                break;

            case CellState.Overflow:
                FlushCell(idx);
                AddScore(300);
                ShowPopup(idx, "+300", new Color(1f, 0f, 0.25f));  // red
                _pressure = Mathf.Max(0f, _pressure - 13f);
                SetStatus($"// OVERFLOW CONTAINED — SECTOR {cell.HexAddress} SECURED");
                PlaySfx(sfxFlush);
                RegisterCleared(1);
                break;

            case CellState.ChainActive:
                int gid = cell.ChainGroupId;
                if (!_chainGroups.TryGetValue(gid, out var group)) break;

                FlushCell(idx);
                AddScore(150);
                group.NextStep++;

                if (group.NextStep >= group.CellIndices.Length)
                {
                    // Chain complete!
                    int bonus = 400 * group.CellIndices.Length;
                    AddScore(bonus);
                    ShowPopup(idx, $"+CHAIN ×{group.CellIndices.Length}", new Color(1f, 0.84f, 0f));  // gold
                    _pressure = Mathf.Max(0f, _pressure - 16f);
                    _chainGroups.Remove(gid);
                    SetStatus($"// CHAIN SEQUENCE CLEARED — BONUS ×{group.CellIndices.Length} AWARDED");
                    PlaySfx(sfxChainComplete);
                    RegisterCleared(group.CellIndices.Length);
                }
                else
                {
                    ShowPopup(idx, "+150", new Color(1f, 0.84f, 0f));  // gold
                    ActivateNextChainStep(gid);
                    SetStatus($"// CHAIN {group.NextStep}/{group.CellIndices.Length} — CONTINUE SEQUENCE");
                    PlaySfx(sfxFlush);
                }
                break;

            case CellState.Chained:
                // Wrong order: penalise
                if (cfg != null) _pressure = Mathf.Min(100f, _pressure + cfg.wrongOrderPenalty);
                SetStatus($"// ✗ WRONG SEQUENCE — FLUSH IN ORDER (see chain number)");
                PlaySfx(sfxWrongOrder);
                StartCoroutine(ShakeCell(cell.transform));
                break;
        }

        UpdateAllHUD();
    }

    // ─────────────────────────────────────────────────────────────────────
    // OVERFLOW EXPIRED
    // ─────────────────────────────────────────────────────────────────────
    private void HandleOverflowExpired(MemoryCell cell)
    {
        if (!_isPlaying) return;

        WaveConfig cfg = GetWaveConfig(_currentWave);
        int idx = cell.GridIndex;

        _integrity = Mathf.Max(0, _integrity - 1);
        if (cfg != null)
            _pressure = Mathf.Min(100f, _pressure + cfg.overflowExpiryPressureSpike);

        // Cascade: corrupt up to 2 free neighbours
        int[] neighbours = GetNeighbours(idx);
        int cascadeCount = 0;
        foreach (int n in neighbours)
        {
            if (_cells[n].State == CellState.Normal && cascadeCount < 2)
            {
                _cells[n].SetState(CellState.Corrupted);
                cascadeCount++;
            }
        }

        cell.ForceReset();
        SetStatus("// ⚠ OVERFLOW EXPIRED — INTEGRITY BREACH — CASCADE TRIGGERED");
        PlaySfx(sfxOverflowExpired);
        UpdateAllHUD();

        if (_integrity <= 0) TriggerCrash("INTEGRITY FAILURE");
    }

    // ─────────────────────────────────────────────────────────────────────
    // CELL HELPERS
    // ─────────────────────────────────────────────────────────────────────
    private void FlushCell(int idx)
    {
        _cells[idx].ChainGroupId = -1;
        _cells[idx].SetState(CellState.Flushing);
    }

    private void RegisterCleared(int count)
    {
        _clearedCount += count;
        WaveConfig cfg = GetWaveConfig(_currentWave);
        UpdateAllHUD();
        if (cfg != null && _clearedCount >= cfg.cellsToComplete)
            AdvanceWave();
    }

    private int[] GetFreeIndices()
    {
        var list = new List<int>();
        for (int i = 0; i < _totalCells; i++)
            if (_cells[i].State == CellState.Normal) list.Add(i);
        return list.ToArray();
    }

    private int[] GetNeighbours(int idx)
    {
        var list = new List<int>();
        int r = idx / gridColumns, c = idx % gridColumns;
        if (r > 0)                list.Add(idx - gridColumns);
        if (r < gridRows - 1)     list.Add(idx + gridColumns);
        if (c > 0)                list.Add(idx - 1);
        if (c < gridColumns - 1)  list.Add(idx + 1);
        return list.ToArray();
    }

    private IEnumerator ShakeCell(Transform t)
    {
        Vector3 origin = t.localPosition;
        float[] offsets = { -5f, 5f, -4f, 4f, -2f, 0f };
        foreach (float x in offsets)
        {
            t.localPosition = origin + new Vector3(x, 0f, 0f);
            yield return new WaitForSecondsRealtime(0.04f);  // unaffected by timeScale = 0
        }
        t.localPosition = origin;
    }

    // ─────────────────────────────────────────────────────────────────────
    // CRASH
    // ─────────────────────────────────────────────────────────────────────
    private void TriggerCrash(string reason)
    {
        if (!_isPlaying) return;
        _isPlaying = false;
        StopGameLoops();
        SetStatus($"// FATAL ERROR 0xDEADBEEF — {reason}");
        Debug.Log($"[RAMPuzzle] SYSTEM CRASH: {reason} | Score: {_score} | Wave: {_currentWave}");
        OnPuzzleFailed?.Invoke();
    }

    // ─────────────────────────────────────────────────────────────────────
    // HUD
    // ─────────────────────────────────────────────────────────────────────
    private void UpdateAllHUD()
    {
        if (waveText    != null) waveText.text    = $"WAVE {_currentWave:00}";
        if (scoreText   != null) scoreText.text   = _score.ToString("D6");

        WaveConfig cfg = GetWaveConfig(_currentWave);
        if (clearedText != null && cfg != null)
            clearedText.text = $"{_clearedCount:D2}/{cfg.cellsToComplete}";

        int activeChains = _chainGroups.Count;
        if (chainCountText != null)
            chainCountText.text = activeChains > 0 ? $"{activeChains} ACTIVE" : "--";

        UpdatePressureBar();
        UpdateHearts();
    }

    private void UpdatePressureBar()
    {
        float pct = _pressure / 100f;
        if (pressureBarFill != null) pressureBarFill.fillAmount = pct;
        if (pressureText    != null) pressureText.text = $"{Mathf.RoundToInt(_pressure)}%";
    }

    private void UpdateHearts()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null) continue;
            heartImages[i].color = i < _integrity ? heartActiveColor : heartDeadColor;
        }
    }

    private void AddScore(int pts)
    {
        _score += pts;
        if (scoreText != null) scoreText.text = _score.ToString("D6");
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }

    // ─────────────────────────────────────────────────────────────────────
    // SCORE POPUP
    // ─────────────────────────────────────────────────────────────────────
    private void ShowPopup(int cellIdx, string text, Color color)
    {
        if (scorePopupPrefab == null || popupCanvas == null) return;

        RectTransform cellRect = _cells[cellIdx].GetComponent<RectTransform>();
        if (cellRect == null) return;

        GameObject popup = Instantiate(scorePopupPrefab, popupCanvas.transform);
        popup.GetComponent<RectTransform>().position = cellRect.position;
        popup.GetComponent<ScorePopup>()?.Setup(text, color);
    }

    // ─────────────────────────────────────────────────────────────────────
    // AUDIO
    // ─────────────────────────────────────────────────────────────────────
    private void PlaySfx(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }
}