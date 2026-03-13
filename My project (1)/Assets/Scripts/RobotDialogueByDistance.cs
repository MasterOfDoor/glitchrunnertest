using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Sadece mesafe + E ile konuşma. Cpubazaar robotlarına ekle; öğretici sahne RobotLabel kullanmaya devam etsin.
/// Inspector'da Etkileşim Mesafesi ve Cümleler listesini doldur.
/// </summary>
public class RobotDialogueByDistance : MonoBehaviour
{
    [Header("Mesafe ile etkileşim")]
    [Tooltip("Oyuncu bu mesafeden yakınsa E ile konuşulur.")]
    [SerializeField] private float interactionDistance = 4f;

    [Header("Cümleler (E ile sırayla — + ile ekle)")]
    [SerializeField] private List<string> lines = new List<string>();

    [Header("Oyuncu (boş bırakırsan otomatik aranır)")]
    [SerializeField] private Transform player;

    [Header("Yazı (isteğe bağlı)")]
    [SerializeField] private TMP_FontAsset fontAsset;
    [SerializeField] private float labelHeightOffset = 1.8f;
    [SerializeField] private float labelScale = 0.01f;

    private Transform _player;
    private TMP_Text _tmpText;
    private GameObject _labelRoot;
    private List<int> _validIndices;
    private int _index = -1;

    void Start()
    {
        TryFindPlayer();
        _validIndices = new List<int>();
        if (lines != null)
        {
            for (int i = 0; i < lines.Count; i++)
                if (!string.IsNullOrWhiteSpace(lines[i]))
                    _validIndices.Add(i);
        }
        EnsureLabel();
        SetVisible(false);
    }

    void Update()
    {
        if (_player == null)
        {
            TryFindPlayer();
            return;
        }

        float dist = Vector3.Distance(transform.position, _player.position);
        bool inRange = dist <= interactionDistance;

        if (inRange && Input.GetKeyDown(KeyCode.E))
        {
            if (_validIndices == null || _validIndices.Count == 0)
                return;

            _index++;
            if (_index >= _validIndices.Count)
            {
                _index = -1;
                SetVisible(false);
                return;
            }

            SetVisible(true);
            if (_tmpText != null)
                _tmpText.text = lines[_validIndices[_index]];
        }
    }

    void TryFindPlayer()
    {
        if (player != null) { _player = player; return; }
        var p = GameObject.FindWithTag("Player");
        if (p != null) { _player = p.transform; return; }
        var asil = FindObjectOfType<AsılScript>();
        if (asil != null) { _player = asil.transform; return; }
        var female = FindObjectOfType<PlayerControllerFemale>();
        if (female != null) _player = female.transform;
    }

    void EnsureLabel()
    {
        if (_tmpText != null) return;
        if (fontAsset == null && TMP_Settings.defaultFontAsset != null)
            fontAsset = TMP_Settings.defaultFontAsset;
        if (fontAsset == null)
            fontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (fontAsset == null) return;

        var canvasGo = new GameObject("RobotDialogueByDistance_Canvas");
        canvasGo.transform.SetParent(transform);
        canvasGo.transform.localPosition = new Vector3(0f, labelHeightOffset, 0f);
        canvasGo.transform.localRotation = Quaternion.identity;
        canvasGo.transform.localScale = Vector3.one * labelScale;

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>().dynamicPixelsPerUnit = 100f;
        canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        var textGo = new GameObject("LabelText");
        textGo.transform.SetParent(canvasGo.transform, false);
        var rect = textGo.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(400f, 80f);

        _tmpText = textGo.AddComponent<TextMeshProUGUI>();
        _tmpText.font = fontAsset;
        _tmpText.text = " ";
        _tmpText.fontSize = 36f;
        _tmpText.alignment = TextAlignmentOptions.Center;
        _tmpText.raycastTarget = false;
        _tmpText.color = new Color(0f, 1f, 0.53f, 1f);

        _labelRoot = canvasGo;
    }

    void SetVisible(bool visible)
    {
        if (_labelRoot != null)
            _labelRoot.SetActive(visible);
        else if (_tmpText != null)
            _tmpText.gameObject.SetActive(visible);
    }
}
