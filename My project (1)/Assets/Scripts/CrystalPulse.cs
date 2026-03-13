using UnityEngine;
using UnityEngine.Rendering.Universal; // 2D ışıkları kodla kontrol etmek için bu kütüphane şart!

public class CrystalPulse : MonoBehaviour
{
    private Light2D pointLight;
    
    [Header("Işık Ayarları")]
    public float minIntensity = 0.5f; // Işığın ineceği en sönük an
    public float maxIntensity = 2.0f; // Işığın çıkacağı en parlak an
    public float pulseSpeed = 3.0f;   // Yanıp sönme hızı

    void Start()
    {
        // Objenin üzerindeki Işık bileşenini yakalıyoruz
        pointLight = GetComponent<Light2D>();
    }

    void Update()
    {
        // Zamanı sinüs dalgasına sokup -1 ile 1 arasında bir değer alıyoruz.
        // Sonra bunu 0 ile 1 arasına çekip (t değeri), min ve max parlaklık arasında yumuşakça gezdiriyoruz.
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f; 
        
        pointLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
    }
}
