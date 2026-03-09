using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Sahne geçişlerinde ve puzzle açılırken/kapanırken fade efekti.
///
/// KURULUM:
///   1. Sahneye bir Canvas ekle (Sort Order: 999 — her şeyin önünde)
///   2. Canvas'ın child'ına tam ekran siyah Image ekle
///   3. Bu script'i Canvas objesine ekle
///   4. Inspector'dan fadeImage alanına o Image'ı bağla
///   5. Image başlangıçta alpha:0 olsun (görünmez)
/// </summary>
public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [Header("Referans")]
    [SerializeField] private Image fadeImage;

    [Header("Ayarlar")]
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private Color fadeColor    = Color.black;

    private bool _busy = false;

    // ─────────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeImage != null)
        {
            Color c = fadeColor; c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);
            fadeImage.raycastTarget = true;   // fade sırasında tıklamaları blokla
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // PUBLIC API
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Ekranı karart, bitince callback çağır.</summary>
    public void FadeOut(Action onComplete = null)
    {
        if (_busy) return;
        StartCoroutine(FadeRoutine(0f, 1f, onComplete));
    }

    /// <summary>Ekranı aç.</summary>
    public void FadeIn(Action onComplete = null)
    {
        StartCoroutine(FadeRoutine(1f, 0f, onComplete));
    }

    /// <summary>FadeOut → callback → FadeIn zinciri — tek çağrıyla her ikisi.</summary>
    public void FadeOutIn(Action midCallback = null, Action onComplete = null)
    {
        if (_busy) return;
        StartCoroutine(FadeOutInRoutine(midCallback, onComplete));
    }

    /// <summary>FadeOut → LoadSceneAsync(sahne) → FadeIn. Sahne geçişi için.</summary>
    public void TransitionToScene(string sceneName, Action onComplete = null)
    {
        if (_busy) return;
        StartCoroutine(TransitionRoutine(sceneName, onComplete));
    }

    IEnumerator TransitionRoutine(string sceneName, Action onComplete)
    {
        yield return FadeRoutine(0f, 1f, null);
        var op = SceneManager.LoadSceneAsync(sceneName);
        if (op != null)
        {
            while (!op.isDone)
                yield return null;
        }
        else
            SceneManager.LoadScene(sceneName);
        yield return FadeRoutine(1f, 0f, null);
        onComplete?.Invoke();
    }

    // ─────────────────────────────────────────────────────────────────────
    IEnumerator FadeRoutine(float from, float to, Action onComplete)
    {
        _busy = true;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(from, to, t / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(to);

        _busy = false;
        onComplete?.Invoke();
    }

    IEnumerator FadeOutInRoutine(Action midCallback, Action onComplete)
    {
        yield return FadeRoutine(0f, 1f, null);
        midCallback?.Invoke();
        yield return FadeRoutine(1f, 0f, null);
        onComplete?.Invoke();
    }

    void SetAlpha(float a)
    {
        if (fadeImage == null) return;
        Color c = fadeColor; c.a = a;
        fadeImage.color = c;
    }
}
