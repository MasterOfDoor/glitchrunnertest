using System.Collections;
using UnityEngine;
using TMPro;

/// Sadece TMP Text objesine ekle.
/// Trigger'a değince glitch + fade ile yok olur.
public class AccessPanel : MonoBehaviour
{
    [Header("Referans")]
    [SerializeField] private TMP_Text label;

    [Header("Renkler")]
    [SerializeField] private Color colorA = new Color(0f, 1f, 0.22f);   // #00FF38 neon yeşil
    [SerializeField] private Color colorB = new Color(1f, 0.06f, 0.25f); // #FF0F40 kırmızı

    [Header("Nabız")]
    [SerializeField] private float pulseSpeed = 3f;   // parlama hızı
    [SerializeField] private float pulseMin   = 0.3f; // en karanlık nokta
    [SerializeField] private float pulseMax   = 1f;   // en parlak nokta

    [Header("Efekt")]
    [SerializeField] private float glitchDuration = 0.5f;
    [SerializeField] private float fadeDuration   = 0.35f;

    static readonly string[] GlitchPool =
    {
        "// access denied",
        "// 4cc3ss d3n13d",
        "// @cc#ss d%n!ed",
        "// █ccess den█ed",
        "// ̷a̷c̷c̷e̷s̷s̷ ̷d̷e̷n̷i̷e̷d̷",
        "// acce$$ derr0r",
    };

    bool _done;
    float _t;

    void Awake()
    {
        if (label == null) label = GetComponent<TMP_Text>();
        if (label == null) label = GetComponentInChildren<TMP_Text>();
        if (label == null)
        {
            Debug.LogError("[AccessPanel] TMP_Text bulunamadı! Label alanını Inspector'dan bağla.");
            return;
        }
        label.text  = "// access denied";
        label.color = colorA;
    }

    // sürekli parlayıp sönme
    void Update()
    {
        if (_done || label == null) return;
        _t += Time.deltaTime * pulseSpeed;
        float b = Mathf.Lerp(pulseMin, pulseMax, (Mathf.Sin(_t) + 1f) * 0.5f);
        Color c = colorA;
        c.r *= b; c.g *= b; c.b *= b; c.a = 1f;
        label.color = c;
    }

    public void Dismiss()
    {
        if (_done) return;
        _done = true;
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        // 1 — glitch
        float t = 0f;
        while (t < glitchDuration)
        {
            label.text  = GlitchPool[Random.Range(0, GlitchPool.Length)];
            label.color = Random.value > 0.5f ? colorA : colorB;

            // yatay kayma
            label.transform.localPosition = new Vector3(
                Random.Range(-6f, 6f), Random.Range(-2f, 2f), 0f);

            t += Time.deltaTime;
            yield return new WaitForSeconds(0.035f);
        }

        label.transform.localPosition = Vector3.zero;

        // 2 — fade out
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            Color c = label.color;
            c.a = 1f - t;
            label.color = c;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}