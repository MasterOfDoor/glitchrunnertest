using UnityEngine;

/// <summary>
/// Kameranın sahne dışını göstermesini engeller. Kamerayı seç, bu scripti ekle,
/// sahnenin içinde kalmasını istediğin alanı Min/Max X ve Y ile belirle.
/// </summary>
public class CameraBounds : MonoBehaviour
{
    [Header("Sahne sınırları (kamera merkezinin gidebileceği min/max)")]
    [Tooltip("Kameranın sol sınırı (X). Duvarın iç tarafına denk gelecek değeri yaz.")]
    public float minX = -100f;
    [Tooltip("Kameranın sağ sınırı (X).")]
    public float maxX = 100f;
    [Tooltip("Kameranın alt sınırı (Y).")]
    public float minY = -100f;
    [Tooltip("Kameranın üst sınırı (Y).")]
    public float maxY = 100f;

    [Header("İsteğe bağlı: Ortographic Size'a göre otomatik daralt")]
    [Tooltip("Açıksa kamera boyutuna göre sınırları içe çeker (kenarda siyah çıkmaması için).")]
    public bool useCameraSize = true;

    private Camera _cam;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam == null) _cam = Camera.main;
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;

        if (useCameraSize && _cam != null && _cam.orthographic)
        {
            float halfHeight = _cam.orthographicSize;
            float halfWidth = halfHeight * _cam.aspect;
            float clampMinX = minX + halfWidth;
            float clampMaxX = maxX - halfWidth;
            float clampMinY = minY + halfHeight;
            float clampMaxY = maxY - halfHeight;
            pos.x = Mathf.Clamp(pos.x, clampMinX, clampMaxX);
            pos.y = Mathf.Clamp(pos.y, clampMinY, clampMaxY);
        }
        else
        {
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
        }

        transform.position = pos;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
        Vector3 center = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, 0f);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0.1f);
        Gizmos.DrawCube(center, size);
    }
#endif
}
