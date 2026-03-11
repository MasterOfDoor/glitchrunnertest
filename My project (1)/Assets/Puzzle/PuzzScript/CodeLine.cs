using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Sürüklenebilir kod satırı. PuzzleManager tarafından Setup() ile başlatılır.
/// </summary>
public class CodeLine : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    public string text { get; private set; }

    // ── Görsel renkler ────────────────────────────────────────────────────
    private static readonly Color BgNormal  = new Color(0.03f, 0.12f, 0.09f, 0.95f);
    private static readonly Color BgHover   = new Color(0.00f, 0.25f, 0.16f, 0.95f);
    private static readonly Color BgDrag    = new Color(0.00f, 0.40f, 0.26f, 0.95f);
    private static readonly Color TextColor = new Color(0.00f, 1.00f, 0.53f, 1.00f);
    private static readonly Color BorderIdle = new Color(0.00f, 0.40f, 0.26f, 1.00f);
    private static readonly Color BorderDrag = new Color(0.00f, 1.00f, 0.53f, 1.00f);

    // ── References ────────────────────────────────────────────────────────
    private PuzzleManager  _manager;
    private RectTransform  _rect;
    private Canvas         _canvas;
    private Image          _bg;
    private Image          _border;
    private TMP_Text       _label;

    // ── Drag state ────────────────────────────────────────────────────────
    private Transform _originalParent;
    private Vector2   _originalPos;
    private int       _originalSiblingIndex;
    private Vector3   _dragOffset;

    // ─────────────────────────────────────────────────────────────────────
    public void Setup(string lineText, PuzzleManager manager, TMP_FontAsset fontOverride = null)
    {
        text     = lineText;
        _manager = manager;
        _rect    = GetComponent<RectTransform>();

        _canvas = GetComponentInParent<Canvas>();
        Transform t = transform;
        while (_canvas == null && t.parent != null)
        {
            t      = t.parent;
            _canvas = t.GetComponent<Canvas>();
        }

        _bg     = GetComponent<Image>();
        _border = transform.Find("Border")?.GetComponent<Image>();
        _label  = GetComponentInChildren<TMP_Text>();

        // Font override — runtime'da prefab fontunu değiştir
        if (_label != null && fontOverride != null)
            _label.font = fontOverride;

        if (_bg     != null) _bg.color     = BgNormal;
        if (_border != null) _border.color = BorderIdle;
        if (_label  != null)
        {
            _label.text  = lineText;
            _label.color = TextColor;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // HOVER
    // ─────────────────────────────────────────────────────────────────────
    public void OnPointerEnter(PointerEventData _)
    {
        if (_bg != null) _bg.color = BgHover;
    }

    public void OnPointerExit(PointerEventData _)
    {
        if (_bg != null) _bg.color = BgNormal;
    }

    // ─────────────────────────────────────────────────────────────────────
    // DRAG
    // ─────────────────────────────────────────────────────────────────────
    public void OnBeginDrag(PointerEventData eventData)
    {
        _originalParent       = transform.parent;
        _originalPos          = _rect.anchoredPosition;
        _originalSiblingIndex = transform.GetSiblingIndex();

        // Canvas'ın üst katmanına taşı — diğer UI'ların üstünde görünsün
        transform.SetParent(_canvas.transform, true);
        transform.SetAsLastSibling();

        if (_bg     != null) _bg.color     = BgDrag;
        if (_border != null) _border.color = BorderDrag;

        // Fare ile obje merkezi arasındaki offset'i hesapla
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        _dragOffset = _rect.localPosition - (Vector3)localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_canvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pos);

        _rect.localPosition = (Vector3)pos + _dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_bg     != null) _bg.color     = BgNormal;
        if (_border != null) _border.color = BorderIdle;

        // Hedef slot'ları tara
        foreach (Transform slot in _manager.targetArea)
        {
            RectTransform slotRect = slot as RectTransform;

            if (!RectTransformUtility.RectangleContainsScreenPoint(
                slotRect, eventData.position, eventData.pressEventCamera))
                continue;

            if (slot.childCount == 0)
            {
                // Boş slot — yerleştir
                SnapToSlot(slot);
            }
            else
            {
                // Dolu slot — yer değiştir
                Transform other = slot.GetChild(0);
                CodeLine  otherCode = other.GetComponent<CodeLine>();

                // Diğerini eski yerimize gönder
                if (_originalParent != null && _originalParent != _manager.targetArea &&
                    IsSlot(_originalParent))
                {
                    // Orijinalimiz bir slot ise diğerini oraya koy
                    otherCode?.SnapToSlot(_originalParent);
                }
                else
                {
                    // Orijinalimiz serbest alanda ise diğerini codeContainer'a bırak
                    other.SetParent(_manager.codeContainer, true);
                    CodeLine ol = other.GetComponent<CodeLine>();
                    if (ol != null)
                        (other as RectTransform).anchoredPosition = _originalPos;
                }

                SnapToSlot(slot);
            }

            _manager.CheckTargetSlots();
            return;
        }

        // Hiçbir slota düşmediyse orijinal konuma dön
        ReturnToOrigin();
    }

    // ─────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────
    public void SnapToSlot(Transform slot)
    {
        transform.SetParent(slot, false);
        _rect.anchoredPosition = Vector2.zero;
        _rect.anchorMin        = new Vector2(0.5f, 0.5f);
        _rect.anchorMax        = new Vector2(0.5f, 0.5f);
        _rect.pivot            = new Vector2(0.5f, 0.5f);
    }

    void ReturnToOrigin()
    {
        if (_originalParent == null || _originalParent == _canvas.transform)
        {
            transform.SetParent(_manager.codeContainer, false);
            _rect.anchoredPosition = _originalPos;
        }
        else
        {
            transform.SetParent(_originalParent, false);
            if (IsSlot(_originalParent))
            {
                _rect.anchoredPosition = Vector2.zero;
            }
            else
            {
                transform.SetSiblingIndex(_originalSiblingIndex);
                _rect.anchoredPosition = _originalPos;
            }
        }
    }

    bool IsSlot(Transform t) => t != null && t.parent == _manager.targetArea;
}