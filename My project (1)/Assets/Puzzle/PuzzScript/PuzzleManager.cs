using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject      backgroundDim;
    public GameObject      puzzlePanel;
    public RectTransform   codeContainer;
    public GameObject      codeLinePrefab;
    public TMP_Text        timerText;
    public MatrixBitWriter matrixWriter;

    [Header("Font Override (boş bırakılırsa prefab fontu kullanılır)")]
    public TMP_FontAsset   overrideFont;

    [Header("Target Area")]
    public GameObject targetSlotPrefab;
    public Transform  targetArea;

    [Header("Puzzle Data")]
    public List<string> correctOrder = new List<string>();

    [Header("Settings")]
    public bool  autoStartOnPlay = true;
    public float timeLimit       = 60f;

    [Header("Events — dışarıdan bağla")]
    public UnityEvent OnPuzzleCompleted;
    public UnityEvent OnPuzzleFailed;

    // ── Slot renkleri ─────────────────────────────────────────────────────
    private static readonly Color SlotEmpty   = new Color(0.02f, 0.10f, 0.08f, 1.00f);
    private static readonly Color SlotFilled  = new Color(0.00f, 0.55f, 0.35f, 0.25f);
    private static readonly Color SlotCorrect = new Color(0.00f, 1.00f, 0.53f, 0.35f);
    private static readonly Color SlotWrong   = new Color(1.00f, 0.00f, 0.25f, 0.30f);

    // ── Private ───────────────────────────────────────────────────────────
    private List<CodeLine> _lines        = new List<CodeLine>();
    private float          _timer;
    private bool           _timerRunning;
    private bool           _puzzleActive;
    private bool           _solved;

    // ─────────────────────────────────────────────────────────────────────
    void Start()
    {
        ClosePuzzle();
        if (autoStartOnPlay) OpenPuzzle();
    }

    void Update()
    {
        if (!_puzzleActive || !_timerRunning) return;

        _timer -= Time.unscaledDeltaTime;

        // Renk geçişi: yeşil → sarı → kırmızı
        float ratio     = Mathf.Clamp01(_timer / timeLimit);
        timerText.color = ratio > 0.5f
            ? Color.Lerp(new Color(1f, 0.8f, 0f), new Color(0f, 1f, 0.53f), (ratio - 0.5f) * 2f)
            : Color.Lerp(new Color(1f, 0.05f, 0.25f), new Color(1f, 0.8f, 0f), ratio * 2f);

        timerText.text = Mathf.Ceil(_timer).ToString("00");

        if (_timer <= 0f)
        {
            _timerRunning = false;
            StartCoroutine(FailRoutine());
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // PUBLIC API
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>PuzzleSessionManager veya trigger sistemi bu metodu çağırır.</summary>
    public void BeginPuzzle() => OpenPuzzle();

    public void OpenPuzzle()
    {
        if (_puzzleActive) return;
        _puzzleActive = true;
        _solved       = false;
        _timerRunning = false;

        Time.timeScale = 0f;

        backgroundDim.SetActive(true);
        puzzlePanel.SetActive(true);

        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
            timerText.color = new Color(0f, 1f, 0.53f);
        }

        if (matrixWriter != null)
            matrixWriter.OnFinished = StartTimer;

        GeneratePuzzle();
    }

    public void ClosePuzzle()
    {
        _puzzleActive = false;
        _timerRunning = false;

        Time.timeScale = 1f;

        if (backgroundDim != null) backgroundDim.SetActive(false);
        if (puzzlePanel   != null) puzzlePanel.SetActive(false);
        if (timerText     != null) timerText.gameObject.SetActive(false);

        foreach (Transform c in codeContainer) Destroy(c.gameObject);
        foreach (Transform c in targetArea)    Destroy(c.gameObject);

        _lines.Clear();
    }

    // ─────────────────────────────────────────────────────────────────────
    // TIMER
    // ─────────────────────────────────────────────────────────────────────
    void StartTimer()
    {
        _timer        = timeLimit;
        _timerRunning = true;
        if (timerText != null) timerText.gameObject.SetActive(true);
    }

    // ─────────────────────────────────────────────────────────────────────
    // GENERATION
    // ─────────────────────────────────────────────────────────────────────
    void GeneratePuzzle()
    {
        for (int i = 0; i < correctOrder.Count; i++)
        {
            GameObject slot = Instantiate(targetSlotPrefab, targetArea);
            slot.name = $"Slot_{i}";
            var img = slot.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.color = SlotEmpty;
        }

        List<string> shuffled = new List<string>(correctOrder);
        Shuffle(shuffled);

        StartCoroutine(SpawnLines(shuffled));
    }

    IEnumerator SpawnLines(List<string> shuffled)
    {
        // 2 frame bekle — canvas ve codeContainer boyutları hazır olsun
        yield return null;
        yield return null;

        List<Rect> occupied = new List<Rect>();

        // Sağ üst bölgeyi (timer + matrix) rezerve et
        occupied.Add(new Rect(160f,  80f, 270f,  70f)); // timer
        occupied.Add(new Rect(160f, -80f, 270f, 150f)); // matrix

        foreach (string line in shuffled)
        {
            GameObject    obj  = Instantiate(codeLinePrefab, codeContainer);
            RectTransform rt   = obj.GetComponent<RectTransform>();

            yield return null; // prefab layout bir frame sonra hazır

            Vector2 size = new Vector2(
                rt.rect.width  > 1f ? rt.rect.width  : 280f,
                rt.rect.height > 1f ? rt.rect.height :  44f
            );

            Vector2 pos = SafePos(occupied, size);
            rt.anchoredPosition = pos;
            occupied.Add(new Rect(pos, size));

            CodeLine code = obj.GetComponent<CodeLine>();
            code.Setup(line, this, overrideFont);
            _lines.Add(code);
        }
    }

    Vector2 SafePos(List<Rect> occupied, Vector2 size, int tries = 80)
    {
        Rect c    = codeContainer.rect;
        float pad = 8f;
        // Sol %58'e sınırla — sağ taraf matrix/timer için
        float xMax = c.xMin + c.width * 0.58f;
        float xMin = c.xMin + pad;
        float yMin = c.yMin + pad;
        float yMax = c.yMax - size.y - pad;

        for (int i = 0; i < tries; i++)
        {
            float x = Random.Range(xMin, xMax - size.x);
            float y = Random.Range(yMin, yMax);
            Rect  r = new Rect(x, y, size.x, size.y);

            bool hit = false;
            foreach (Rect o in occupied) { if (r.Overlaps(o)) { hit = true; break; } }
            if (!hit) return new Vector2(x, y);
        }

        // Fallback: üst üste de olsa yerleştir
        return new Vector2(xMin, yMax - occupied.Count * (size.y + 6f));
    }

    // ─────────────────────────────────────────────────────────────────────
    // CHECK
    // ─────────────────────────────────────────────────────────────────────
    public void CheckTargetSlots()
    {
        RefreshSlotColors();

        // Tüm slotlar dolu mu?
        for (int i = 0; i < targetArea.childCount; i++)
            if (targetArea.GetChild(i).childCount == 0) return;

        // Sıra doğru mu?
        for (int i = 0; i < correctOrder.Count; i++)
        {
            Transform slot = targetArea.GetChild(i);
            if (slot.childCount == 0) return;
            CodeLine  line = slot.GetChild(0).GetComponent<CodeLine>();
            if (line == null || line.text != correctOrder[i]) return;
        }

        if (!_solved) { _solved = true; StartCoroutine(SuccessRoutine()); }
    }

    public void RefreshSlotColors()
    {
        for (int i = 0; i < targetArea.childCount; i++)
        {
            Transform slot = targetArea.GetChild(i);
            var img = slot.GetComponent<UnityEngine.UI.Image>();
            if (img == null) continue;

            if (slot.childCount == 0)
            {
                img.color = SlotEmpty;
            }
            else
            {
                CodeLine line    = slot.GetChild(0).GetComponent<CodeLine>();
                bool     correct = line != null && i < correctOrder.Count && line.text == correctOrder[i];
                img.color = correct ? SlotCorrect : SlotFilled;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // SUCCESS / FAIL
    // ─────────────────────────────────────────────────────────────────────
    IEnumerator SuccessRoutine()
    {
        _timerRunning = false;

        for (int i = 0; i < targetArea.childCount; i++)
        {
            var img = targetArea.GetChild(i).GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.color = SlotCorrect;
        }

        if (timerText != null)
        {
            timerText.text  = "ACCESS GRANTED";
            timerText.color = new Color(0f, 1f, 0.53f);
            timerText.gameObject.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(1.0f);

        ClosePuzzle();
        OnPuzzleCompleted?.Invoke();
    }

    IEnumerator FailRoutine()
    {
        for (int i = 0; i < targetArea.childCount; i++)
        {
            var img = targetArea.GetChild(i).GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.color = SlotWrong;
        }

        if (timerText != null)
        {
            timerText.text  = "SYSTEM CRASH";
            timerText.color = new Color(1f, 0.05f, 0.25f);
            timerText.gameObject.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(1.2f);

        ClosePuzzle();
        OnPuzzleFailed?.Invoke();
    }

    // ─────────────────────────────────────────────────────────────────────
    void Shuffle(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}