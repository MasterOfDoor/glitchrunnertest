using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Reown.AppKit.Unity;

/// <summary>
/// WalletModalUI.cs — WebGL uyumlu, derleme hataları giderildi
///
/// DÜZELTMELER:
///   - ViewType.SocialLogin yok → AppKit.OpenModal() kullanılıyor
///   - ConnectorController.Connectors yok → GetAvailableWallets() sadece KnownWallets döndürüyor
///   - Google WebGL: SocialLogin.Google.Open() → AppKit.OpenModal() ile Reown kendi UI'ında Google sunuyor
/// </summary>
public class WalletModalUI : MonoBehaviour
{
    [Header("Modal Kök")]
    public CanvasGroup    modalGroup;
    public RectTransform  modalPanel;

    [Header("Google Butonu")]
    public Button          googleButton;
    public TextMeshProUGUI googleLabel;

    [Header("Wallet Listesi")]
    public RectTransform walletListContent;
    public GameObject    walletRowPrefab;

    [Header("Status")]
    public TextMeshProUGUI statusText;

    [Header("Kapat Butonu")]
    public Button closeButton;

    [Header("Sahne Geçişi")]
    public string      targetSceneName = "MainMenu";
    public CanvasGroup fadeGroup;
    public float       fadeDuration    = 1.0f;

    private static readonly Color CIdle  = new Color(0f, 1f,    0.35f, 1.00f);
    private static readonly Color CWarn  = new Color(1f, 0.90f, 0f,    1.00f);
    private static readonly Color CGreen = new Color(0f, 1f,    0.40f, 1.00f);
    private static readonly Color CError = new Color(1f, 0.10f, 0.05f, 1.00f);

    // WalletConnect her zaman başta — WebGL'de QR ile çalışır
    private static readonly WalletInfo[] KnownWallets =
    {
        new WalletInfo("WalletConnect",   "walletconnect"),
        new WalletInfo("MetaMask",        "metamask"),
        new WalletInfo("Coinbase Wallet", "coinbase"),
        new WalletInfo("Trust Wallet",    "trust"),
        new WalletInfo("Phantom",         "phantom"),
        new WalletInfo("Rainbow",         "rainbow"),
    };

    private bool _open = false;
    private bool _busy = false;
    private readonly List<GameObject> _rows = new List<GameObject>();

    // ── Lifecycle ─────────────────────────────────────────────────────────
    void Awake()
    {
        if (modalGroup)
        {
            modalGroup.alpha          = 0f;
            modalGroup.interactable   = false;
            modalGroup.blocksRaycasts = false;
        }
        if (closeButton)  closeButton.onClick.AddListener(CloseModal);
        if (googleButton) googleButton.onClick.AddListener(OnGoogleClick);
        if (statusText)   statusText.text = "";
    }

    void Start()
    {
        TrySubscribeAppKit();
    }

