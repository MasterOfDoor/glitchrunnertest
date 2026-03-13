using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum CellState { Normal, Corrupted, Chained, ChainActive, Overflow, Flushing }

/// <summary>
/// Controls the visual state and interaction for a single RAM memory cell.
/// Attach to each cell prefab. Configure color/visual references in the Inspector.
/// </summary>
public class MemoryCell : MonoBehaviour
{
    // ── Public Properties ──────────────────────────────────────────────
    public CellState State         { get; private set; } = CellState.Normal;
    public int       GridIndex     { get; set; }
    public int       ChainGroupId  { get; set; } = -1;
    public int       ChainOrder    { get; set; } = -1;
    public string    HexAddress    { get; private set; }

    // ── Events ─────────────────────────────────────────────────────────
    /// <summary>Fired when this overflow cell's timer expires without being cleared.</summary>
    public event Action<MemoryCell> OnOverflowExpired;
    /// <summary>Fired when the player clicks this cell.</summary>
    public event Action<MemoryCell> OnCellClicked;

    // ── Inspector References ───────────────────────────────────────────
    [Header("UI References")]
    [SerializeField] private Image       cellBackground;
    [SerializeField] private Image       borderGlow;
    [SerializeField] private TextMeshProUGUI addressText;
    [SerializeField] private TextMeshProUGUI chainNumberText;
    [SerializeField] private Image       overflowTimerBar;   // fill image, fillAmount 1→0
    [SerializeField] private Image       cornerPip;

    [Header("Normal State Colors")]
    [SerializeField] private Color normalBg        = new Color(0.012f, 0.043f, 0.082f);
    [SerializeField] private Color normalBorder     = new Color(0.05f,  0.12f,  0.19f);
    [SerializeField] private Color normalAddrColor  = new Color(0.04f,  0.29f,  0.35f);

    [Header("Corrupted State Colors")]
    [SerializeField] private Color corruptBg        = new Color(0.063f, 0.02f,  0f);
    [SerializeField] private Color corruptColor     = new Color(1f,     0.33f,  0f);

    [Header("Chained State Colors")]
    [SerializeField] private Color chainBg          = new Color(0.04f,  0.04f,  0f);
    [SerializeField] private Color chainColor       = new Color(1f,     0.84f,  0f);

    [Header("Overflow State Colors")]
    [SerializeField] private Color overflowBg       = new Color(0.06f,  0f,     0.03f);
    [SerializeField] private Color overflowColor    = new Color(1f,     0f,     0.25f);

    [Header("Flush Colors")]
    [SerializeField] private Color flushStartColor  = Color.white;
    [SerializeField] private Color flushMidColor    = new Color(0f, 0.9f, 1f);
    [SerializeField] private float flushDuration    = 0.45f;

    [Header("Pulse Animation")]
    [SerializeField] private float corruptPulseSpeed   = 1.1f;
    [SerializeField] private float overflowPulseSpeed  = 2.8f;
    [SerializeField] private float chainPulseSpeed     = 1.8f;

    // ── Private ─────────────────────────────────────────────────────────
    private Button   _button;
    private float    _overflowTimeLeft;
    private float    _overflowDuration;
    private bool     _overflowRunning;
    private float    _pulseT;
    private Coroutine _flushRoutine;

