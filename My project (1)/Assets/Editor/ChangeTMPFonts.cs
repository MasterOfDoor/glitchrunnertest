#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;

/// Assets/Editor/ klasörüne koy.
/// Üst menüde Tools → Change All TMP Fonts ile çalışır.
public class ChangeTMPFonts : EditorWindow
{
    TMP_FontAsset newFont;

    [MenuItem("Tools/Change All TMP Fonts")]
    static void Open() => GetWindow<ChangeTMPFonts>("TMP Font Değiştir");

    void OnGUI()
    {
        GUILayout.Label("Yeni Font:", EditorStyles.boldLabel);
        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField(
            newFont, typeof(TMP_FontAsset), false);

        GUILayout.Space(10);

        if (newFont == null)
        {
            EditorGUILayout.HelpBox("Önce bir font seç.", MessageType.Warning);
            return;
        }

        if (GUILayout.Button("Sahnedeki Tüm TMP'leri Değiştir"))
            ChangeAll();
    }

    void ChangeAll()
    {
        int count = 0;

        // UI (TextMeshProUGUI)
        foreach (var t in FindObjectsByType<TextMeshProUGUI>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Undo.RecordObject(t, "Change TMP Font");
            t.font = newFont;
            EditorUtility.SetDirty(t);
            count++;
        }

        // 3D (TextMeshPro)
        foreach (var t in FindObjectsByType<TextMeshPro>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Undo.RecordObject(t, "Change TMP Font");
            t.font = newFont;
            EditorUtility.SetDirty(t);
            count++;
        }

        Debug.Log($"[ChangeTMPFonts] {count} obje güncellendi.");
    }
}
#endif
