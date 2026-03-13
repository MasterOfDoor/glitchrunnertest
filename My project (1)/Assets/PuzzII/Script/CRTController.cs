using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Adds a CRT scanline + vignette overlay on top of the puzzle UI.
///
/// SETUP:
///   1. Place CRTOverlay.shader in Assets/Shaders/
///   2. Attach this script to any GameObject in the puzzle scene.
///   3. Assign puzzleCanvas (your PuzzleUI canvas).
///   4. Hit Play — the overlay builds itself automatically.
///
/// Tweak all values live in the Inspector during Play mode.
/// </summary>
public class CRTController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The Canvas that contains your puzzle UI")]
    [SerializeField] private Canvas puzzleCanvas;

    [Header("Scanlines")]
    [SerializeField] private float scanlineSpacing   = 4f;    // pixels between lines (HTML: 4px)
    [SerializeField][Range(0f, 1f)]
    private float scanlineDarkness  = 0.18f;                  // HTML: rgba(0,0,0,0.18)
    [SerializeField][Range(0.1f, 1f)]
    private float scanlineSharpness = 0.55f;

    [Header("Vignette")]
    [SerializeField][Range(0f, 1f)]
    private float vignetteRadius    = 0.50f;                  // HTML: ellipse 85% starts ~0.42
    [SerializeField][Range(0f, 1f)]
    private float vignetteStrength  = 0.75f;                  // HTML: rgba(0,0,0,0.75)
    [SerializeField][Range(0.01f, 1f)]
    private float vignetteSoftness  = 0.40f;

    [Header("Flicker (subtle)")]
    [SerializeField][Range(0f, 0.04f)]
    private float flickerStrength   = 0.012f;
    [SerializeField]
    private float flickerSpeed      = 8f;

    [Header("Screen Curvature")]
    [SerializeField][Range(0f, 0.12f)]
    private float curvatureX        = 0.03f;
    [SerializeField][Range(0f, 0.12f)]
    private float curvatureY        = 0.02f;

    [Header("Overall Strength")]
    [SerializeField][Range(0f, 1f)]
    private float overlayAlpha      = 1f;

    // ── Private ──────────────────────────────────────────────────────────
    private GameObject  _overlayGO;
    private Image       _overlayImage;
    private Material    _crtMaterial;

    // ─────────────────────────────────────────────────────────────────────
    // LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (puzzleCanvas == null)
            puzzleCanvas = FindObjectOfType<Canvas>();

        BuildOverlay();
    }

    private void Update()
    {
        PushProperties();
    }

    private void OnDestroy()
    {
        if (_crtMaterial != null) Destroy(_crtMaterial);
        if (_overlayGO   != null) Destroy(_overlayGO);
    }

    // ─────────────────────────────────────────────────────────────────────
    // BUILD
    // ─────────────────────────────────────────────────────────────────────
    private void BuildOverlay()
    {
        // Find shader
        Shader shader = Shader.Find("Custom/CRTOverlay");
        if (shader == null)
        {
            Debug.LogError("[CRTController] Could not find shader 'Custom/CRTOverlay'. " +
                           "Make sure CRTOverlay.shader is in your Assets folder.");
            return;
        }

        // Create material
        _crtMaterial       = new Material(shader);
        _crtMaterial.name  = "CRTOverlay_Runtime";

        // Create full-screen UI Image as the top-most child of the canvas
        _overlayGO = new GameObject("CRTOverlay");
        _overlayGO.transform.SetParent(puzzleCanvas.transform, false);
        _overlayGO.transform.SetAsLastSibling();   // must be on top of everything

        _overlayImage               = _overlayGO.AddComponent<Image>();
        _overlayImage.material      = _crtMaterial;
        _overlayImage.color         = new Color(0f, 0f, 0f, overlayAlpha);
        _overlayImage.raycastTarget = false;        // never block clicks

        // Stretch to fill the entire canvas
        var rt          = _overlayGO.GetComponent<RectTransform>();
        rt.anchorMin    = Vector2.zero;
        rt.anchorMax    = Vector2.one;
        rt.offsetMin    = Vector2.zero;
        rt.offsetMax    = Vector2.zero;

        PushProperties();
    }

    // ─────────────────────────────────────────────────────────────────────
    // PUSH PROPERTIES TO SHADER EVERY FRAME
    // ─────────────────────────────────────────────────────────────────────
    private void PushProperties()
    {
        if (_crtMaterial == null) return;

        _crtMaterial.SetFloat("_ScanlineSpacing",   scanlineSpacing);
        _crtMaterial.SetFloat("_ScanlineDarkness",  scanlineDarkness);
        _crtMaterial.SetFloat("_ScanlineSharp",     scanlineSharpness);

        _crtMaterial.SetFloat("_VignetteRadius",    vignetteRadius);
        _crtMaterial.SetFloat("_VignetteStrength",  vignetteStrength);
        _crtMaterial.SetFloat("_VignetteSoftness",  vignetteSoftness);

        _crtMaterial.SetFloat("_FlickerStrength",   flickerStrength);
        _crtMaterial.SetFloat("_FlickerSpeed",      flickerSpeed);

        _crtMaterial.SetFloat("_CurvatureX",        curvatureX);
        _crtMaterial.SetFloat("_CurvatureY",        curvatureY);

        if (_overlayImage != null)
            _overlayImage.color = new Color(0f, 0f, 0f, overlayAlpha);
    }

    // ─────────────────────────────────────────────────────────────────────
    // PUBLIC — toggle on/off from other scripts if needed
    // ─────────────────────────────────────────────────────────────────────
    public void SetEnabled(bool value)
    {
        if (_overlayGO != null) _overlayGO.SetActive(value);
    }

    public void SetStrength(float alpha)
    {
        overlayAlpha = Mathf.Clamp01(alpha);
    }
}
