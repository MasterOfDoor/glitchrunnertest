using UnityEngine;

/// <summary>
/// Merdiven tırmanma sistemi — PlayerController.cs ile uyumlu.
/// 
/// KURULUM:
/// 1. Merdiven objesine BoxCollider2D (IsTrigger: ON) ekle
/// 2. Bu scripti merdiven objesine ekle
/// 3. Merdiven objesine "Ladder" tag'i ver (opsiyonel, debug için)
/// 4. Inspector'dan playerTag ayarla ("Player")
/// </summary>
public class LadderClimb : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private string playerTag    = "Player";
    [SerializeField] private float  climbSpeed   = 3f;
    [SerializeField] private float  exitJumpForce = 5f; // üstten çıkışta hafif zıplama

    // Merdiven sınırları (otomatik hesaplanır)
    private float _topY;
    private float _bottomY;

    // Aktif oyuncu referansı
    private Rigidbody2D      _playerRb;
    private PlayerController _playerCtrl;
    private bool             _climbing;

    void Awake()
    {
        // Collider'dan üst/alt Y sınırlarını hesapla
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            _topY    = col.bounds.max.y;
            _bottomY = col.bounds.min.y;
        }
    }

    // ── Oyuncu girer ──────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        _playerRb   = other.GetComponent<Rigidbody2D>();
        _playerCtrl = other.GetComponent<PlayerController>();
    }

    // ── Oyuncu çıkar ──────────────────────────────────────
    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        StopClimbing();
    }

    // ── Her frame ─────────────────────────────────────────
    void Update()
    {
        if (_playerRb == null) return;

        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");

        // Tırmanmaya başla: trigger zone'dayken W veya S'e bas
        if (!_climbing && Mathf.Abs(v) > 0.1f)
            StartClimbing();

        if (!_climbing) return;

        // ── Tırmanma hareketi ──
        Vector2 vel = new Vector2(h * climbSpeed, v * climbSpeed);
        _playerRb.linearVelocity = vel;

        // ── Üst sınır: merdivenin tepesine ulaşınca çık ──
        if (_playerRb.position.y >= _topY)
        {
            _playerRb.position = new Vector2(_playerRb.position.x, _topY);
            StopClimbing();
            return;
        }

        // ── Alt sınır: aşağı inince merdivenle bağı kes ──
        if (_playerRb.position.y <= _bottomY && v < 0f)
        {
            StopClimbing();
            return;
        }

        // ── Zıplayarak çıkış ──
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var rb = _playerRb;   // StopClimbing null yapmadan önce sakla
            StopClimbing();
            if (rb != null)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, exitJumpForce);
        }
    }

    // ── Tırmanmayı başlat ─────────────────────────────────
    void StartClimbing()
    {
        if (_playerRb == null) return;
        _climbing = true;

        // PlayerController'ı devre dışı bırak (çakışmasın)
        if (_playerCtrl != null) _playerCtrl.enabled = false;

        // Gravity'i kapat
        _playerRb.gravityScale = 0f;
        _playerRb.linearVelocity = Vector2.zero;
    }

    // ── Tırmanmayı durdur ─────────────────────────────────
    void StopClimbing()
    {
        _climbing = false;

        if (_playerRb != null)
        {
            // Gravity'yi geri aç (PlayerController'daki değere göre)
            _playerRb.gravityScale = 1f;
        }

        // PlayerController'ı geri aç
        if (_playerCtrl != null) _playerCtrl.enabled = true;

        _playerRb   = null;
        _playerCtrl = null;
    }
}