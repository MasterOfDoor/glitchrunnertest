using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Reown.AppKit.Unity;

/// <summary>
/// WalletModalUI.cs
/// Reown AppKit'in kendi modal'ı yerine kendi distopik UI'ımızı kullanır.
/// Kullanıcının yüklü wallet'larını listeler, logolar + isimlerle gösterir.
/// Google giriş butonu en üstte sabit durur.
/// Konum: Assets/Scripts/WalletModalUI.cs
/// </summary>
public class WalletModalUI : MonoBehaviour
{
    [Header("Modal Kök")]
    public CanvasGroup modalGroup;       // Modal'ın tüm CanvasGroup'u
    public RectTransform modalPanel;     // Ana panel RectTransform

    [Header("Google Butonu")]
    public Button      googleButton;
    public TextMeshProUGUI googleLabel;

    [Header("Wallet Listesi")]
    public RectTransform  walletListContent;  // ScrollRect > Viewport > Content
    public GameObject     walletRowPrefab;    // Wallet satır prefab'ı

    [Header("Status")]
    public TextMeshProUGUI statusText;

    [Header("Kapat Butonu")]
    public Button closeButton;

    [Header("Sahne Geçişi")]
    [Tooltip("Bağlantı sonrası gidilecek sahne adı (Build Settings'te olmalı)")]
    public string targetSceneName = "MainMenu";
    [Tooltip("Geçiş öncesi FadeCanvas'ın CanvasGroup'u")]
    public CanvasGroup fadeGroup;
    [Tooltip("Geçiş süresi (saniye)")]
    public float fadeDuration = 1.0f;

    // Renk sabitleri — parlak ve net
    private static readonly Color CIdle  = new Color(0f, 1f, 0.35f, 1.00f);
    private static readonly Color CWarn  = new Color(1f, 0.90f, 0f, 1.00f);
    private static readonly Color CGreen = new Color(0f, 1f, 0.40f, 1.00f);
    private static readonly Color CDim   = new Color(0f, 0.80f, 0.25f, 0.90f);

    // Desteklenen wallet listesi — Reown SDK'nın algıladığı isimlerle eşleşmeli
    private static readonly WalletInfo[] KnownWallets =
    {
        new WalletInfo("MetaMask",        "metamask"),
        new WalletInfo("WalletConnect",   "walletconnect"),
        new WalletInfo("Coinbase Wallet", "coinbase"),
        new WalletInfo("Trust Wallet",    "trust"),
        new WalletInfo("Phantom",         "phantom"),
        new WalletInfo("Rainbow",         "rainbow"),
        new WalletInfo("Ledger",          "ledger"),
        new WalletInfo("Trezor",          "trezor"),
    };

    private bool _open   = false;
    private bool _busy   = false;
    private readonly List<GameObject> _rows = new List<GameObject>();

    void Awake()
    {
        if (modalGroup)
        {
            modalGroup.alpha          = 0f;
            modalGroup.interactable   = false;
            modalGroup.blocksRaycasts = false;
        }

        if (closeButton) closeButton.onClick.AddListener(CloseModal);
        if (googleButton) googleButton.onClick.AddListener(OnGoogleClick);
        if (statusText) statusText.text = "";
    }

    void Start()
    {
        if (AppKit.IsInitialized)
        {
            AppKit.AccountConnected    += OnReownConnected;
            AppKit.AccountDisconnected += OnReownDisconnected;
        }
    }

    void OnDestroy()
    {
        try
        {
            if (AppKit.IsInitialized)
            {
                AppKit.AccountConnected    -= OnReownConnected;
                AppKit.AccountDisconnected -= OnReownDisconnected;
            }
        }
        catch (System.Exception)
        {
            // AppKit zaten kaldırılmış olabilir (örn. Play mode bitince)
        }
    }

