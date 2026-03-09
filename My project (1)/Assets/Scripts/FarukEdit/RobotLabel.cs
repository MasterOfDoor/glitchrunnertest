using UnityEngine;
using TMPro;

/// <summary>
/// Robotların üzerinde FarukEdit tarzı (PressStart2P yeşil) metin göstermek için.
/// Ya Inspector'dan bir TMP_Text atayın ya da boş bırakıp Font'u atayın — runtime'da label oluşturulur.
/// </summary>
public class RobotLabel : MonoBehaviour
{
    [Header("Metin")]
    [Tooltip("Gösterilecek yazı. Boş bırakırsanız sadece stil uygulanır.")]
    [TextArea(1, 3)]
    public string labelText = "Robot";

    [Header("Etkileşim (robotun yanına gel + E = cümleler)")]
    [Tooltip("İkinci cümle (E'ye 2. kez basınca).")]
    [TextArea(1, 3)]
    [SerializeField] private string secondLine = "";
    [Tooltip("Oyuncu bu mesafeden yakınsa E ile etkileşim kurulur.")]
    [SerializeField] private float interactionRadius = 3f;
    [Tooltip("Boş bırakırsan 'Player' tag'li obje aranır.")]
    [SerializeField] private Transform player;

    /// <summary>0 = gizli, 1 = ilk cümle, 2 = ikinci cümle. E ile döngü.</summary>
    private int _displayState;

    [Header("Referanslar")]
    [Tooltip("Zaten bir TMP_Text varsa buraya sürükleyin. Yoksa boş bırakın, Font atayın — runtime'da oluşturulur.")]
    [SerializeField] private TMP_Text tmpText;

    [Tooltip("FarukEdit tarzı için: Assets/Puzzle/PuzzFont/Press_Start_2P/PressStart2P-Regular SDF Cyan")]
    [SerializeField] private TMP_FontAsset fontAsset;

    [Header("FarukEdit stili (InteractPrompt / DialogueBubble ile aynı)")]
    [SerializeField] private Color textColor = new Color(0.00f, 1.00f, 0.53f, 1.00f);
    [SerializeField] private float fontSize = 36f;
    [SerializeField] private float fontSizeMin = 18f;
    [SerializeField] private float fontSizeMax = 72f;
    [SerializeField] private bool autoSize = true;

    [Header("Runtime oluşturma (tmpText boşsa kullanılır)")]
    [SerializeField] private float heightOffset = 1.8f;
    [SerializeField] private float scale = 0.01f;

    /// <summary>Runtime'da oluşturulan canvas (gizleyince tüm label kaybolur).</summary>
    private GameObject _labelRoot;

    private void Start()
    {
        if (tmpText == null && fontAsset != null)
            CreateLabelAtRuntime();
        ApplyStyle();
        _displayState = 0; // Başta gizli — E'ye basana kadar hiçbir cümle gözükmesin
        SetLabelVisible(false);

        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
            if (player == null)
            {
                var asil = FindObjectOfType<AsılScript>();
                if (asil != null) player = asil.transform;
            }
        }
    }

    private void Update()
    {
        if (tmpText == null || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        bool inRange = dist <= interactionRadius;

        if (inRange && Input.GetKeyDown(KeyCode.E))
        {
            _displayState = (_displayState + 1) % 3; // 0→1→2→0→...
            if (_displayState == 0)
            {
                SetLabelVisible(false);
            }
            else
            {
                SetLabelVisible(true);
                string show = _displayState == 1 ? labelText : (secondLine ?? "");
                tmpText.text = string.IsNullOrEmpty(show) ? " " : show;
            }
        }
    }

    private void SetLabelVisible(bool visible)
    {
        if (_labelRoot != null)
            _labelRoot.SetActive(visible);
        else if (tmpText != null)
            tmpText.gameObject.SetActive(visible);
    }

    private void LateUpdate()
    {
        if (tmpText == null) return;
        var canvas = tmpText.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null && Camera.main != null)
            canvas.worldCamera = Camera.main;
    }

    private void CreateLabelAtRuntime()
    {
        // World Space Canvas — robotun üstünde sabit kalır
        var canvasGo = new GameObject("RobotLabelCanvas");
        canvasGo.transform.SetParent(transform);
        canvasGo.transform.localPosition = new Vector3(0f, heightOffset, 0f);
        canvasGo.transform.localRotation = Quaternion.identity;
        canvasGo.transform.localScale = Vector3.one * scale;

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        var scaler = canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100f;
        scaler.referencePixelsPerUnit = 100f;
        canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        var textGo = new GameObject("LabelText");
        textGo.transform.SetParent(canvasGo.transform, false);

        var rect = textGo.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(400f, 80f);

        tmpText = textGo.AddComponent<TextMeshProUGUI>();
        tmpText.font = fontAsset;
        tmpText.text = labelText;
        tmpText.color = textColor;
        tmpText.fontSize = fontSize;
        tmpText.enableAutoSizing = autoSize;
        tmpText.fontSizeMin = fontSizeMin;
        tmpText.fontSizeMax = fontSizeMax;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.raycastTarget = false;

        _labelRoot = canvasGo;
    }

    private void ApplyStyle()
    {
        if (tmpText == null) return;

        tmpText.text = string.IsNullOrEmpty(labelText) ? " " : labelText;
        tmpText.color = textColor;
        tmpText.fontSize = fontSize;
        tmpText.enableAutoSizing = autoSize;
        tmpText.fontSizeMin = fontSizeMin;
        tmpText.fontSizeMax = fontSizeMax;
        if (fontAsset != null)
            tmpText.font = fontAsset;
    }

    /// <summary>Runtime'da metni değiştir.</summary>
    public void SetText(string text)
    {
        labelText = text ?? "";
        if (tmpText != null) tmpText.text = labelText;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (tmpText != null && Application.isPlaying)
            ApplyStyle();
    }
#endif
}
