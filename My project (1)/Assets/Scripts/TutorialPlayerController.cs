using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// TutorialPlayer prefab kontrolü: Input System (Move, Jump), Rigidbody2D velocity,
/// sprite yönü (flipX) ve Animator "Speed" parametresi.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class TutorialPlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Jump (optional)")]
    [SerializeField] private bool enableJump = false;
    [SerializeField] private float jumpForce = 8f;

    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private InputActionMap _playerMap;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        if (inputActions == null)
            return;

        _playerMap = inputActions.FindActionMap("Player");
        if (_playerMap != null)
        {
            _moveAction = _playerMap.FindAction("Move");
            _jumpAction = _playerMap.FindAction("Jump");
        }
    }

    private void OnEnable()
    {
        if (inputActions != null)
            inputActions.Enable();
        if (_playerMap != null)
            _playerMap.Enable();
    }

    private void OnDisable()
    {
        if (_playerMap != null)
            _playerMap.Disable();
        if (inputActions != null)
            inputActions.Disable();
    }

    private void FixedUpdate()
    {
        float horizontal = 0f;

        if (_moveAction != null)
            horizontal = _moveAction.ReadValue<Vector2>().x;
        else
            horizontal = Input.GetAxisRaw("Horizontal");

        if (_rb == null)
            return;

        float targetVelX = horizontal * moveSpeed;
        float velY = _rb.linearVelocity.y;

        if (enableJump && _jumpAction != null && _jumpAction.WasPressedThisFrame())
        {
            velY = jumpForce;
            if (_rb.gravityScale == 0f)
                _rb.gravityScale = 1f;
        }

        _rb.linearVelocity = new Vector2(targetVelX, velY);

        if (_spriteRenderer != null && horizontal != 0f)
            _spriteRenderer.flipX = horizontal < 0f;

        if (_animator != null)
            _animator.SetFloat(SpeedHash, Mathf.Abs(horizontal));
    }
}
