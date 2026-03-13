#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class CreateGameUITheme
{
    [MenuItem("Game/Create UI Theme (Resources)")]
    public static void Create()
    {
        if (Resources.Load<GameUITheme>("GameUITheme") != null)
        {
            Debug.Log("GameUITheme already exists in Resources.");
            return;
        }
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        var so = ScriptableObject.CreateInstance<GameUITheme>();
        AssetDatabase.CreateAsset(so, "Assets/Resources/GameUITheme.asset");
        AssetDatabase.SaveAssets();
        Debug.Log("Created Assets/Resources/GameUITheme.asset - Grafik tasarımcı bu asset'i seçip Inspector'dan renk ve boyutları düzenleyebilir.");
    }
}
#endif
