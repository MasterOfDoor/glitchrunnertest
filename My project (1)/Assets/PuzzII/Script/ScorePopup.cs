using UnityEngine;
using TMPro;

/// <summary>
/// Attaches to the ScorePopup prefab. Floats upward and fades out, then destroys itself.
/// Uses unscaled time so it works correctly when Time.timeScale = 0 (during puzzle).
/// </summary>
public class ScorePopup : MonoBehaviour
{
    [SerializeField] private float floatSpeed   = 80f;
    [SerializeField] private float fadeDuration = 0.85f;

    private TextMeshProUGUI _tmp;
    private float           _timer      = 0f;
    private Color           _startColor = Color.white;
    private bool            _ready      = false;

    // ── Awake: cache the TMP reference ───────────────────────────────────
    private void Awake()
    {
        _tmp = GetComponent<TextMeshProUGUI>();

        // If TMP is on a child instead of root, find it
        if (_tmp == null)
            _tmp = GetComponentInChildren<TextMeshProUGUI>();
    }

    // ── Setup: called by RAMPuzzleManager right after Instantiate ────────
    public void Setup(string text, Color color)
    {
        // Awake may not have run yet if called the same frame as Instantiate,
        // so re-fetch here to be safe
        if (_tmp == null) _tmp = GetComponent<TextMeshProUGUI>();
        if (_tmp == null) _tmp = GetComponentInChildren<TextMeshProUGUI>();

        if (_tmp != null)
        {
            _tmp.text       = text;
            _tmp.color      = color;
        }

        _startColor = color;
        _timer      = 0f;
        _ready      = true;
    }

    // ── Update: float up and fade every frame ────────────────────────────
    private void Update()
    {
        if (!_ready) return;

        // unscaledDeltaTime: works even when Time.timeScale = 0
        _timer += Time.unscaledDeltaTime;

        // Move upward
        transform.position += Vector3.up * floatSpeed * Time.unscaledDeltaTime;

        // Fade alpha from 1 → 0
        float alpha = Mathf.Clamp01(1f - (_timer / fadeDuration));
        if (_tmp != null)
            _tmp.color = new Color(_startColor.r, _startColor.g, _startColor.b, alpha);

        // Destroy when animation is done
        if (_timer >= fadeDuration)
        {
            Destroy(gameObject);
        }
    }

    // ── Fallback safety: destroy after 3 seconds no matter what ─────────
    private void Start()
    {
        Destroy(gameObject, 3f);
    }
}