    // ─────────────────────────────────────────────────────────────────────
    // LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
            _button.onClick.AddListener(() => OnCellClicked?.Invoke(this));
    }

    private void Update()
    {
        TickPulse();
        TickOverflowTimer();
    }

    // ─────────────────────────────────────────────────────────────────────
    // PUBLIC API
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Call once after instantiation to set grid position and address.</summary>
    public void Initialize(int index, string hexAddress)
    {
        GridIndex  = index;
        HexAddress = hexAddress;
        if (addressText != null) addressText.text = hexAddress;
        SetState(CellState.Normal);
    }

    /// <summary>Transition cell to a new state. Pass overflowDuration when setting Overflow.</summary>
    public void SetState(CellState newState, float overflowDuration = 3f)
    {
        StopAllPulse();
        _overflowRunning = false;
        State = newState;

        switch (newState)
        {
            case CellState.Normal:
                ApplyColors(normalBg, normalBorder, normalAddrColor);
                ShowAddress(true);
                ShowChainNum(false);
                ShowTimerBar(false);
                SetInteractable(false);
                break;

            case CellState.Corrupted:
                ApplyColors(corruptBg, corruptColor, corruptColor);
                ShowAddress(true);
                ShowChainNum(false);
                ShowTimerBar(false);
                SetInteractable(true);
                break;

            case CellState.Chained:
                ApplyColors(chainBg, chainColor, chainColor);
                ShowAddress(false);
                ShowChainNum(true);
                ShowTimerBar(false);
                SetInteractable(false);   // only ChainActive is clickable
                break;

            case CellState.ChainActive:
                ApplyColors(chainBg, chainColor, chainColor);
                ShowAddress(false);
                ShowChainNum(true);
                ShowTimerBar(false);
                SetInteractable(true);
                break;

            case CellState.Overflow:
                ApplyColors(overflowBg, overflowColor, overflowColor);
                ShowAddress(true);
                ShowChainNum(false);
                ShowTimerBar(true, 1f);
                SetInteractable(true);
                _overflowDuration  = overflowDuration;
                _overflowTimeLeft  = overflowDuration;
                _overflowRunning   = true;
                break;

            case CellState.Flushing:
                SetInteractable(false);
                ShowChainNum(false);   // chain numarasını hemen gizle
                ShowAddress(false);
                ShowTimerBar(false);
                if (_flushRoutine != null) StopCoroutine(_flushRoutine);
                _flushRoutine = StartCoroutine(FlushRoutine());
                break;
        }
    }

    /// <summary>Set the chain sequence number displayed on this cell.</summary>
    public void SetChainNumber(int number)
    {
        ChainOrder = number;
        if (chainNumberText != null) chainNumberText.text = number.ToString();
    }

    /// <summary>Reset all state fields (used when clearing a wave).</summary>
    public void ForceReset()
    {
        _overflowRunning = false;
        ChainGroupId = -1;
        ChainOrder   = -1;
        if (_flushRoutine != null) { StopCoroutine(_flushRoutine); _flushRoutine = null; }
        SetState(CellState.Normal);
    }

    // ─────────────────────────────────────────────────────────────────────
    // PRIVATE HELPERS
    // ─────────────────────────────────────────────────────────────────────
    private void TickPulse()
    {
        if (State == CellState.Normal || State == CellState.Flushing) return;

        float speed = State switch
        {
            CellState.Corrupted   => corruptPulseSpeed,
            CellState.Overflow    => overflowPulseSpeed,
            CellState.ChainActive => chainPulseSpeed,
            _                     => 0f
        };

        if (speed <= 0f) return;

        _pulseT += Time.unscaledDeltaTime * speed;
        float alpha = 0.5f + 0.5f * Mathf.Sin(_pulseT * Mathf.PI * 2f);

        Color glowColor = State switch
        {
            CellState.Corrupted   => corruptColor,
            CellState.Overflow    => overflowColor,
            CellState.ChainActive => chainColor,
            _                     => Color.clear
        };

        if (borderGlow != null)
        {
            Color c = glowColor;
            c.a = alpha * 0.85f;
            borderGlow.color = c;
        }
    }

    private void TickOverflowTimer()
    {
        if (!_overflowRunning || State != CellState.Overflow) return;

        _overflowTimeLeft -= Time.unscaledDeltaTime;
        float fraction = Mathf.Clamp01(_overflowTimeLeft / _overflowDuration);

        if (overflowTimerBar != null)
            overflowTimerBar.fillAmount = fraction;

        if (_overflowTimeLeft <= 0f)
        {
            _overflowRunning = false;
            OnOverflowExpired?.Invoke(this);
        }
    }

    private IEnumerator FlushRoutine()
    {
        // ── Step 1: Instant white flash ──────────────────────────────────
        ApplyColors(flushStartColor, flushStartColor, flushStartColor);
        ShowAddress(false);
        ShowTimerBar(false);

        yield return new WaitForSecondsRealtime(0.06f);

        // ── Step 2: Cyan mid-flash ───────────────────────────────────────
        ApplyColors(flushMidColor, flushMidColor, flushMidColor);

        yield return new WaitForSecondsRealtime(0.04f);

        // ── Step 3: Fade from cyan → normal over flushDuration ───────────
        float t = 0f;
        while (t < flushDuration)
        {
            // unscaledDeltaTime: keeps animating when Time.timeScale = 0
            t += Time.unscaledDeltaTime;
            float p   = Mathf.Clamp01(t / flushDuration);
            Color bg  = Color.Lerp(flushMidColor, normalBg,     p);
            Color bdr = Color.Lerp(flushMidColor, normalBorder, p);
            Color adr = Color.Lerp(flushMidColor, normalAddrColor, p);
            ApplyColors(bg, bdr, adr);
            yield return null;
        }

        // ── Step 4: Snap to final normal state ───────────────────────────
        State        = CellState.Normal;
        ChainGroupId = -1;
        ChainOrder   = -1;
        ApplyColors(normalBg, normalBorder, normalAddrColor);
        ShowAddress(true);
        ShowTimerBar(false);
        _flushRoutine = null;
    }

    private void ApplyColors(Color bg, Color border, Color addr)
    {
        if (cellBackground != null) cellBackground.color = bg;
        if (borderGlow     != null) borderGlow.color     = border;
        if (addressText    != null) addressText.color    = addr;
        if (cornerPip      != null) cornerPip.color      = border;
    }

    private void ShowAddress(bool show)
    {
        if (addressText != null) addressText.gameObject.SetActive(show);
    }

    private void ShowChainNum(bool show)
    {
        if (chainNumberText != null) chainNumberText.gameObject.SetActive(show);
    }

    private void ShowTimerBar(bool show, float fillAmount = 1f)
    {
        if (overflowTimerBar == null) return;
        overflowTimerBar.gameObject.SetActive(show);
        if (show) overflowTimerBar.fillAmount = fillAmount;
    }

    private void SetInteractable(bool value)
    {
        if (_button != null) _button.interactable = value;
    }

    private void StopAllPulse()
    {
        _pulseT = 0f;
        if (borderGlow != null) borderGlow.color = normalBorder;
    }
}