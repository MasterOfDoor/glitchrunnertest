using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sahne geçişi tetikler. Buton OnClick veya collider trigger ile kullanılabilir.
/// SceneFader varsa FadeOut → LoadScene → FadeIn; yoksa doğrudan LoadScene.
/// </summary>
public class SceneTransitionTrigger : MonoBehaviour
{
    [Tooltip("Yüklenecek sahne adı (Build Settings'ta olmalı). Örn: CpuBazaar, Cpu giriş, MotherBoard")]
    public string targetSceneName = "CpuBazaar";

    /// <summary>Buton veya kod ile çağrılır.</summary>
    public void Transition()
    {
        if (string.IsNullOrEmpty(targetSceneName)) return;
        if (SceneFader.Instance != null)
            SceneFader.Instance.TransitionToScene(targetSceneName);
        else
            SceneManager.LoadScene(targetSceneName);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            Transition();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Transition();
    }
}
