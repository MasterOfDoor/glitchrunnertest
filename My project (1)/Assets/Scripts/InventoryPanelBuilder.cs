using UnityEngine;
using UnityEngine.UI;
using GlitchRunner.Inventory;

/// <summary>
/// Envanter panelini runtime'da oluşturur. Grafik tasarımcı GameUITheme asset'i ile renk/boyut düzenleyebilir.
/// </summary>
public static class InventoryPanelBuilder
{
    const int SlotCount = 20;
    const int GridColumns = 5;
    const int SlotSize = 48;
    const int SlotSpacing = 4;
    const int Padding = 12;
    const int TitleHeight = 28;

    static GameUITheme GetTheme()
    {
        return Resources.Load<GameUITheme>("GameUITheme");
    }

    public static GameObject Build()
    {
        EnsureCanvasAndEventSystem();
        var t = GetTheme();
        GameObject canvasObj = GetOrCreateCanvas(t);

        if (t != null && t.inventoryPanelPrefab != null)
        {
            GameObject panel = Object.Instantiate(t.inventoryPanelPrefab, canvasObj.transform, false);
            panel.name = "InventoryPanel";
            var rt = panel.GetComponent<RectTransform>();
            if (rt == null) rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            panel.SetActive(false);
            return panel;
        }

        return BuildFromCode(t, canvasObj);
    }

    static GameObject GetOrCreateCanvas(GameUITheme t)
    {
        GameObject canvasObj = GameObject.Find("InventoryCanvas");
        if (canvasObj != null) return canvasObj;
        int refW = t != null ? t.referenceWidth : 640;
        int refH = t != null ? t.referenceHeight : 360;
        canvasObj = new GameObject("InventoryCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(refW, refH);
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referencePixelsPerUnit = 100f;
        canvasObj.AddComponent<GraphicRaycaster>();
        return canvasObj;
    }

    static GameObject BuildFromCode(GameUITheme t, GameObject canvasObj)
    {
        int refW = t != null ? t.referenceWidth : 640;
        int refH = t != null ? t.referenceHeight : 360;
        Color panelBg = t != null ? t.panelBackground : new Color(0.05f, 0.12f, 0.05f, 0.98f);
        Color panelOut = t != null ? t.panelOutline : new Color(0f, 1f, 0.25f, 1f);
        Vector2 outlineDist = t != null ? t.panelOutlineDistance : new Vector2(2, 2);
        Vector2 panelSize = t != null ? t.inventoryPanelSize : new Vector2(300, 270);
        Color titleCol = t != null ? t.titleColor : new Color(0f, 1f, 0.25f, 1f);
        int titleSize = t != null ? t.titleFontSize : 18;
        Color slotBg = t != null ? t.rowBackground : new Color(0.06f, 0.15f, 0.06f, 0.95f);
        Color slotOut = t != null ? t.rowOutline : new Color(0f, 0.5f, 0.15f, 0.9f);
        Vector2 slotOutDist = t != null ? t.rowOutlineDistance : new Vector2(1, 1);

        GameObject panel = new GameObject("InventoryPanel");
        panel.transform.SetParent(canvasObj.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = panelSize;
        panelRect.anchoredPosition = Vector2.zero;
        panel.AddComponent<Image>().color = panelBg;
        var outline = panel.AddComponent<Outline>();
        outline.effectColor = panelOut;
        outline.effectDistance = outlineDist;

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.sizeDelta = new Vector2(-Padding * 2, TitleHeight);
        titleRect.anchoredPosition = new Vector2(0, -Padding);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "ENVANTER";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = titleSize;
        titleText.color = titleCol;
        titleText.alignment = TextAnchor.MiddleCenter;

        GameObject slotsRoot = new GameObject("Slots");
        slotsRoot.transform.SetParent(panel.transform, false);
        RectTransform slotsRect = slotsRoot.AddComponent<RectTransform>();
        slotsRect.anchorMin = new Vector2(0, 0);
        slotsRect.anchorMax = new Vector2(1, 1);
        slotsRect.offsetMin = new Vector2(Padding, Padding);
        slotsRect.offsetMax = new Vector2(-Padding, -(Padding + TitleHeight + 4));
        GridLayoutGroup grid = slotsRoot.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(SlotSize, SlotSize);
        grid.spacing = new Vector2(SlotSpacing, SlotSpacing);
        grid.padding = new RectOffset(0, 0, 0, 0);
        grid.constraintCount = GridColumns;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.childAlignment = TextAnchor.UpperCenter;

        for (int i = 0; i < SlotCount; i++)
        {
            GameObject slotObj = new GameObject("Slot" + i);
            slotObj.transform.SetParent(slotsRoot.transform, false);
            slotObj.AddComponent<Image>().color = slotBg;
            var slotOutline = slotObj.AddComponent<Outline>();
            slotOutline.effectColor = slotOut;
            slotOutline.effectDistance = slotOutDist;
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(slotObj.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(4, 4);
            iconRect.offsetMax = new Vector2(-4, -18);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.color = Color.white;
            iconImg.enabled = false;
            iconImg.raycastTarget = false;
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(slotObj.transform, false);
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(1, 0);
            labelRect.pivot = new Vector2(0.5f, 0);
            labelRect.offsetMin = new Vector2(2, 2);
            labelRect.offsetMax = new Vector2(-2, 14);
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = "";
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 9;
            labelText.color = new Color(0f, 1f, 0.35f);
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.raycastTarget = false;
            InventorySlotUI slotUI = slotObj.AddComponent<InventorySlotUI>();
            slotUI.iconImage = iconImg;
            slotUI.labelText = labelText;
        }

        panel.AddComponent<InventoryUI>();
        panel.SetActive(false);
        return panel;
    }

    static void EnsureCanvasAndEventSystem()
    {
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }
}
