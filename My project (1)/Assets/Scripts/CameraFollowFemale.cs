using UnityEngine;

public class CameraFollowFemale : MonoBehaviour {
    [Header("Takip Ayarları")]
    public Transform target;        // Takip edilecek karakter (Senin Player objen)
    public float smoothTime = 0.15f; // Takip yumuşaklığı (Daha küçük sayı = Daha hızlı takip)
    public Vector3 offset = new Vector3(0, 0, -10); // Kamera ve karakter arasındaki mesafe

    private Vector3 velocity = Vector3.zero;

    void LateUpdate() {
        // Hedef (karakter) sahnede yoksa hata vermemesi için kontrol ediyoruz
        if (target != null) {
            // Karakterin olduğu yere offset (Z: -10) ekleyerek kameranın durması gereken noktayı buluyoruz
            Vector3 targetPosition = target.position + offset;
            
            // SmoothDamp sayesinde kamera hedefe doğru "yağ gibi" kayarak gider
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}