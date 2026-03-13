using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Takip Edilecek Obje")]
    public Transform target; // Buraya karakterini sürükleyeceğiz

    [Header("Takip Ayarları")]
    public float smoothSpeed = 0.125f; // Kamera ne kadar yumuşak gelsin? (0-1 arası)
    public Vector3 offset; // Kameranın karakterden uzaklığı

    void LateUpdate()
    {
        if (target != null)
        {
            // Kameranın gitmesi gereken hedef pozisyon
            Vector3 desiredPosition = target.position + offset;
            
            // Mevcut pozisyondan hedef pozisyona yumuşak geçiş yap
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            
            // Kamerayı yeni pozisyona taşı
            transform.position = smoothedPosition;
        }
    }
}