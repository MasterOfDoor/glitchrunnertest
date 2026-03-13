using UnityEngine;

/// <summary>
/// Menteşe Köprüsü
/// - Oyuncu trigger zone'a girince otomatik düşer
/// - Oyuncunun TERSİ yönde devrilir
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HingeJoint2D))]
public class HingeBridge : MonoBehaviour
{
    [Header("Fizik")]
    [SerializeField] private float gravityScale = 2f;
    [SerializeField] private float kickTorque   = 5f;   // yön belirleyen başlangıç torku

    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";

    Rigidbody2D  _rb;
    HingeJoint2D _hinge;
    bool         _released;

    void Awake()
    {
        _rb    = GetComponent<Rigidbody2D>();
        _hinge = GetComponent<HingeJoint2D>();

        // Başlangıçta dik ve donmuş
        _rb.constraints  = RigidbodyConstraints2D.FreezeAll;
        _rb.gravityScale = 0f;

        // Limit: sadece bir yöne yatsın
        _hinge.useLimits = true;
        var lim = _hinge.limits;
        lim.min = -90f;
        lim.max =  90f;
        _hinge.limits = lim;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_released) return;
        if (!other.CompareTag(playerTag)) return;

        Release(other.transform.position);
    }

    void Release(Vector2 playerPos)
    {
        _released = true;

        // Oyuncu kalasın sağında mı solunda mı?
        float side = playerPos.x - transform.position.x;

        // Limit: oyuncunun TERSİ yönde yatacak şekilde ayarla
        var lim = _hinge.limits;
        if (side >= 0f)
        {
            // Oyuncu sağda → kalas sola yatsın
            lim.min = -90f;
            lim.max =  0f;
        }
        else
        {
            // Oyuncu solda → kalas sağa yatsın
            lim.min =  0f;
            lim.max =  90f;
        }
        _hinge.limits = lim;

        // Rotasyon kilidini kaldır
        _rb.constraints  = RigidbodyConstraints2D.None;
        _rb.gravityScale = gravityScale;

        // Yön torku — oyuncunun tersine
        float torqueDir = side >= 0f ? 1f : -1f;
        _rb.AddTorque(torqueDir * kickTorque, ForceMode2D.Impulse);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!TryGetComponent<HingeJoint2D>(out var h)) return;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.TransformPoint(h.anchor), 0.12f);
    }
#endif
}