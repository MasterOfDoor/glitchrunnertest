using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Görsele uyum: siyah arka planda dağınık, yarı saydam yeşil geometrik semboller.
/// Canvas altında bir GameObject'e ekle; Start'ta sembolleri oluşturur.
/// </summary>
public class ScatteredMatrixSymbols : MonoBehaviour
{
    [Header("Sembol sayısı")]
    [SerializeField] int symbolCount = 120;

    [Header("Görünüm")]
    [SerializeField] float minOpacity = 0.15f;
    [SerializeField] float maxOpacity = 0.55f;
    [SerializeField] int fontSizeMin = 10;
    [SerializeField] int fontSizeMax = 18;

    [Header("Hareket (opsiyonel)")]
    [SerializeField] float pulseSpeed = 0.5f;
    [SerializeField] float pulseAmount = 0.15f;

    [Header("Font (boş bırakılırsa varsayılan kullanılır)")]
    [SerializeField] Font customFont;

    private static readonly string[] Syms =
    {
        "▣", "▤", "▥", "▦", "▧", "▨", "▩", "◈", "◇", "◆",
        "⬡", "⬢", "⬦", "▰", "▱", "⧫", "◉", "◎",
        "⊞", "⊟", "⊠", "⊡", "▲", "▼", "◀", "▶",
    };

    private RectTransform _rt;
    private List<Text> _texts = new List<Text>();
    private List<float> _baseAlpha = new List<float>();
    private List<float> _phase = new List<float>();

    void Start()
    {
        _rt = GetComponent<RectTransform>();
        if (!_rt) return;

        float w = _rt.rect.width * 0.5f;
        float h = _rt.rect.height * 0.5f;

        for (int i = 0; i < symbolCount; i++)
        {
            var go = new GameObject($"Sym_{i}");
            go.transform.SetParent(transform, false);

            var childRT = go.AddComponent<RectTransform>();
            childRT.anchorMin = new Vector2(0.5f, 0.5f);
            childRT.anchorMax = new Vector2(0.5f, 0.5f);
            childRT.pivot = new Vector2(0.5f, 0.5f);
            childRT.anchoredPosition = new Vector2(Random.Range(-w, w), Random.Range(-h, h));
            childRT.sizeDelta = new Vector2(28, 28);

            var txt = go.AddComponent<Text>();
            txt.text = Syms[Random.Range(0, Syms.Length)];
            txt.fontSize = Random.Range(fontSizeMin, fontSizeMax + 1);
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;
            if (customFont) txt.font = customFont;
            else
            {
                var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (font) txt.font = font;
            }

            float alpha = Mathf.Lerp(minOpacity, maxOpacity, Random.value);
            _baseAlpha.Add(alpha);
            _phase.Add(Random.Range(0f, Mathf.PI * 2f));
            txt.color = new Color(0f, 0.85f, 0.4f, alpha);
            _texts.Add(txt);
        }
    }

    void Update()
    {
        if (pulseAmount <= 0f || _texts.Count == 0) return;
        for (int i = 0; i < _texts.Count; i++)
        {
            float a = _baseAlpha[i] + pulseAmount * Mathf.Sin(Time.time * pulseSpeed + _phase[i]);
            a = Mathf.Clamp01(a);
            var c = _texts[i].color;
            _texts[i].color = new Color(c.r, c.g, c.b, a);
        }
    }
}
