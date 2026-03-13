using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// PromptAnchor üzerindeki piksel ekran efekti.
/// Konum/hareket işi PromptBob.cs'te — bu script sadece görsel efekt yapar.
/// </summary>
public class InteractPrompt : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Image    screenBg;
    [SerializeField] private Image    screenBorder;

    [Header("Renkler")]
    [SerializeField] private Color bgColor     = new Color(0.01f, 0.05f, 0.02f, 1.00f);
    [SerializeField] private Color borderColor = new Color(0.00f, 0.85f, 0.40f, 1.00f);
    [SerializeField] private Color textColor   = new Color(0.00f, 1.00f, 0.53f, 1.00f);

    [Header("Pulse")]
    [SerializeField] private float pulseSpeed = 2.4f;
    [SerializeField] private float pulseMin   = 0.45f;
    [SerializeField] private float pulseMax   = 1.00f;

    [Header("Flicker")]
    [SerializeField] private float flickerSpeed    = 7f;
    [SerializeField] private float flickerStrength = 0.07f;

    void Awake()
    {
        if (screenBg     != null) screenBg.color     = bgColor;
        if (screenBorder != null) screenBorder.color = borderColor;
        if (promptText   != null) promptText.color   = textColor;
    }

    void Update()
    {
        PulseText();
        PulseBorder();
        FlickerBg();
    }

    void PulseText()
    {
        if (promptText == null) return;
        float a = Mathf.Lerp(pulseMin, pulseMax,
                  0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * pulseSpeed));
        Color c = textColor; c.a = a;
        promptText.color = c;
    }

    void PulseBorder()
    {
        if (screenBorder == null) return;
        float a = Mathf.Lerp(0.5f, 1.0f,
                  0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * pulseSpeed * 0.65f));
        Color c = borderColor; c.a = a;
        screenBorder.color = c;
    }

    void FlickerBg()
    {
        if (screenBg == null) return;
        float n = Mathf.Sin(Time.unscaledTime * flickerSpeed)
                * Mathf.Sin(Time.unscaledTime * flickerSpeed * 2.37f);
        float b = 1f + n * flickerStrength;
        Color c = bgColor;
        c.r = Mathf.Clamp01(c.r * b);
        c.g = Mathf.Clamp01(c.g * b);
        c.b = Mathf.Clamp01(c.b * b);
        screenBg.color = c;
    }
}