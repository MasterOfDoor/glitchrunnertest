#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using TMPro;

/// <summary>
/// WalletModalBuilder.cs
/// Tools > GLITCHRUNNER > Build Wallet Modal
/// BlockchainMenu sahnesine WalletModal objelerini otomatik ekler.
/// Konum: Assets/Editor/WalletModalBuilder.cs
/// </summary>
public static class WalletModalBuilder
{
    [MenuItem("Tools/GLITCHRUNNER/Build Wallet Modal")]
    public static void Build()
    {
        // UICanvas'ı bul
        var uiCanvas = GameObject.Find("UICanvas");
        if (uiCanvas == null)
        {
            EditorUtility.DisplayDialog("Hata",
                "UICanvas bulunamadı!\nÖnce 'Build Blockchain Menu Scene' çalıştır.", "OK");
            return;
        }

        // Eski modal varsa sil
        var old = uiCanvas.transform.Find("WalletModal");
        if (old) GameObject.DestroyImmediate(old.gameObject);

        // ── Modal kök ────────────────────────────────────────────────────
        var modalGO = new GameObject("WalletModal");
        modalGO.transform.SetParent(uiCanvas.transform, false);
        var modalRT = modalGO.AddComponent<RectTransform>();
        modalRT.anchorMin = modalRT.anchorMax = new Vector2(0.5f, 0.5f);
        modalRT.pivot     = new Vector2(0.5f, 0.5f);
        modalRT.sizeDelta = new Vector2(340, 440);
        modalRT.anchoredPosition = Vector2.zero;

        var modalCG = modalGO.AddComponent<CanvasGroup>();
        modalCG.alpha = 0f; modalCG.interactable = false; modalCG.blocksRaycasts = false;

        // Arka plan
        var modalBg = modalGO.AddComponent<Image>();
        modalBg.color = new Color(0f, 0.012f, 0.004f, 0.95f);
        var modalOL = modalGO.AddComponent<Outline>();
        modalOL.effectColor = new Color(0f, 1f, 0.3f, 0.55f);
        modalOL.effectDistance = new Vector2(1f, -1f);

        // Köşe süsleri
        AddCornerDecor(modalGO.transform, modalRT.sizeDelta);

        // ── Header ───────────────────────────────────────────────────────
        var headerGO = new GameObject("Header");
        headerGO.transform.SetParent(modalGO.transform, false);
        var headerRT = headerGO.AddComponent<RectTransform>();
        headerRT.anchorMin = new Vector2(0f, 1f); headerRT.anchorMax = new Vector2(1f, 1f);
        headerRT.pivot = new Vector2(0.5f, 1f);
        headerRT.offsetMin = new Vector2(0, -50); headerRT.offsetMax = Vector2.zero;
        headerGO.AddComponent<Image>().color = new Color(0f, 0.02f, 0.005f, 0f);

        MakeTmpTxt("AuthLabel", headerGO.transform,
            "AUTHENTICATION MODULE",
            new Vector2(0, -10), new Vector2(280, 16),
            new Color(0f, 1f, 0.3f, 0.85f), 9, FontStyle.Normal, TextAlignmentOptions.Center);

        MakeTmpTxt("SelectLabel", headerGO.transform,
            "SELECT WALLET",
            new Vector2(0, -26), new Vector2(280, 20),
            new Color(0f, 1f, 0.35f, 1.00f), 13, FontStyle.Bold, TextAlignmentOptions.Center);

        // Ayırıcı
        var divGO = new GameObject("Divider"); divGO.transform.SetParent(headerGO.transform, false);
        var divRT = divGO.AddComponent<RectTransform>();
        divRT.anchorMin = new Vector2(0f, 0f); divRT.anchorMax = new Vector2(1f, 0f);
        divRT.pivot = new Vector2(0.5f, 0f);
        divRT.offsetMin = new Vector2(12, -1); divRT.offsetMax = new Vector2(-12, 0);
        divGO.AddComponent<Image>().color = new Color(0f, 1f, 0.3f, 0.12f);

        // Kapat butonu
        var closeGO = new GameObject("CloseBtn");
        closeGO.transform.SetParent(headerGO.transform, false);
        var closeRT = closeGO.AddComponent<RectTransform>();
        closeRT.anchorMin = closeRT.anchorMax = new Vector2(1f, 0.5f);
        closeRT.pivot = new Vector2(1f, 0.5f);
        closeRT.anchoredPosition = new Vector2(-8, 0); closeRT.sizeDelta = new Vector2(20, 20);
        closeGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        var closeBtn = closeGO.AddComponent<Button>();
        var closeTxt = MakeTmpTxt("X", closeGO.transform, "✕",
            Vector2.zero, new Vector2(20, 20),
            new Color(1f, 0.1f, 0.05f, 0.7f), 12, FontStyle.Bold, TextAlignmentOptions.Center);

        // ── Google Butonu ────────────────────────────────────────────────
        var googleGO = new GameObject("GoogleBtn");
        googleGO.transform.SetParent(modalGO.transform, false);
        var googleRT = googleGO.AddComponent<RectTransform>();
        googleRT.anchorMin = new Vector2(0f, 1f); googleRT.anchorMax = new Vector2(1f, 1f);
        googleRT.pivot = new Vector2(0.5f, 1f);
        googleRT.offsetMin = new Vector2(12, -100); googleRT.offsetMax = new Vector2(-12, -54);
        var googleImg = googleGO.AddComponent<Image>();
        googleImg.color = new Color(0f, 0.04f, 0.01f, 1f);
        var googleOL = googleGO.AddComponent<Outline>();
        googleOL.effectColor = new Color(0f, 1f, 0.3f, 0.38f);
        googleOL.effectDistance = new Vector2(1f, -1f);
        var googleBtn = googleGO.AddComponent<Button>();
        var gColors = googleBtn.colors;
        gColors.normalColor      = new Color(0,0,0,0);
        gColors.highlightedColor = new Color(0f, 1f, 0.3f, 0.08f);
        gColors.pressedColor     = new Color(0f, 1f, 0.3f, 0.18f);
        googleBtn.colors = gColors;

        // Google logo (SVG yerine basit "G" — gerçek logo Resources'tan gelecek)
        var gLogoGO = new GameObject("Logo"); gLogoGO.transform.SetParent(googleGO.transform, false);
        var gLogoRT = gLogoGO.AddComponent<RectTransform>();
        gLogoRT.anchorMin = gLogoRT.anchorMax = new Vector2(0f, 0.5f);
        gLogoRT.pivot = new Vector2(0f, 0.5f);
        gLogoRT.anchoredPosition = new Vector2(10, 0); gLogoRT.sizeDelta = new Vector2(20, 20);
        var gLogoImg = gLogoGO.AddComponent<Image>();
        gLogoImg.color = new Color(0.26f, 0.52f, 1f, 0.9f); // Google logo yüklenene kadar mavi
        // CURSOR NOTU: Resources.Load<Sprite>("WalletLogos/google") ile logoyu yükle

        MakeTmpTxt("GoogleLabel", googleGO.transform,
            "SIGN IN WITH GOOGLE",
            new Vector2(14, 0), new Vector2(220, 36),
            new Color(0f, 1f, 0.35f, 1.00f), 13, FontStyle.Bold, TextAlignmentOptions.Left);

        MakeTmpTxt("GoogleArrow", googleGO.transform,
            "▶",
            new Vector2(-10, 0), new Vector2(14, 36),
            new Color(0f, 1f, 0.3f, 0.30f), 10, FontStyle.Normal, TextAlignmentOptions.Right);

        // ── Ayırıcı çizgi ────────────────────────────────────────────────
        var sep = new GameObject("Separator"); sep.transform.SetParent(modalGO.transform, false);
        var sepRT = sep.AddComponent<RectTransform>();
        sepRT.anchorMin = new Vector2(0f, 1f); sepRT.anchorMax = new Vector2(1f, 1f);
        sepRT.pivot = new Vector2(0.5f, 1f);
        sepRT.offsetMin = new Vector2(12, -112); sepRT.offsetMax = new Vector2(-12, -111);
        sep.AddComponent<Image>().color = new Color(0f, 1f, 0.3f, 0.08f);

        MakeTmpTxt("OrLabel", sep.transform,
            "── OR ──",
            new Vector2(0, -6), new Vector2(80, 14),
            new Color(0f, 1f, 0.3f, 0.45f), 9, FontStyle.Normal, TextAlignmentOptions.Center);

        // ── ScrollRect + Wallet List ──────────────────────────────────────
        var scrollGO = new GameObject("WalletScroll");
        scrollGO.transform.SetParent(modalGO.transform, false);
        var scrollRT = scrollGO.AddComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0f, 0f); scrollRT.anchorMax = new Vector2(1f, 1f);
        scrollRT.offsetMin = new Vector2(12, 36); scrollRT.offsetMax = new Vector2(-12, -118);
        scrollGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

