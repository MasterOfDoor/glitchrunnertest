using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Basit market: Gun ve Spear, Satın Al, M ile aç/kapa.
/// Canvas GameState altında oluşturulur; sahne değişse bile referans kaybolmaz.
/// </summary>
public class MarketUI : MonoBehaviour
{
    public static MarketUI Instance { get; private set; }

    GameObject panel;
    Text balanceLabel;
    Font font;

    const int Padding = 12;
    const int RowHeight = 40;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Show()
    {
        if (panel == null) BuildPanel();
        if (panel != null)
        {
            panel.SetActive(true);
            if (AvalancheWallet.Instance != null)
                AvalancheWallet.Instance.SyncCoinFromWallet();
            RefreshCoin();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
        // İmleci burada zorlamıyoruz; oyun kendi ayarlarını kullanır
    }

    public bool IsVisible => panel != null && panel.activeSelf;

    void Update()
    {
        // M veya Escape ile kapat; M burada da olsun ki kapanma garanti çalışsın
        if (panel != null && panel.activeSelf && (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Escape)))
            Hide();
    }

    void BuildPanel()
    {
        GameObject canvasObj = new GameObject("MarketCanvas");
        canvasObj.transform.SetParent(transform, false);

        Canvas c = canvasObj.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 101;
        CanvasScaler cs = canvasObj.AddComponent<CanvasScaler>();
        cs.referenceResolution = new Vector2(640, 360);
        cs.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        panel = new GameObject("MarketPanel");
        panel.transform.SetParent(canvasObj.transform, false);

        RectTransform pr = panel.AddComponent<RectTransform>();
        pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(280, 220);
        pr.anchoredPosition = Vector2.zero;
        Image pImg = panel.AddComponent<Image>();
        pImg.color = new Color(0.06f, 0.14f, 0.06f, 0.98f);

        float y = 90f;

        Text title = CreateText(panel.transform, "MARKET", 20, new Color(0, 1f, 0.3f), 0, y);
        title.alignment = TextAnchor.MiddleLeft;
        RectTransform titleR = title.GetComponent<RectTransform>();
        titleR.anchorMin = new Vector2(0, 1);
        titleR.anchorMax = new Vector2(1, 1);
        titleR.pivot = new Vector2(0.5f, 1);
        titleR.anchoredPosition = new Vector2(0, -Padding);
        titleR.sizeDelta = new Vector2(-Padding * 2, 26);

        // Coin yazısı: sağ üstte ama kapat butonunun solunda kalacak (gizlenmesin)
        GameObject coinObj = new GameObject("Coin");
        coinObj.transform.SetParent(panel.transform, false);
        RectTransform coinR = coinObj.AddComponent<RectTransform>();
        coinR.anchorMin = new Vector2(1, 1);
        coinR.anchorMax = new Vector2(1, 1);
        coinR.pivot = new Vector2(1, 1);
        coinR.anchoredPosition = new Vector2(-48, -Padding); // Kapat butonundan (32+8) solunda
        coinR.sizeDelta = new Vector2(110, 26);
        balanceLabel = coinObj.AddComponent<Text>();
        balanceLabel.font = font;
        balanceLabel.fontSize = 14;
        balanceLabel.color = new Color(0, 0.8f, 0.25f);
        balanceLabel.alignment = TextAnchor.MiddleRight;

        y = 70f;
        AddRow("Gun", "gun", 50m, ref y);
        AddRow("Spear", "spear", 30m, ref y);

        // Kapat butonu: en sağ köşede, coin yazısının sağında
        GameObject closeBtn = new GameObject("CloseBtn");
        closeBtn.transform.SetParent(panel.transform, false);
        RectTransform closeR = closeBtn.AddComponent<RectTransform>();
        closeR.anchorMin = new Vector2(1, 1);
        closeR.anchorMax = new Vector2(1, 1);
        closeR.pivot = new Vector2(1, 1);
        closeR.anchoredPosition = new Vector2(-Padding, -Padding);
        closeR.sizeDelta = new Vector2(28, 28);
        Image closeImg = closeBtn.AddComponent<Image>();
        closeImg.color = new Color(0, 0.5f, 0.15f, 1f);
        Button closeB = closeBtn.AddComponent<Button>();
        closeB.onClick.AddListener(Hide);
        Text closeTxt = CreateText(closeBtn.transform, "X", 18, new Color(0, 1f, 0.3f), 0, 0);
        closeTxt.alignment = TextAnchor.MiddleCenter;
        RectTransform closeTxtR = closeTxt.GetComponent<RectTransform>();
        closeTxtR.anchorMin = Vector2.zero;
        closeTxtR.anchorMax = Vector2.one;
        closeTxtR.offsetMin = closeTxtR.offsetMax = Vector2.zero;

        RefreshCoin();
        panel.SetActive(false);
    }