    void TrySubscribeAppKit()
    {
        try
        {
            if (AppKit.IsInitialized)
            {
                AppKit.AccountConnected    += OnReownConnected;
                AppKit.AccountDisconnected += OnReownDisconnected;
            }
            else
            {
                StartCoroutine(RetrySubscribe());
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[WalletModal] AppKit subscribe: {ex.Message}");
            StartCoroutine(RetrySubscribe());
        }
    }

    IEnumerator RetrySubscribe()
    {
        yield return new WaitForSeconds(1.5f);
        TrySubscribeAppKit();
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
        catch { }
    }

    // ── Reown Events ─────────────────────────────────────────────────────
    void OnReownConnected(object sender, Connector.AccountConnectedEventArgs e)
    {
        string addr = e != null ? (e.Account.Address ?? "") : "";
        string shortAddr = addr.Length > 10
            ? $"{addr.Substring(0, 6)}...{addr.Substring(addr.Length - 4)}"
            : addr;
        SetStatus($"> ACCESS GRANTED // {shortAddr}");
        SetStatusColor(CGreen);
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
            Debug.LogWarning("[WalletModal] targetSceneName boş!");
    }

    // ── Modal Aç / Kapat ─────────────────────────────────────────────────
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

    // ── Wallet Listesi ───────────────────────────────────────────────────
    void BuildWalletList()
    {
        foreach (var r in _rows) Destroy(r);
        _rows.Clear();

        var wallets = GetAvailableWallets();
        SetStatus($"> {wallets.Count} WALLET PROVIDER DETECTED");

        foreach (var w in wallets)
        {
            if (walletRowPrefab == null || walletListContent == null) break;
            var row = Instantiate(walletRowPrefab, walletListContent);
            _rows.Add(row);

            var label = row.GetComponentInChildren<TextMeshProUGUI>();
            if (label) { label.text = w.DisplayName.ToUpper(); label.color = CIdle; }

            var img = row.transform.Find("Logo")?.GetComponent<Image>();
            if (img)
            {
                var sprite = Resources.Load<Sprite>($"WalletLogos/{w.Id}");
                if (sprite) img.sprite = sprite;
                else        img.color  = new Color(0f, 1f, 0.3f, 0.3f);
            }

            string capturedId   = w.Id;
            string capturedName = w.DisplayName;
            var btn = row.GetComponent<Button>();
            if (btn) btn.onClick.AddListener(() => OnWalletClick(capturedId, capturedName));
            AddHoverEffect(row);
        }

        EnsureLayout();
    }

    List<WalletInfo> GetAvailableWallets()
    {
        // ConnectorController.Connectors bu SDK sürümünde yok.
        // KnownWallets listesi kullanılıyor — WebGL'de her wallet AppKit.OpenModal() ile açılır.
        return new List<WalletInfo>(KnownWallets);
    }

    void EnsureLayout()
    {
        if (!walletListContent) return;
        var layout = walletListContent.GetComponent<VerticalLayoutGroup>();
        if (!layout)
        {
            layout = walletListContent.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing               = 4f;
            layout.childForceExpandWidth  = true;
            layout.childForceExpandHeight = false;
        }
        var fitter = walletListContent.GetComponent<ContentSizeFitter>();
        if (!fitter)
        {
            fitter = walletListContent.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }

    // ── Google Butonu ─────────────────────────────────────────────────────
    // WebGL'de SocialLogin.Google.Open() browser popup bloğuna takılır.
    // Çözüm: AppKit.OpenModal() açılır, Reown kendi UI'ında Google seçeneğini
    // otomatik gösterir. Kullanıcı oradan Google'ı seçer.
    void OnGoogleClick()
    {
        if (_busy) return;
        StartCoroutine(ConnectGoogle());
    }

    IEnumerator ConnectGoogle()
    {
        _busy = true;
        SetStatus("> OPENING GOOGLE AUTH...");
        SetStatusColor(CWarn);
        yield return new WaitForSeconds(0.2f);

        if (!AppKit.IsInitialized)
        {
            SetStatus("> SYSTEM NOT READY — TRY AGAIN IN A MOMENT");
            SetStatusColor(CError);
            yield return new WaitForSeconds(2f);
            SetStatus("");
            _busy = false;
            yield break;
        }

        bool err = false;
        try
        {
            // WebGL'de en güvenli yol: normal modal açılır.
            // Reown'un WebGL UI'ında Google seçeneği otomatik çıkar.
            AppKit.OpenModal();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WalletModal] Google modal hatası: {ex.Message}");
            err = true;
        }

        if (err)
        {
            SetStatus("> ERROR: GOOGLE AUTH UNAVAILABLE");
            SetStatusColor(CError);
            yield return new WaitForSeconds(2f);
            SetStatus("");
            _busy = false;
        }
        // Başarı → OnReownConnected event'i
    }

    // ── Wallet Tıklama ────────────────────────────────────────────────────
    void OnWalletClick(string walletId, string displayName)
    {
        if (_busy) return;
        StartCoroutine(ConnectWallet(walletId, displayName));
    }

    IEnumerator ConnectWallet(string walletId, string displayName)
    {
        _busy = true;
        SetStatus($"> CONNECTING TO {displayName.ToUpper()}...");
        SetStatusColor(CWarn);
        yield return new WaitForSeconds(0.3f);

        if (!AppKit.IsInitialized)
        {
            SetStatus("> SYSTEM NOT READY — TRY AGAIN IN A MOMENT");
            SetStatusColor(CError);
            yield return new WaitForSeconds(2f);
            SetStatus("");
            _busy = false;
            yield break;
        }

        SetStatus($"> AWAITING {displayName.ToUpper()} SIGNATURE...");

        bool err = false;
        try
        {
            // WebGL'de tüm wallet'lar için AppKit.OpenModal() kullanılır.
            // Reown kendi QR/deep-link akışını yönetir.
            AppKit.OpenModal();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WalletModal] Wallet connect hatası: {ex.Message}");
            err = true;
        }

        if (err)
        {
            SetStatus("> ERROR: CONNECTION FAILED");
            SetStatusColor(CError);
            yield return new WaitForSeconds(2f);
            SetStatus("");
            _busy = false;
        }
        // Başarı → OnReownConnected event'i
    }

    // ── Fade ─────────────────────────────────────────────────────────────
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

    // ── Hover ─────────────────────────────────────────────────────────────
    void AddHoverEffect(GameObject row)
    {
        var trigger = row.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (!trigger) trigger = row.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        var enter = new UnityEngine.EventSystems.EventTrigger.Entry
            { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ =>
        {
            var img = row.GetComponent<Image>();
            var ol  = row.GetComponent<Outline>();
            if (img) img.color      = new Color(0f, 1f, 0.3f, 0.07f);
            if (ol)  ol.effectColor = new Color(0f, 1f, 0.3f, 0.6f);
        });

        var exit = new UnityEngine.EventSystems.EventTrigger.Entry
            { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        exit.callback.AddListener(_ =>
        {
            var img = row.GetComponent<Image>();
            var ol  = row.GetComponent<Outline>();
            if (img) img.color      = new Color(0f, 0f, 0f, 0f);
            if (ol)  ol.effectColor = new Color(0f, 1f, 0.3f, 0.25f);
        });

        trigger.triggers.Add(enter);
        trigger.triggers.Add(exit);
    }

    void SetStatus(string msg)     { if (statusText) statusText.text  = msg; }
    void SetStatusColor(Color col) { if (statusText) statusText.color = col; }

    [System.Serializable]
    public class WalletInfo
    {
        public string DisplayName;
        public string Id;
        public WalletInfo(string d, string i) { DisplayName = d; Id = i; }
    }
}