    void OnReownConnected(object sender, Connector.AccountConnectedEventArgs e)
    {
        string addr = e != null ? (e.Account.Address ?? "") : "";
        string shortAddr = addr.Length > 10
            ? $"{addr.Substring(0, 6)}...{addr.Substring(addr.Length - 4)}"
            : addr;

        SetStatus($"> ACCESS GRANTED // {shortAddr}");
        SetStatusColor(new Color(0f, 1f, 0.35f, 1f));
        _busy = false;

        StartCoroutine(SuccessAndLoad());
    }

    void OnReownDisconnected(object sender, Connector.AccountDisconnectedEventArgs e)
    {
        SetStatus("> DISCONNECTED");
        _busy = false;
    }

    IEnumerator SuccessAndLoad()
    {
        yield return new WaitForSeconds(1.2f);

        if (fadeGroup != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t = Mathf.Min(t + Time.deltaTime / fadeDuration, 1f);
                fadeGroup.alpha = t;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        if (!string.IsNullOrEmpty(targetSceneName))
            SceneManager.LoadScene(targetSceneName);
        else
            Debug.LogWarning("[WalletModal] targetSceneName boş! Inspector'dan sahne adını gir.");
    }

    IEnumerator DelayedClose(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseModal();
    }

    // ── Dışarıdan çağrılır (BootPanelController'dan) ─────────────────────
    public void OpenModal()
    {
        if (_open) return;
        _open = true;
        BuildWalletList();
        StartCoroutine(FadeIn());
    }

    public void CloseModal()
    {
        if (!_open) return;
        _open = false;
        StartCoroutine(FadeOut());
    }

    // ── Wallet listesini oluştur ──────────────────────────────────────────
    void BuildWalletList()
    {
        // Eski satırları temizle
        foreach (var r in _rows) Destroy(r);
        _rows.Clear();

        // Reown'dan mevcut wallet'ları al
        // NOT: Bu kısım Cursor tarafından Reown SDK'ya göre doldurulacak
        // Şimdilik KnownWallets listesini filtrele (SDK entegrasyonu sonrası güncellenir)
        var available = GetAvailableWallets();

        SetStatus($"> {available.Count} WALLET PROVIDER DETECTED");

        foreach (var w in available)
        {
            var row = Instantiate(walletRowPrefab, walletListContent);
            _rows.Add(row);

            var label = row.GetComponentInChildren<TextMeshProUGUI>();
            if (label) { label.text = w.DisplayName.ToUpper(); label.color = CIdle; }

            // Logo sprite — Resources/WalletLogos/{w.Id} adında yükle
            var img = row.transform.Find("Logo")?.GetComponent<Image>();
            if (img)
            {
                var sprite = Resources.Load<Sprite>($"WalletLogos/{w.Id}");
                if (sprite) img.sprite = sprite;
                else img.color = new Color(0f, 1f, 0.3f, 0.3f); // bulunamazsa yeşil placeholder
            }

            // Tıklama — walletId'yi capture et
            string id = w.Id;
            var btn = row.GetComponent<Button>();
            if (btn) btn.onClick.AddListener(() => OnWalletClick(id, w.DisplayName));

            // Hover efekti
            AddHoverEffect(row);
        }

        // İçerik yüksekliğini ayarla (scroll için)
        var layout = walletListContent.GetComponent<VerticalLayoutGroup>();
        if (!layout)
        {
            layout = walletListContent.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing          = 4f;
            layout.childForceExpandWidth  = true;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(0, 0, 0, 0);
        }
        var fitter = walletListContent.GetComponent<ContentSizeFitter>();
        if (!fitter)
        {
            fitter = walletListContent.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }

    List<WalletInfo> GetAvailableWallets()
    {
        // Bu SDK sürümünde ConnectorController.AvailableWallets yok; bilinen liste kullanılıyor
        return new List<WalletInfo>(KnownWallets);
    }

    // ── Google ile giriş ─────────────────────────────────────────────────
    void OnGoogleClick()
    {
        if (_busy) return;
        StartCoroutine(ConnectSequence("google", "GOOGLE"));
    }

    // ── Wallet tıklama ────────────────────────────────────────────────────
    void OnWalletClick(string walletId, string displayName)
    {
        if (_busy) return;
        StartCoroutine(ConnectSequence(walletId, displayName.ToUpper()));
    }

    IEnumerator ConnectSequence(string walletId, string displayName)
    {
        _busy = true;
        SetStatus($"> CONNECTING TO {displayName}...");
        SetStatusColor(CWarn);
        yield return new WaitForSeconds(0.3f);

        if (!AppKit.IsInitialized)
        {
            SetStatus("> APP NOT READY — Start from main menu to enable wallet.");
            SetStatusColor(new Color(1f, 0.5f, 0f, 0.9f));
            _busy = false;
            yield break;
        }

        bool connectError = false;
        if (walletId == "google")
        {
            SetStatus("> OPENING GOOGLE AUTH...");
            yield return new WaitForSeconds(0.2f);
            try { SocialLogin.Google.Open(); }
            catch (System.Exception e)
            {
                Debug.LogError($"[WalletModal] Google connect hatası: {e.Message}");
                connectError = true;
            }
        }
        else
        {
            SetStatus($"> AWAITING {displayName} SIGNATURE...");
            yield return new WaitForSeconds(0.2f);
            try { AppKit.OpenModal(); }
            catch (System.Exception e)
            {
                Debug.LogError($"[WalletModal] Wallet connect hatası: {e.Message}");
                connectError = true;
            }
        }

        if (connectError)
        {
            yield return StartCoroutine(ShowConnectError());
            yield break;
        }
        // Başarı OnReownConnected event'i ile gelir
    }

    IEnumerator ShowConnectError()
    {
        SetStatus("> ERROR: CONNECTION FAILED");
        SetStatusColor(new Color(1f, 0.1f, 0.05f, 0.9f));
        yield return new WaitForSeconds(2f);
        SetStatus("");
        _busy = false;
    }

    // ── Fade animasyonları ────────────────────────────────────────────────
    IEnumerator FadeIn()
    {
        modalGroup.interactable   = true;
        modalGroup.blocksRaycasts = true;
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime * 5f, 1f);
            modalGroup.alpha = t;
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        modalGroup.interactable   = false;
        modalGroup.blocksRaycasts = false;
        float t = 1f;
        while (t > 0f)
        {
            t = Mathf.Max(t - Time.deltaTime * 5f, 0f);
            modalGroup.alpha = t;
            yield return null;
        }
    }

    // ── Hover efekti ─────────────────────────────────────────────────────
    void AddHoverEffect(GameObject row)
    {
        var trigger = row.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (!trigger) trigger = row.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry
            { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener(_ =>
        {
            var img = row.GetComponent<Image>();
            if (img) img.color = new Color(0f, 1f, 0.3f, 0.07f);
            var outline = row.GetComponent<Outline>();
            if (outline) outline.effectColor = new Color(0f, 1f, 0.3f, 0.6f);
        });

        var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry
            { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        exitEntry.callback.AddListener(_ =>
        {
            var img = row.GetComponent<Image>();
            if (img) img.color = new Color(0f, 0f, 0f, 0f);
            var outline = row.GetComponent<Outline>();
            if (outline) outline.effectColor = new Color(0f, 1f, 0.3f, 0.25f);
        });

        trigger.triggers.Add(enterEntry);
        trigger.triggers.Add(exitEntry);
    }

    void SetStatus(string msg)      { if (statusText) statusText.text  = msg; }
    void SetStatusColor(Color col)  { if (statusText) statusText.color = col; }

    static TMP_FontAsset LoadMonoFont()
    {
        var font = Resources.Load<TMP_FontAsset>("Fonts/ShareTechMono-Regular SDF");
        if (font != null) return font;
        return TMP_Settings.defaultFontAsset;
    }

    // ── Wallet bilgi yapısı ───────────────────────────────────────────────
    [System.Serializable]
    public class WalletInfo
    {
        public string DisplayName;
        public string Id;
        public WalletInfo(string displayName, string id) { DisplayName = displayName; Id = id; }
    }
}
