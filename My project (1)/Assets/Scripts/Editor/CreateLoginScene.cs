#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class CreateLoginScene
{
    [MenuItem("Game/Create Login Scene")]
    public static void Create()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var loginGo = new GameObject("LoginUI");
        loginGo.AddComponent<LoginUI>();
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        string path = "Assets/Scenes/Login.unity";
        EditorSceneManager.SaveScene(scene, path);

        var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        int existing = list.FindIndex(s => s.path == path);
        if (existing >= 0) list.RemoveAt(existing);
        list.Insert(0, new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = list.ToArray();

        Debug.Log("Created " + path + " and added as first scene in Build Settings.");
    }
}
#endif
