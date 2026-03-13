#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class BlockchainMenuBuilder
{
    [MenuItem("Tools/GLITCHRUNNER/Build Blockchain Menu Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Camera ───────────────────────────────────────────────────────
        var camGO = new GameObject("Main Camera"); camGO.tag = "MainCamera";
        var cam   = camGO.AddComponent<Camera>();
        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0f, 0.003f, 0f);
        cam.orthographic    = true; cam.depth = -1;
        camGO.transform.position = new Vector3(0, 0, -10);
        camGO.AddComponent<AudioListener>();

        // ── EventSystem ──────────────────────────────────────────────────
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>(); es.AddComponent<StandaloneInputModule>();

        // ════════════════════════════════════════════════════════════════
        // CANVAS 1 — Matrix yağmuru (sortingOrder = 0, EN ARKADA)
        // ════════════════════════════════════════════════════════════════
        var matCanvasGO = new GameObject("MatrixCanvas");
        var matCanvas   = matCanvasGO.AddComponent<Canvas>();
        matCanvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        matCanvas.sortingOrder = 0;
        var matScaler = matCanvasGO.AddComponent<CanvasScaler>();
        matScaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        matScaler.referenceResolution  = new Vector2(640, 360);
        matScaler.screenMatchMode      = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        matScaler.matchWidthOrHeight   = 0.5f;
        matCanvasGO.AddComponent<GraphicRaycaster>();

        // MatrixLayer — tam ekran, şeffaf arka plan
        var matLayerGO = new GameObject("MatrixLayer");
        matLayerGO.transform.SetParent(matCanvasGO.transform, false);
        var matRT = matLayerGO.AddComponent<RectTransform>();
        matRT.anchorMin = Vector2.zero; matRT.anchorMax = Vector2.one;
        matRT.offsetMin = matRT.offsetMax = Vector2.zero;
        // Image YOK — sadece script var, Image eklersek yağmurun önünü kapar
        AttachScript(matLayerGO, "BlockchainMatrix");

        // ════════════════════════════════════════════════════════════════
        // CANVAS 2 — Boot Panel (sortingOrder = 10, ÖNDE)
        // ════════════════════════════════════════════════════════════════
        var uiCanvasGO = new GameObject("UICanvas");
        var uiCanvas   = uiCanvasGO.AddComponent<Canvas>();
        uiCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        uiCanvas.sortingOrder = 10;
        var uiScaler = uiCanvasGO.AddComponent<CanvasScaler>();
        uiScaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        uiScaler.referenceResolution  = new Vector2(640, 360);
        uiScaler.screenMatchMode      = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        uiScaler.matchWidthOrHeight   = 0.5f;
        uiCanvasGO.AddComponent<GraphicRaycaster>();

        // ── BootPanel ────────────────────────────────────────────────────
        var bootGO = new GameObject("BootPanel");
        bootGO.transform.SetParent(uiCanvasGO.transform, false);
        var bootRT = bootGO.AddComponent<RectTransform>();
        bootRT.anchorMin        = new Vector2(0.5f, 0.5f);
        bootRT.anchorMax        = new Vector2(0.5f, 0.5f);
        bootRT.pivot            = new Vector2(0.5f, 0.5f);
        bootRT.sizeDelta        = new Vector2(300, 260);   // Küçük panel
        bootRT.anchoredPosition = Vector2.zero;

        // Panel arka planı: koyu siyah, yarı şeffaf
        var bootImg = bootGO.AddComponent<Image>();
        bootImg.color = new Color(0f, 0.015f, 0.005f, 0.87f);

        var bootCG = bootGO.AddComponent<CanvasGroup>();
        bootCG.alpha = 0f; bootCG.interactable = true; bootCG.blocksRaycasts = true;

        // Yeşil border outline
        var bootOL = bootGO.AddComponent<Outline>();
        bootOL.effectColor    = new Color(0f, 1f, 0.3f, 0.6f);
        bootOL.effectDistance = new Vector2(1f, -1f);

        // ── Header ───────────────────────────────────────────────────────
        MakeTxt("HeaderText", bootGO.transform,
            "BLOCKCHAIN_OS  v0.1",
            new Vector2(0, 113), new Vector2(280, 18),
            new Color(0f, 1f, 0.3f, 0.80f), 9, FontStyle.Bold, TextAnchor.MiddleCenter);

        MakeTxt("SubText", bootGO.transform,
            "// SYSTEM COMPROMISED",
            new Vector2(0, 97), new Vector2(280, 14),
            new Color(1f, 0.1f, 0.05f, 0.75f), 7, FontStyle.Normal, TextAnchor.MiddleCenter);

        // Ayırıcı
        var divGO = new GameObject("Divider"); divGO.transform.SetParent(bootGO.transform, false);
        var divRT = divGO.AddComponent<RectTransform>();
        divRT.anchorMin = divRT.anchorMax = new Vector2(0.5f, 0.5f);
        divRT.anchoredPosition = new Vector2(0, 86); divRT.sizeDelta = new Vector2(270, 1);
        divGO.AddComponent<Image>().color = new Color(0f, 1f, 0.3f, 0.15f);

        // ── Log satırları (6) ─────────────────────────────────────────────
        for (int i = 0; i < 6; i++)
            MakeTxt($"LogLine_{i}", bootGO.transform, "",
                new Vector2(-8, 72f - i * 19f), new Vector2(270, 17),
                new Color(0f, 0.55f, 0.15f, 0.8f), 8, FontStyle.Normal, TextAnchor.MiddleLeft);

        // İmleç
        MakeTxt("CursorText", bootGO.transform, "",
            new Vector2(-126, -44), new Vector2(16, 16),
            new Color(0f, 1f, 0.3f, 1f), 9, FontStyle.Bold, TextAnchor.MiddleLeft);

        // ── PromptText — "PLEASE ENTER: CONNECT" (başta gizli) ──────────
        var promptGO = MakeTxt("PromptText", bootGO.transform,
            "PLEASE ENTER: CONNECT",
            new Vector2(0, -62), new Vector2(280, 20),
            new Color(1f, 0f, 0f, 1f), 11, FontStyle.Bold, TextAnchor.MiddleCenter);
        promptGO.SetActive(false);

        // ── InputField (başta gizli) ──────────────────────────────────────
        var ifGO = new GameObject("ConnectInput");
        ifGO.transform.SetParent(bootGO.transform, false);
        var ifRT = ifGO.AddComponent<RectTransform>();
        ifRT.anchorMin = ifRT.anchorMax = new Vector2(0.5f, 0.5f);
        ifRT.anchoredPosition = new Vector2(0, -84); ifRT.sizeDelta = new Vector2(220, 24);

        ifGO.AddComponent<Image>().color = new Color(0f, 0.06f, 0.015f, 0.9f);
        var ifOL = ifGO.AddComponent<Outline>();
        ifOL.effectColor = new Color(0f, 1f, 0.3f, 0.45f); ifOL.effectDistance = new Vector2(1f, -1f);

        var inputField = ifGO.AddComponent<InputField>();
        Font mono = FindMono();

        // Placeholder
        var phGO = new GameObject("Placeholder"); phGO.transform.SetParent(ifGO.transform, false);
        SetRTFull(phGO, 5, 2);
        var phTxt = phGO.AddComponent<Text>();
        phTxt.text = "type \"connect\"...";
        phTxt.color = new Color(0f, 0.45f, 0.12f, 0.45f);
        phTxt.fontSize = 9; phTxt.fontStyle = FontStyle.Italic;
        phTxt.alignment = TextAnchor.MiddleLeft;
        if (mono) phTxt.font = mono;

        // Text
        var itGO = new GameObject("Text"); itGO.transform.SetParent(ifGO.transform, false);
        SetRTFull(itGO, 5, 2);
        var itTxt = itGO.AddComponent<Text>();
        itTxt.color = new Color(0f, 1f, 0.3f, 1f);
        itTxt.fontSize = 10; itTxt.fontStyle = FontStyle.Bold;
        itTxt.alignment = TextAnchor.MiddleLeft;
        if (mono) itTxt.font = mono;

        inputField.textComponent  = itTxt;
        inputField.placeholder    = phTxt;
        inputField.caretColor     = new Color(0f, 1f, 0.3f, 1f);
        inputField.caretWidth     = 2;
        inputField.selectionColor = new Color(0f, 1f, 0.3f, 0.25f);
        ifGO.SetActive(false);

        // ── BootPanelController — tüm referansları bağla ─────────────────
        // walletModalUI, "Build Wallet Modal" çalıştırıldığında WalletModalBuilder tarafından atanır
        var bpc = bootGO.AddComponent<BootPanelController>();
        bpc.bootPanelGroup  = bootCG;
        bpc.panelBackground = bootImg;
        bpc.cursorText      = bootGO.transform.Find("CursorText")?.GetComponent<Text>();
        bpc.promptText      = bootGO.transform.Find("PromptText")?.GetComponent<Text>();
        bpc.inputField      = ifGO.GetComponent<InputField>();
        var logs = new Text[6];
        for (int i = 0; i < 6; i++)
            logs[i] = bootGO.transform.Find($"LogLine_{i}")?.GetComponent<Text>();
        bpc.bootLogTexts = logs;

        // ── FadeCanvas (en üstte, geçiş için) ───────────────────────────
        var fadeCV = new GameObject("FadeCanvas");
        var fC = fadeCV.AddComponent<Canvas>();
        fC.renderMode = RenderMode.ScreenSpaceOverlay; fC.sortingOrder = 99;
        fadeCV.AddComponent<CanvasScaler>(); fadeCV.AddComponent<GraphicRaycaster>();
        var fadeImgGO = new GameObject("FadeImage"); fadeImgGO.transform.SetParent(fadeCV.transform, false);
        SetRTStretch(fadeImgGO);
        fadeImgGO.AddComponent<Image>().color = new Color(0,0,0,0);
        var fadeCG = fadeImgGO.AddComponent<CanvasGroup>();
        fadeCG.alpha = 0f; fadeCG.interactable = false; fadeCG.blocksRaycasts = false;

        // ── Kaydet ────────────────────────────────────────────────────────
        if (!Directory.Exists("Assets/Scenes")) Directory.CreateDirectory("Assets/Scenes");
        string path = "Assets/Scenes/BlockchainMenu.unity";
        EditorSceneManager.SaveScene(scene, path);
        AssetDatabase.Refresh();
        AddToBuild(path);

        EditorUtility.DisplayDialog("GLITCHRUNNER ✓",
            "BlockchainMenu.unity hazır!\n\nPlay'e bas ve dene.", "OK");
    }

    // ── Yardımcılar ──────────────────────────────────────────────────────

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

    static void SetRTFull(GameObject go, float padH, float padV)
    {
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(padH, padV); rt.offsetMax = new Vector2(-padH, -padV);
    }

    static void SetRTStretch(GameObject go)
    {
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static Font FindMono()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Font"))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid).ToLower();
            if (p.Contains("mono") || p.Contains("consolas") || p.Contains("courier") ||
                p.Contains("sharetech") || p.Contains("liberation") || p.Contains("hack"))
                return AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(guid));
        }
        return null;
    }

    static void AttachScript(GameObject go, string scriptName)
    {
        foreach (var guid in AssetDatabase.FindAssets($"{scriptName} t:Script"))
        {
            var ms = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));
            if (ms?.GetClass() != null) { go.AddComponent(ms.GetClass()); return; }
        }
        Debug.LogWarning($"[Builder] '{scriptName}.cs' bulunamadı!");
    }

    static void AddToBuild(string path)
    {
        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in list) if (s.path == path) return;
        list.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = list.ToArray();
    }
}
#endif
