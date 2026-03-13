using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GlitchrunnerIntro : MonoBehaviour
{
    [Header("Giriş Ayarları")]
    public Sprite[] introSprites; // 4 görseli buraya sürükle
    public Image displayImage;    // UI Image bileşenini buraya sürükle
    public float fadeDuration = 0.8f; // Geçiş hızı (saniye)
    
    [Header("Sahne Geçişi")]
    public string nextSceneName; // Intro bitince açılacak sahnenin adı

    private int currentIndex = 0;
    private bool isTransitioning = false;

    void Start()
    {
        // İlk görseli göstererek başla
        if (introSprites.Length > 0 && displayImage != null)
        {
            displayImage.sprite = introSprites[0];
            // Resmin şeffaf kalmaması için rengini full beyaz yap
            displayImage.color = Color.white; 
        }
    }

    void Update()
    {
        // Mouse sol tık, Space veya Enter tuşuyla ilerle
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) && !isTransitioning)
        {
            AdvanceIntro();
        }
    }

    void AdvanceIntro()
    {
        currentIndex++;
        
        if (currentIndex < introSprites.Length)
        {
            // Bir sonraki görsele yumuşak geçiş yap
            StartCoroutine(FadeToNextSprite(introSprites[currentIndex]));
        }
        else
        {
            // Bütün görseller bitti, ana oyuna geç
            Debug.Log("Intro bitti, sahne yükleniyor: " + nextSceneName);
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }

    IEnumerator FadeToNextSprite(Sprite nextSprite)
    {
        isTransitioning = true;
        
        float timer = 0;
        // 1. Kararma (Fade Out)
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = 1 - (timer / fadeDuration);
            displayImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        displayImage.sprite = nextSprite;

        // 2. Geri Açılma (Fade In)
        timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = timer / fadeDuration;
            displayImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        isTransitioning = false;
    }
}