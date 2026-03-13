using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Konuşma balonu scripti.
///
/// ÖNEMLİ — Bu script BubbleCanvas üzerinde durmalı (her zaman aktif).
/// BubbleRoot child'dır ve başta inactive olabilir — sorun yok.
///
/// PREFAB YAPISI:
///   NPC
///   └── BubbleCanvas  ← Bu script BURAYA eklenir (her zaman aktif)
///       └── BubbleRoot  ← başta inactive, script açar/kapatır
///           ├── BubbleText  (TMP)
///           └── CursorDot   (Image)
/// </summary>
public class DialogueBubble : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private GameObject bubbleRoot;
    [SerializeField] private TMP_Text   bubbleText;
    [SerializeField] private Image      cursorDot;
    [SerializeField] private Image      bubbleBg;

    [Header("Yazma Efekti")]
    [SerializeField] private float typeSpeed      = 28f;
    [SerializeField] private float lingerDuration = 3.2f;
    [SerializeField] private float fadeSpeed      = 4f;

    [Header("Görünüm")]
    [SerializeField] private Color bgColor    = new Color(0.02f, 0.10f, 0.07f, 0.92f);
    [SerializeField] private Color textColor  = new Color(0.00f, 1.00f, 0.53f, 1.00f);
    [SerializeField] private Color cursorColor = new Color(0.00f, 1.00f, 0.53f, 1.00f);

    private Coroutine _activeRoutine;
    private bool      _visible;

    // ─────────────────────────────────────────────────────────────────────
    void Awake()
    {
        ApplyColors();
        // BubbleRoot'u kapat — bu script BubbleCanvas'ta, sorun yok
        if (bubbleRoot != null) bubbleRoot.SetActive(false);
        _visible = false;
    }

    // ─────────────────────────────────────────────────────────────────────
    // PUBLIC API
    // ─────────────────────────────────────────────────────────────────────
    public void ShowLine(string line, float? customLinger = null)
    {
        // Bu script BubbleCanvas'ta (aktif) — StartCoroutine güvenli
        if (_activeRoutine != null) StopCoroutine(_activeRoutine);
        _activeRoutine = StartCoroutine(TypeRoutine(line, customLinger ?? lingerDuration));
    }

    public void Hide()
    {
        if (_activeRoutine != null) { StopCoroutine(_activeRoutine); _activeRoutine = null; }
        if (bubbleRoot != null) bubbleRoot.SetActive(false);
        _visible = false;
    }

    /// <summary>Metni anında gösterir (yazma efekti yok). E ile ilerleme için — Hide() ile kapat.</summary>
    public void ShowLineInstant(string line)
    {
        if (_activeRoutine != null) { StopCoroutine(_activeRoutine); _activeRoutine = null; }
        if (bubbleText != null) bubbleText.text = line ?? "";
        if (bubbleRoot != null) bubbleRoot.SetActive(true);
        _visible = true;
    }

    public bool IsVisible => _visible;

    // ─────────────────────────────────────────────────────────────────────
    // COROUTINE
    // ─────────────────────────────────────────────────────────────────────
    private IEnumerator TypeRoutine(string line, float linger)
    {
        _visible = true;

        // BubbleRoot'u aç (bu script onun üzerinde değil — güvenli)
        if (bubbleRoot != null) bubbleRoot.SetActive(true);

        SetAlpha(0f);
        yield return null;

        // Fade in
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha = Mathf.MoveTowards(alpha, 1f, fadeSpeed * Time.unscaledDeltaTime);
            SetAlpha(alpha);
            yield return null;
        }

        // Typewriter
        if (bubbleText != null) bubbleText.text = "";
        if (cursorDot  != null) cursorDot.gameObject.SetActive(true);

        float interval = 1f / typeSpeed;
        float acc      = 0f;

        for (int i = 0; i <= line.Length; i++)
        {
            if (bubbleText != null) bubbleText.text = line.Substring(0, i);

            if (cursorDot != null)
            {
                Color c = cursorColor;
                c.a = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 12f);
                cursorDot.color = c;
            }

            acc += interval;
            while (acc > 0f) { acc -= Time.unscaledDeltaTime; yield return null; }
        }

        if (cursorDot != null) cursorDot.gameObject.SetActive(false);

        // Linger
        float t = 0f;
        while (t < linger) { t += Time.unscaledDeltaTime; yield return null; }

        // Fade out
        alpha = 1f;
        while (alpha > 0f)
        {
            alpha = Mathf.MoveTowards(alpha, 0f, fadeSpeed * Time.unscaledDeltaTime);
            SetAlpha(alpha);
            yield return null;
        }

        if (bubbleRoot != null) bubbleRoot.SetActive(false);
        _visible       = false;
        _activeRoutine = null;
    }

    // ─────────────────────────────────────────────────────────────────────
    void SetAlpha(float a)
    {
        if (bubbleBg != null)
        {
            Color c = bubbleBg.color; c.a = bgColor.a * a; bubbleBg.color = c;
        }
        if (bubbleText != null)
        {
            Color c = bubbleText.color; c.a = a; bubbleText.color = c;
        }
    }

    void ApplyColors()
    {
        if (bubbleBg   != null) bubbleBg.color  = bgColor;
        if (bubbleText != null) bubbleText.color = textColor;
        if (cursorDot  != null) cursorDot.color  = cursorColor;
    }
}