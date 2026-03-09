using System;
using System.IO;
using UnityEngine;

/// <summary>
/// .env dosyasını yükleyip ortam değişkenlerine yazar.
/// Deploy sonrası env var'ları sistemden gelir; yerel geliştirmede .env kullanılır.
/// </summary>
public static class EnvLoader
{
    const string EnvFileName = ".env";

    /// <summary>Oyun başlarken çağrılır. .env varsa KEY=VALUE satırlarını ortam değişkenine yazar.</summary>
    public static void LoadFromFile()
    {
        string content = null;
#if UNITY_EDITOR
        string editorPath = Path.Combine(Application.dataPath, EnvFileName);
        if (File.Exists(editorPath))
        {
            try { content = File.ReadAllText(editorPath); } catch (Exception e) { Debug.LogWarning("[EnvLoader] .env read failed: " + e.Message); }
        }
#endif
        if (string.IsNullOrEmpty(content))
        {
            string streamingPath = Path.Combine(Application.streamingAssetsPath, EnvFileName);
            if (File.Exists(streamingPath))
            {
                try { content = File.ReadAllText(streamingPath); } catch (Exception e) { Debug.LogWarning("[EnvLoader] .env read failed: " + e.Message); }
            }
        }
        if (string.IsNullOrEmpty(content)) return;
        ParseAndSetEnv(content);
    }

    static void ParseAndSetEnv(string content)
    {
        foreach (string line in content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            string trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith("#")) continue;
            int eq = trimmed.IndexOf('=');
            if (eq <= 0) continue;
            string key = trimmed.Substring(0, eq).Trim();
            string value = trimmed.Substring(eq + 1).Trim();
            if (value.StartsWith("\"") && value.EndsWith("\""))
                value = value.Substring(1, value.Length - 2).Replace("\\\"", "\"");
            else if (value.StartsWith("'") && value.EndsWith("'"))
                value = value.Substring(1, value.Length - 2);
            if (string.IsNullOrEmpty(key)) continue;
            try { Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.Process); } catch { }
        }
    }

    /// <summary>Belirtilen key için env var veya fallback döner.</summary>
    public static string Get(string key, string fallback = null)
    {
        string v = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrEmpty(v) ? fallback : v;
    }
}
