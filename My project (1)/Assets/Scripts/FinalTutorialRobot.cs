using UnityEngine;

/// <summary>
/// Öğretici sahne robotu: Önce oyuncuya yaklaşır, RobotLabel'daki cümleler bitince (E ile) escapePoint'e gidip yok olur.
/// Robota RobotLabel ekleyip Cümleler listesine + ile istediğin cümleleri yaz. Escape Point'i Inspector'dan atamayı unutma.
/// </summary>
public class FinalTutorialRobot : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform player;
    [Tooltip("Robot bu noktaya gidip yok olur. Inspector'dan mutlaka ata!")]
    public Transform escapePoint;
    public Animator anim;

    [Header("Hareket")]
    [Tooltip("İşaretlersen robot yerinde kalır, sen gelip E ile etkileşirsin. İşaretlemezsen robot sana doğru yürür (varsayılan).")]
    public bool stayInPlace = false;
    public float triggerDistance = 3f;
    public float moveSpeed = 3f;
    public float runSpeed = 8f;

    private RobotLabel _robotLabel;
    private Rigidbody2D _rb;
    private bool _isEscaping;
    private float _nextFindPlayerTime;
    private bool _hasLoggedPlayerNull;

    void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _robotLabel = GetComponentInChildren<RobotLabel>(true);
        _isEscaping = false;
        TryFindPlayer();
        _nextFindPlayerTime = Time.time + 1f;
        if (escapePoint == null)
            Debug.LogWarning("FinalTutorialRobot: Escape Point atanmamış! Robota kaçış olmayacak — Inspector'dan Escape Point sürükle.");
    }

    void TryFindPlayer()
    {
        if (player != null) return;
        var p = GameObject.FindWithTag("Player");
        if (p != null) { player = p.transform; return; }
        var asil = FindObjectOfType<AsılScript>();
        if (asil != null) { player = asil.transform; return; }
        var female = FindObjectOfType<PlayerControllerFemale>();
        if (female != null) player = female.transform;
    }

    void Update()
    {
        if (player == null)
        {
            if (Time.time >= _nextFindPlayerTime)
            {
                TryFindPlayer();
                _nextFindPlayerTime = Time.time + 1f;
                if (player == null)
                {
                    if (!_hasLoggedPlayerNull) { _hasLoggedPlayerNull = true; Debug.LogWarning("FinalTutorialRobot: Oyuncu bulunamadı. Karaktere 'Player' tag'i veya AsılScript/PlayerControllerFemale ekleyin."); }
                }
            }
            return;
        }

        if (anim != null)
        {
            bool moving = _isEscaping;
            if (!stayInPlace && !_isEscaping)
                moving = _robotLabel != null && !_robotLabel.IsDialogueFinished && Vector3.Distance(player.position, transform.position) > triggerDistance;
            anim.SetBool("isWalking", moving);
        }

        if (_isEscaping && escapePoint != null && Vector3.Distance(transform.position, escapePoint.position) < 0.5f)
            Destroy(gameObject);

        if (_isEscaping) return;

        // Diyalog bitti mi? (Son cümleden sonra E'ye basıldı) — oyuncu menzilde olmasa da kaç
        if (_robotLabel != null && _robotLabel.IsDialogueFinished && escapePoint != null)
            _isEscaping = true;
    }

    void FixedUpdate()
    {
        if (player == null)
        {
            if (Time.time >= _nextFindPlayerTime) TryFindPlayer();
            return;
        }
        if (_isEscaping)
        {
            if (escapePoint == null) return;
            MoveTo(escapePoint.position, runSpeed);
            return;
        }
        if (!_isEscaping && !stayInPlace)
        {
            float dist = Vector3.Distance(player.position, transform.position);
            if (dist > triggerDistance)
                MoveTo(player.position, moveSpeed);
        }
    }

    void MoveTo(Vector3 target, float speed)
    {
        float dt = Time.fixedDeltaTime;
        Vector3 newPos = Vector3.MoveTowards(transform.position, target, speed * dt);
        if (_rb != null)
            _rb.MovePosition(new Vector2(newPos.x, newPos.y));
        else
            transform.position = newPos;

        if (target.x > transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);
        else if (target.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);
    }
}
