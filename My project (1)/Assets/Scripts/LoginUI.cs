using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Giriş ekranı: Cüzdanla Bağlan + Sosyal medya placeholder; giriş sonrası GameState + sahne geçişi.
/// Cüzdan oluşturma için link (placeholder).
/// </summary>
public class LoginUI : MonoBehaviour
{
    static readonly Color MatrixGreen = new Color(0f, 1f, 0.25f, 1f);
    static readonly Color MatrixGreenDim = new Color(0f, 0.4f, 0.1f, 0.9f);
    static readonly Color PanelBg = new Color(0.02f, 0.06f, 0.02f, 0.96f);

    const int RefWidth = 640;
    const int RefHeight = 360;

    [Tooltip("Giriş sonrası yüklenecek sahne adı.")]
    public string sceneAfterLogin = "Cpu giriş";

    [Tooltip("Cüzdan oluştur linki (placeholder).")]
    public string createWalletUrl = "https://metamask.io/download/";

    GameObject canvasObj;
    GameObject panel;

    void Start()
    {
        if (GameState.Instance != null && GameState.Instance.IsLoggedIn)
        {
            GoToNextScene();
            return;
        }
        BuildUI();
    }

    void BuildUI()
    {
        EnsureEventSystem();

        canvasObj = new GameObject("LoginCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(RefWidth, RefHeight);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        panel = new GameObject("LoginPanel");
        panel.transform.SetParent(canvasObj.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(320, 260);
        panelRect.anchoredPosition = Vector2.zero;
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = PanelBg;
        var outline = panel.AddComponent<Outline>();
        outline.effectColor = MatrixGreen;
        outline.effectDistance = new Vector2(2, 2);

        // Başlık
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1);
        titleRect.anchorMax = new Vector2(0.5f, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -20);
        titleRect.sizeDelta = new Vector2(280, 36);
        var titleText = titleObj.AddComponent<Text>();
        titleText.text = "GİRİŞ";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 24;
        titleText.color = MatrixGreen;
        titleText.alignment = TextAnchor.MiddleCenter;

        // Cüzdanla Bağlan
        var walletBtn = CreateButton("Cüzdanla Bağlan", -20, () => OnWalletConnect());
        walletBtn.transform.SetParent(panel.transform, false);

        // Sosyal medya ile giriş (placeholder)
        var socialBtn = CreateButton("Sosyal medya / E-posta ile giriş", -80, () => OnSocialLogin());
        socialBtn.transform.SetParent(panel.transform, false);

        // Cüzdan oluştur linki
        var linkObj = new GameObject("CreateWalletLink");
        linkObj.transform.SetParent(panel.transform, false);
        var linkRect = linkObj.AddComponent<RectTransform>();
        linkRect.anchorMin = new Vector2(0.5f, 0);
        linkRect.anchorMax = new Vector2(0.5f, 0);
        linkRect.pivot = new Vector2(0.5f, 0);
        linkRect.anchoredPosition = new Vector2(0, 30);
        linkRect.sizeDelta = new Vector2(260, 28);
        var linkBtn = linkObj.AddComponent<Button>();
        var linkImg = linkObj.AddComponent<Image>();
        linkImg.color = new Color(0, 0, 0, 0.01f);
        linkBtn.onClick.AddListener(OnCreateWalletLink);
        var linkTextObj = new GameObject("Text");
        linkTextObj.transform.SetParent(linkObj.transform, false);
        var linkTextRect = linkTextObj.AddComponent<RectTransform>();
        linkTextRect.anchorMin = Vector2.zero;
        linkTextRect.anchorMax = Vector2.one;
        linkTextRect.offsetMin = Vector2.zero;
        linkTextRect.offsetMax = Vector2.zero;
        var linkText = linkTextObj.AddComponent<Text>();
        linkText.text = "Cüzdan oluştur";
        linkText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        linkText.fontSize = 14;
        linkText.color = MatrixGreenDim;
        linkText.alignment = TextAnchor.MiddleCenter;
    }

    GameObject CreateButton(string label, float y, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject("Button_" + label);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, y);
        rect.sizeDelta = new Vector2(240, 44);
        var img = go.AddComponent<Image>();
        img.color = MatrixGreenDim;
        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(onClick);
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(go.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var text = textObj.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 16;
        text.color = MatrixGreen;
        text.alignment = TextAnchor.MiddleCenter;
        return go;
    }

    void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    void OnWalletConnect()
    {
        // Placeholder: test adresi ile giriş. Gerçek entegrasyonda WalletConnect/MetaMask ile adres alınır.
        if (GameState.Instance != null)
            GameState.Instance.SetWalletAddress("0xPlaceholderWalletAddress");
        GoToNextScene();
    }

    void OnSocialLogin()
    {
        // Placeholder: sosyal giriş sonrası isteğe bağlı cüzdan oluşturma sunulacak.
        if (GameState.Instance != null)
            GameState.Instance.SetWalletAddress(""); // Sosyal girişte cüzdan boş; oyuncu sonra "Cüzdanla Bağlan" yapabilir
        GoToNextScene();
    }

    void OnCreateWalletLink()
    {
        if (!string.IsNullOrEmpty(createWalletUrl))
            Application.OpenURL(createWalletUrl);
    }

    void GoToNextScene()
    {
        if (SceneFader.Instance != null)
            SceneFader.Instance.TransitionToScene(sceneAfterLogin);
        else
            SceneManager.LoadScene(sceneAfterLogin);
    }
}
