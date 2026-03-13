using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Matrix-style falling symbols behind the blockchain menu panel (matches blockchain_menu_preview.html canvas).
/// Attach to a RawImage; uses a Texture2D updated every frame.
/// </summary>
[RequireComponent(typeof(RawImage))]
public class MatrixRainEffect : MonoBehaviour
{
    [Header("Size")]
    [SerializeField] int textureWidth = 320;
    [SerializeField] int textureHeight = 180;

    [Header("Columns")]
    [SerializeField] int columnSpacing = 36;
    [SerializeField] float speedMin = 0.25f;
    [SerializeField] float speedMax = 0.65f;

    [Header("Fade")]
    [SerializeField] byte fadeAlpha = 18;

    private static readonly char[] Symbols = { '▣', '▤', '▥', '▦', '▧', '▨', '▩', '◈', '◇', '◆', '⬡', '⬢', '⬦', '▰', '▱', '⊞', '⊟', '⊠', '⊡', '⧫', '◉', '◎' };

    private Texture2D _tex;
    private RawImage _rawImage;
    private int _cols;
    private float[] _drops;
    private float[] _speeds;
    private Color32[] _pixelBuffer;
    private bool _initialized;

    void Awake()
    {
        _rawImage = GetComponent<RawImage>();
        if (!_rawImage) return;

        _tex = new Texture2D(textureWidth, textureHeight);
        _tex.filterMode = FilterMode.Point;
        _rawImage.texture = _tex;
        _rawImage.uvRect = new Rect(0, 0, 1, 1);

        _cols = Mathf.Max(1, textureWidth / columnSpacing);
        _drops = new float[_cols];
        _speeds = new float[_cols];
        _pixelBuffer = new Color32[textureWidth * textureHeight];

        for (int i = 0; i < _cols; i++)
        {
            _drops[i] = Random.Range(-40, 0) * 0.1f;
            _speeds[i] = speedMin + Random.value * (speedMax - speedMin);
        }

        _initialized = true;
    }

    void OnDestroy()
    {
        if (_tex) Destroy(_tex);
    }

    void Update()
    {
        if (!_initialized || _tex == null) return;

        // Fade trail
        for (int i = 0; i < _pixelBuffer.Length; i++)
        {
            Color32 c = _pixelBuffer[i];
            if (c.a > fadeAlpha)
                _pixelBuffer[i] = new Color32(c.r, c.g, c.b, (byte)Mathf.Max(0, c.a - fadeAlpha));
            else
                _pixelBuffer[i] = new Color32(0, 0, 0, 0);
        }

        int charHeight = Mathf.Max(1, textureHeight / 28);
        int charWidth = Mathf.Max(1, columnSpacing);

        for (int col = 0; col < _cols; col++)
        {
            int x = col * columnSpacing;
            if (x >= textureWidth) continue;

            float drop = _drops[col];
            int y = Mathf.RoundToInt(drop * charHeight);
            bool isHead = Random.value < 0.15f;

            byte r = (byte)(isHead ? 180 : 0);
            byte g = (byte)(isHead ? 255 : (160 + Random.Range(0, 80)));
            byte b = (byte)(isHead ? 200 : Random.Range(0, 30));
            byte a = (byte)(isHead ? 240 : (50 + Random.Range(0, 130)));

            for (int dy = 0; dy < charHeight && y + dy < textureHeight; dy++)
            {
                if (y + dy < 0) continue;
                int row = (textureHeight - 1) - (y + dy);
                for (int dx = 0; dx < charWidth && x + dx < textureWidth; dx++)
                {
                    int idx = row * textureWidth + (x + dx);
                    if (idx >= 0 && idx < _pixelBuffer.Length)
                        _pixelBuffer[idx] = new Color32(r, g, b, a);
                }
            }

            _drops[col] += _speeds[col];
            if (_drops[col] * charHeight > textureHeight && Random.value > 0.97f)
                _drops[col] = 0;
        }

        _tex.SetPixelData(_pixelBuffer, 0);
        _tex.Apply(false);
    }
}
