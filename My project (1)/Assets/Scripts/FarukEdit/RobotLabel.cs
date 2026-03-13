using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Robotların üzerinde FarukEdit tarzı metin. Add Component → Robot Label ekle,
/// Cümleler listesine + ile istediğin kadar cümle ekle. Yanına gelip E ile sırayla gösterir.
/// </summary>
public class RobotLabel : MonoBehaviour
{
    [Header("Cümleler (E ile sırayla gösterilir — + ile ekle)")]
    [SerializeField] private List<string> lines = new List<string>();

    [Header("Etkileşim")]
    [SerializeField] private float interactionRadius = 4f;
    [SerializeField] private Transform player;

    /// <summary>0 = gizli, 1..N = o cümle gösteriliyor. Son cümleden sonra E = gizle ve IsDialogueFinished.</summary>
    private int _displayState;
    private bool _dialogueFinished;
    /// <summary>Boş olmayan cümle indeksleri; boş bırakılan satırlar atlanır.</summary>
    private List<int> _validLineIndices;

    [Header("Referanslar")]
    [SerializeField] private TMP_Text tmpText;
    [SerializeField] private TMP_FontAsset fontAsset;

    [Header("FarukEdit stili")]
    [SerializeField] private Color textColor = new Color(0.00f, 1.00f, 0.53f, 1.00f);
    [SerializeField] private float fontSize = 36f;
    [SerializeField] private float fontSizeMin = 18f;
    [SerializeField] private float fontSizeMax = 72f;
    [SerializeField] private bool autoSize = true;

    [Header("Runtime oluşturma (tmpText boşsa)")]
    [SerializeField] private float heightOffset = 1.8f;
    [SerializeField] private float scale = 0.01f;

    private GameObject _labelRoot;
    private float _nextFindPlayerTime;

    /// <summary>Tüm cümleler gösterilip E ile kapatıldıysa true. FinalTutorialRobot kaçışı buna göre başlatır.</summary>
    public bool IsDialogueFinished => _dialogueFinished;

    private void TryFindPlayer()
    {
        if (player != null) return;
        var p = GameObject.FindWithTag("Player");
        if (p != null) { player = p.transform; return; }
        var asil = FindObjectOfType<AsılScript>();
        if (asil != null) { player = asil.transform; return; }
        var female = FindObjectOfType<PlayerControllerFemale>();
        if (female != null) player = female.transform;
    }

    private void Start()
    {
        if (tmpText == null && fontAsset == null)
        {
            if (TMP_Settings.defaultFontAsset != null)
                fontAsset = TMP_Settings.defaultFontAsset;
            if (fontAsset == null)
                fontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }
        if (tmpText == null && fontAsset != null)
            CreateLabelAtRuntime();
        ApplyStyleToCurrent();
        _displayState = 0;
        _dialogueFinished = false;
        SetLabelVisible(false);
        BuildValidLineIndices();

        TryFindPlayer();
        _nextFindPlayerTime = Time.time + 1f;
    }

    private void Update()
    {
        if (player == null)
        {
            if (Time.time >= _nextFindPlayerTime)
            {
                TryFindPlayer();
                _nextFindPlayerTime = Time.time + 1f;
            }
            return;
        }
        if (tmpText == null)
        {
            if (fontAsset == null && TMP_Settings.defaultFontAsset != null)
            {
                fontAsset = TMP_Settings.defaultFontAsset;
                CreateLabelAtRuntime();
                ApplyStyleToCurrent();
                BuildValidLineIndices();
            }
            return;
        }
    }

    private void LateUpdate()
    {
        if (tmpText == null) return;

        // E ile diyalog (oyuncu menzildeyse)
        if (player != null && tmpText != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            bool inRange = dist <= interactionRadius;
            if (inRange && Input.GetKeyDown(KeyCode.E))
            {
                if (_validLineIndices == null || _validLineIndices.Count == 0)
                {
                    _dialogueFinished = true;
                }
                else
                {
                    _displayState++;
                    if (_displayState > _validLineIndices.Count)
                    {
                        _displayState = 0;
                        SetLabelVisible(false);
                        _dialogueFinished = true;
                    }
                    else
                    {
                        SetLabelVisible(true);
                        string show = lines[_validLineIndices[_displayState - 1]];
                        tmpText.text = string.IsNullOrEmpty(show) ? " " : show;
                        if (_displayState == _validLineIndices.Count)
                            _dialogueFinished = true;
                    }
                }
            }
        }

        // Robot sola baktığında scale (-1,1,1) oluyor; yazı ters dönmesin diye label scale'ini telafi et
        if (_labelRoot != null)
        {
            float s = scale;
            if (transform.localScale.x < 0f)
                s = -scale;
            _labelRoot.transform.localScale = new Vector3(s, scale, scale);
        }

        var canvas = tmpText.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null && Camera.main != null)
            canvas.worldCamera = Camera.main;
    }

    private void BuildValidLineIndices()
    {
        _validLineIndices = new List<int>();
        if (lines == null) return;
        for (int i = 0; i < lines.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(lines[i]))
                _validLineIndices.Add(i);
        }
    }

    private void SetLabelVisible(bool visible)
    {
        if (_labelRoot != null)
            _labelRoot.SetActive(visible);
        else if (tmpText != null)
            tmpText.gameObject.SetActive(visible);
    }

    private void CreateLabelAtRuntime()
    {
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
        tmpText.text = " ";
        tmpText.color = textColor;
        tmpText.fontSize = fontSize;
        tmpText.enableAutoSizing = autoSize;
        tmpText.fontSizeMin = fontSizeMin;
        tmpText.fontSizeMax = fontSizeMax;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.raycastTarget = false;

        _labelRoot = canvasGo;
    }

    private void ApplyStyleToCurrent()
    {
        if (tmpText == null) return;
        tmpText.color = textColor;
        tmpText.fontSize = fontSize;
        tmpText.enableAutoSizing = autoSize;
        tmpText.fontSizeMin = fontSizeMin;
        tmpText.fontSizeMax = fontSizeMax;
        if (fontAsset != null)
            tmpText.font = fontAsset;
    }
}