    void OnEnable()
    {
        if (GameState.Instance != null)
            GameState.Instance.OnCoinChanged += RefreshCoin;
    }

    void OnDisable()
    {
        if (GameState.Instance != null)
            GameState.Instance.OnCoinChanged -= RefreshCoin;
    }

    Text CreateText(Transform parent, string text, int fontSize, Color color, float x, float y)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(200, 30);
        Text t = go.AddComponent<Text>();
        t.text = text;
        t.font = font;
        t.fontSize = fontSize;
        t.color = color;
        return t;
    }

    void AddRow(string label, string itemId, decimal price, ref float y)
    {
        GameObject row = new GameObject("Row_" + itemId);
        row.transform.SetParent(panel.transform, false);
        RectTransform rowR = row.AddComponent<RectTransform>();
        rowR.anchorMin = new Vector2(0, 1);
        rowR.anchorMax = new Vector2(1, 1);
        rowR.pivot = new Vector2(0.5f, 1);
        rowR.anchoredPosition = new Vector2(0, -y);
        rowR.sizeDelta = new Vector2(-Padding * 2, RowHeight);

        Image rowBg = row.AddComponent<Image>();
        rowBg.color = new Color(0.08f, 0.18f, 0.08f, 0.95f);

        Text labelTxt = CreateText(row.transform, label + " - " + price + " coin", 14, new Color(0, 1f, 0.35f), -60, 0);
        labelTxt.alignment = TextAnchor.MiddleLeft;
        RectTransform labelR = labelTxt.GetComponent<RectTransform>();
        labelR.anchorMin = new Vector2(0, 0);
        labelR.anchorMax = new Vector2(1, 1);
        labelR.offsetMin = new Vector2(Padding, 4);
        labelR.offsetMax = new Vector2(-90, -4);

        GameObject btn = new GameObject("Buy");
        btn.transform.SetParent(row.transform, false);
        RectTransform btnR = btn.AddComponent<RectTransform>();
        btnR.anchorMin = new Vector2(1, 0.5f);
        btnR.anchorMax = new Vector2(1, 0.5f);
        btnR.pivot = new Vector2(1, 0.5f);
        btnR.anchoredPosition = new Vector2(-Padding, 0);
        btnR.sizeDelta = new Vector2(70, 28);
        btn.AddComponent<Image>().color = new Color(0, 0.5f, 0.2f, 1f);
        Button b = btn.AddComponent<Button>();
        decimal p = price;
        string id = itemId;
        b.onClick.AddListener(() => Buy(id, p));

        Text btnTxt = CreateText(btn.transform, "Satın Al", 12, new Color(0, 1f, 0.3f), 0, 0);
        btnTxt.alignment = TextAnchor.MiddleCenter;
        RectTransform btnTxtR = btnTxt.GetComponent<RectTransform>();
        btnTxtR.anchorMin = Vector2.zero;
        btnTxtR.anchorMax = Vector2.one;
        btnTxtR.offsetMin = btnTxtR.offsetMax = Vector2.zero;

        y += RowHeight + 6;
    }

    void RefreshCoin()
    {
        if (balanceLabel != null && GameState.Instance != null)
            balanceLabel.text = "Coin: " + GameState.Instance.CoinBalance.ToString("F0");
    }

    void Buy(string itemId, decimal price)
    {
        if (GameState.Instance == null) return;
        if (GameState.Instance.CoinBalance < price) return;

        bool walletConnected = AvalancheWallet.Instance != null && GameState.Instance.IsLoggedIn;

        if (walletConnected)
        {
            // Gerçek cüzdan: önce dağıtıcıya transfer, başarılıysa item ver
            AvalancheWallet.Instance.TransferToDistributorAsync(price, success =>
            {
                if (success && GameState.Instance.SpendCoins(price))
                {
                    if (!GameState.Instance.AddItemToInventory(itemId, null, 1))
                        GameState.Instance.AddCoins(price);
                }
            });
        }
        else
        {
            // Cüzdan yok: yerel coin ile satın al (test)
            if (!GameState.Instance.SpendCoins(price)) return;
            if (!GameState.Instance.AddItemToInventory(itemId, null, 1))
                GameState.Instance.AddCoins(price);
        }
    }
}
