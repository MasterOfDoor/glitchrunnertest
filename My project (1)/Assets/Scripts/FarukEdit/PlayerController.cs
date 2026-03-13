using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Hareket")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Zıplama")]
    [SerializeField] private float     jumpForce         = 10f;
    [SerializeField] private Transform groundCheck;        // ayak altına boş child obje
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float     groundCheckRadius  = 0.1f;

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Animator _anim;

    private bool _isGrounded;
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    void Awake()
    {
        _rb   = GetComponent<Rigidbody2D>();
        _sr   = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();

        _rb.gravityScale = 1f;      // prefabda 0 — burada zorla 1
        _rb.freezeRotation = true;
    }

    void Update()
    {
        // Zemin kontrolü
        _isGrounded = groundCheck != null
            && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Zıplama — Update'te oku (FixedUpdate'te kaçabilir)
        if (_isGrounded && (Input.GetKeyDown(KeyCode.Space)
                         || Input.GetKeyDown(KeyCode.W)
                         || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
        }
    }

    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");

        _rb.linearVelocity = new Vector2(h * moveSpeed, _rb.linearVelocity.y);

        // Sprite yönü
        if (_sr != null && h != 0f)
            _sr.flipX = h < 0f;

        // Animator
        if (_anim != null)
            _anim.SetFloat(SpeedHash, Mathf.Abs(h));
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
#endif
}