        var scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical   = true;
        scrollRect.scrollSensitivity = 15f;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        // Scrollbar (ince, yeşil)
        var scrollbarGO = new GameObject("Scrollbar");
        scrollbarGO.transform.SetParent(scrollGO.transform, false);
        var sbRT = scrollbarGO.AddComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(1f, 0f); sbRT.anchorMax = new Vector2(1f, 1f);
        sbRT.pivot = new Vector2(1f, 0.5f);
        sbRT.sizeDelta = new Vector2(3, 0); sbRT.anchoredPosition = Vector2.zero;
        scrollbarGO.AddComponent<Image>().color = new Color(0f, 0.3f, 0.08f, 0.3f);
        var sb = scrollbarGO.AddComponent<Scrollbar>();
        sb.direction = Scrollbar.Direction.BottomToTop;

        var sbHandleArea = new GameObject("SlidingArea"); sbHandleArea.transform.SetParent(scrollbarGO.transform, false);
        var sbHaRT = sbHandleArea.AddComponent<RectTransform>();
        sbHaRT.anchorMin = Vector2.zero; sbHaRT.anchorMax = Vector2.one;
        sbHaRT.offsetMin = sbHaRT.offsetMax = Vector2.zero;
        var sbHandle = new GameObject("Handle"); sbHandle.transform.SetParent(sbHandleArea.transform, false);
        var sbHRT = sbHandle.AddComponent<RectTransform>();
        sbHRT.anchorMin = Vector2.zero; sbHRT.anchorMax = Vector2.one;
        sbHRT.offsetMin = sbHRT.offsetMax = Vector2.zero;
        sbHandle.AddComponent<Image>().color = new Color(0f, 1f, 0.3f, 0.35f);
        sb.handleRect = sbHRT;
        sb.targetGraphic = sbHandle.GetComponent<Image>();
        scrollRect.verticalScrollbar = sb;

