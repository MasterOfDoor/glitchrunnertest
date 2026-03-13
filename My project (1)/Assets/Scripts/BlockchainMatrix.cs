using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockchainMatrix : MonoBehaviour
{
    public int     columnCount     = 20;
    public float   minSpeed        = 55f;
    public float   maxSpeed        = 145f;
    public float   blockSpacing    = 40f;
    public int     blocksPerColumn = 5;
    public Color   chainColor      = new Color(0f, 0.9f, 0.3f, 0.75f);
    public Color   fadeColor       = new Color(0f, 0.3f, 0.08f, 0.18f);
    public Color   headColor       = new Color(0.6f, 1f, 0.7f, 0.95f);
    public Vector2 blockSize       = new Vector2(30f, 30f);

    private static readonly string[] Syms =
    {
        "▣","▤","▥","▦","▧","▨","▩","◈","◇","◆",
        "⬡","⬢","⬦","▰","▱","⬛","⬜","⧫","◉","◎",
        "⊞","⊟","⊠","⊡","⬖","⬗","⬘","⬙","▲","▼","◀","▶","⬒","⬓","⬔","⬕",
    };

    private RectTransform  _canvasRT;
    private List<Col>      _cols = new List<Col>();
    private bool           _ready;

    void Start()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (!canvas) { Debug.LogError("[Matrix] Canvas bulunamadı!"); return; }
        _canvasRT = canvas.GetComponent<RectTransform>();
        _ready    = true;
        Spawn();
    }

    void Spawn()
    {
        float w = _canvasRT.rect.width, h = _canvasRT.rect.height;
        float cw = w / columnCount;
        for (int i = 0; i < columnCount; i++)
        {
            float x = -w / 2f + cw * i + cw / 2f;
            _cols.Add(new Col(this, i, x, Random.Range(-h/2f, h/2f),
                              Random.Range(minSpeed, maxSpeed), h));
        }
    }

    void Update() { if (_ready) foreach (var c in _cols) c.Tick(Time.deltaTime); }

    private class Col
    {
        readonly BlockchainMatrix _m;
        readonly float _x, _speed, _halfH;
        float _y;
        readonly List<RectTransform> _rts  = new List<RectTransform>();
        readonly List<Text>          _txts = new List<Text>();
        readonly List<Image>         _bgs  = new List<Image>();

        public Col(BlockchainMatrix m, int idx, float x, float startY, float speed, float h)
        {
            _m = m; _x = x; _y = startY; _speed = speed; _halfH = h / 2f;
            for (int b = 0; b < m.blocksPerColumn; b++)
            {
                var go = new GameObject($"M{idx}_{b}");
                go.transform.SetParent(m.transform, false);
                var rt = go.AddComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = m.blockSize;
                var bg = go.AddComponent<Image>();
                bg.color = new Color(0f, 0.05f, 0.01f, 0.7f);
                _rts.Add(rt); _bgs.Add(bg);

                var tgo = new GameObject("T");
                tgo.transform.SetParent(go.transform, false);
                var trt = tgo.AddComponent<RectTransform>();
                trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
                trt.offsetMin = trt.offsetMax = Vector2.zero;
                var txt = tgo.AddComponent<Text>();
                txt.alignment = TextAnchor.MiddleCenter;
                txt.fontSize  = 13; txt.fontStyle = FontStyle.Bold;
                txt.color     = m.chainColor; txt.text = R();
                _txts.Add(txt);

                if (b < m.blocksPerColumn - 1)
                {
                    var lg = new GameObject("L"); lg.transform.SetParent(go.transform, false);
                    var lrt = lg.AddComponent<RectTransform>();
                    float lh = m.blockSpacing - m.blockSize.y;
                    lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f, 0f);
                    lrt.pivot = new Vector2(0.5f, 1f);
                    lrt.sizeDelta = new Vector2(1.5f, lh);
                    lrt.anchoredPosition = Vector2.zero;
                    var li = lg.AddComponent<Image>();
                    li.color = new Color(m.chainColor.r, m.chainColor.g, m.chainColor.b, 0.22f);
                }
            }
        }

        public void Tick(float dt)
        {
            _y -= _speed * dt;
            float chainH = _m.blocksPerColumn * _m.blockSpacing;
            if (_y < -_halfH - chainH)
                _y = _halfH + _m.blockSize.y + Random.Range(0f, 50f);

            for (int b = 0; b < _rts.Count; b++)
            {
                float by = _y - b * _m.blockSpacing;
                _rts[b].anchoredPosition = new Vector2(_x, by);
                float t = 1f - (float)b / Mathf.Max(1, _rts.Count - 1);
                _txts[b].color = b == 0 ? _m.headColor : Color.Lerp(_m.fadeColor, _m.chainColor, t * t);
                _bgs[b].color  = new Color(0f, 0.05f, 0.01f, 0.65f * t + 0.05f);
                bool vis = by > -_halfH - _m.blockSize.y && by < _halfH + _m.blockSize.y;
                _rts[b].gameObject.SetActive(vis);
            }
            if (Random.value < 0.006f && _txts.Count > 1)
                _txts[Random.Range(1, _txts.Count)].text = R();
        }

        static string R() => Syms[Random.Range(0, Syms.Length)];
    }
}