        // Viewport
        var vpGO = new GameObject("Viewport"); vpGO.transform.SetParent(scrollGO.transform, false);
        var vpRT = vpGO.AddComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero; vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = Vector2.zero; vpRT.offsetMax = new Vector2(-5, 0);
        vpGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        var mask = vpGO.AddComponent<Mask>(); mask.showMaskGraphic = false;

        // Content
        var contentGO = new GameObject("Content"); contentGO.transform.SetParent(vpGO.transform, false);
        var contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f); contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot = new Vector2(0.5f, 1f);
        contentRT.sizeDelta = new Vector2(0, 0);
        contentRT.anchoredPosition = Vector2.zero;
        var vLayout = contentGO.AddComponent<VerticalLayoutGroup>();
        vLayout.spacing = 4f; vLayout.childForceExpandWidth = true; vLayout.childForceExpandHeight = false;
        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content  = contentRT;
        scrollRect.viewport = vpRT;

        // ── Wallet Row Prefab ─────────────────────────────────────────────
        var prefabGO = BuildWalletRowPrefab();

        // ── Status bar ───────────────────────────────────────────────────
        var statusGO = new GameObject("StatusBar"); statusGO.transform.SetParent(modalGO.transform, false);
        var statusRT = statusGO.AddComponent<RectTransform>();
        statusRT.anchorMin = new Vector2(0f, 0f); statusRT.anchorMax = new Vector2(1f, 0f);
        statusRT.pivot = new Vector2(0.5f, 0f);
        statusRT.offsetMin = Vector2.zero; statusRT.offsetMax = new Vector2(0, 30);
        statusGO.AddComponent<Image>().color = new Color(0f, 0.02f, 0.005f, 0f);
        var statusTxt = MakeTmpTxt("StatusText", statusGO.transform, "",
            new Vector2(0, 0), new Vector2(300, 28),
            new Color(0f, 0.85f, 0.25f, 0.90f), 10, FontStyle.Normal, TextAlignmentOptions.Center)
            .GetComponent<TextMeshProUGUI>();

        // ── WalletModalUI scripti bağla ───────────────────────────────────
        var walletUI = modalGO.AddComponent<WalletModalUI>();
        walletUI.modalGroup        = modalCG;
        walletUI.modalPanel        = modalRT;
        walletUI.googleButton      = googleBtn;
        walletUI.googleLabel       = googleGO.transform.Find("GoogleLabel")?.GetComponent<TextMeshProUGUI>();
        walletUI.walletListContent = contentRT;
        walletUI.walletRowPrefab   = prefabGO;
        walletUI.statusText        = statusTxt;
        walletUI.closeButton       = closeBtn;

        var fadeCanvas = GameObject.Find("FadeCanvas");
        if (fadeCanvas != null)
        {
            var fadeImg = fadeCanvas.transform.Find("FadeImage");
            if (fadeImg != null)
                walletUI.fadeGroup = fadeImg.GetComponent<CanvasGroup>();
        }

        // BootPanelController'a walletModalUI referansını ver
        var bpc = Object.FindObjectOfType<BootPanelController>();
        if (bpc != null)
        {
            var so = new SerializedObject(bpc);
            var prop = so.FindProperty("walletModalUI");
            if (prop != null)
            {
                prop.objectReferenceValue = walletUI;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        EditorUtility.DisplayDialog("GLITCHRUNNER ✓",
            "WalletModal oluşturuldu!\n\n" +
            "Sonraki adım: Cursor rehberini oku ve\n" +
            "Reown SDK entegrasyonunu tamamla.", "OK");
    }

    static GameObject BuildWalletRowPrefab()
    {
        var rowGO = new GameObject("WalletRow_PREFAB");
        rowGO.transform.SetParent(null); // prefab gibi davransın
        var rowRT = rowGO.AddComponent<RectTransform>();
        rowRT.sizeDelta = new Vector2(260, 36);
        var rowImg = rowGO.AddComponent<Image>();
        rowImg.color = new Color(0f, 0f, 0f, 0f);
        var rowOL = rowGO.AddComponent<Outline>();
        rowOL.effectColor = new Color(0f, 1f, 0.3f, 0.22f);
        rowOL.effectDistance = new Vector2(1f, -1f);
        var rowBtn = rowGO.AddComponent<Button>();
        var rowColors = rowBtn.colors;
        rowColors.normalColor      = new Color(0,0,0,0);
        rowColors.highlightedColor = new Color(0f, 1f, 0.3f, 0.07f);
        rowColors.pressedColor     = new Color(0f, 1f, 0.3f, 0.15f);
        rowBtn.colors = rowColors;

        // Logo
        var logoGO = new GameObject("Logo"); logoGO.transform.SetParent(rowGO.transform, false);
        var logoRT = logoGO.AddComponent<RectTransform>();
        logoRT.anchorMin = logoRT.anchorMax = new Vector2(0f, 0.5f);
        logoRT.pivot = new Vector2(0f, 0.5f);
        logoRT.anchoredPosition = new Vector2(8, 0); logoRT.sizeDelta = new Vector2(22, 22);
        var logoImg = logoGO.AddComponent<Image>();
        logoImg.color = new Color(0f, 1f, 0.3f, 0.25f); // placeholder

        // İsim — TMP (Türkçe karakter + monospace)
        var nameGO = new GameObject("Name"); nameGO.transform.SetParent(rowGO.transform, false);
        var nameRT = nameGO.AddComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0f, 0f); nameRT.anchorMax = new Vector2(1f, 1f);
        nameRT.offsetMin = new Vector2(38, 0); nameRT.offsetMax = new Vector2(-20, 0);
        var nameTxt = nameGO.AddComponent<TextMeshProUGUI>();
        nameTxt.text = "WALLET"; nameTxt.color = new Color(0f, 1f, 0.35f, 1.00f);
        nameTxt.fontSize = 13; nameTxt.fontStyle = FontStyles.Bold;
        nameTxt.alignment = TextAlignmentOptions.Left;
        var rowFont = LoadMonoFont();
        if (rowFont != null) nameTxt.font = rowFont;

        // Ok
        var arrowGO = new GameObject("Arrow"); arrowGO.transform.SetParent(rowGO.transform, false);
        var arrowRT = arrowGO.AddComponent<RectTransform>();
        arrowRT.anchorMin = new Vector2(1f, 0f); arrowRT.anchorMax = new Vector2(1f, 1f);
        arrowRT.pivot = new Vector2(1f, 0.5f);
        arrowRT.offsetMin = new Vector2(-18, 0); arrowRT.offsetMax = new Vector2(-6, 0);
        var arrowTxt = arrowGO.AddComponent<TextMeshProUGUI>();
        arrowTxt.text = "▶"; arrowTxt.color = new Color(0f, 1f, 0.3f, 0.35f);
        arrowTxt.fontSize = 10; arrowTxt.alignment = TextAlignmentOptions.Center;
        if (rowFont != null) arrowTxt.font = rowFont;

        // Prefab olarak kaydet
        if (!System.IO.Directory.Exists("Assets/Prefabs"))
            System.IO.Directory.CreateDirectory("Assets/Prefabs");
        string prefabPath = "Assets/Prefabs/WalletRow.prefab";
        var saved = PrefabUtility.SaveAsPrefabAsset(rowGO, prefabPath);
        GameObject.DestroyImmediate(rowGO);
        AssetDatabase.Refresh();
        return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
    }

    static void AddCornerDecor(Transform parent, Vector2 panelSize)
    {
        float s = 8f; float hw = panelSize.x / 2f; float hh = panelSize.y / 2f;
        var corners = new[] {
            (new Vector2(-hw, hh),  new Vector2(1,-1)),
            (new Vector2( hw, hh),  new Vector2(-1,-1)),
            (new Vector2(-hw, -hh), new Vector2(1, 1)),
            (new Vector2( hw, -hh), new Vector2(-1, 1)),
        };
        foreach (var (pos, dir) in corners)
        {
            var go = new GameObject("Corner"); go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(s, s);
            var img = go.AddComponent<Image>(); img.color = new Color(0f,1f,0.3f,0f);
            var ol = go.AddComponent<Outline>();
            ol.effectColor = new Color(0f, 1f, 0.3f, 0.4f);
            ol.effectDistance = new Vector2(dir.x, dir.y);
        }
    }

    static GameObject MakeTmpTxt(string name, Transform parent, string content,
        Vector2 pos, Vector2 size, Color color, int fontSize, FontStyle style, TextAlignmentOptions align)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        var txt = go.AddComponent<TextMeshProUGUI>();
        txt.text = content; txt.color = color; txt.fontSize = fontSize;
        txt.fontStyle = (style == FontStyle.Bold) ? FontStyles.Bold : FontStyles.Normal;
        txt.alignment = align;
        var font = LoadMonoFont();
        if (font != null) txt.font = font;
        return go;
    }

    static TMP_FontAsset LoadMonoFont()
    {
        var font = Resources.Load<TMP_FontAsset>("Fonts/ShareTechMono-Regular SDF");
        if (font != null) return font;
        return TMP_Settings.defaultFontAsset;
    }

    static GameObject MakeTxt(string name, Transform parent, string content,
        Vector2 pos, Vector2 size, Color color, int fontSize, FontStyle style, TextAnchor align)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        var txt = go.AddComponent<Text>();
        txt.text = content; txt.color = color; txt.fontSize = fontSize;
        txt.fontStyle = style; txt.alignment = align; txt.supportRichText = false;
        Font m = FindMono(); if (m) txt.font = m;
        return go;
    }

    static Font FindMono()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Font"))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid).ToLower();
            if (p.Contains("mono") || p.Contains("consolas") || p.Contains("courier") ||
                p.Contains("sharetech") || p.Contains("hack") || p.Contains("liberation"))
                return AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(guid));
        }
        return null;
    }
}
#endif
