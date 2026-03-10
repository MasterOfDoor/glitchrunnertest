using UnityEngine;
using System.Collections;

/// <summary>
/// Spawn sonrası çıkacak ikinci robot: Oyuncuya gelir, RobotLabel ile konuşur (E ile),
/// konuşma bitince FallTarget'a gider, FallArea'ya düşünce ölüm animasyonu oynar ve yok olur (bir daha gelmez).
/// Bu objeyi başta inactive yap; EscapePointRespawn spawn sonrası aktif edecek.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SecondTutorialRobot : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform player;
    [Tooltip("Robot konuşma bitince bu noktaya gider (FallArea kenarı).")]
    public Transform fallTarget;
    public Animator anim;

    [Header("Hareket")]
    [Tooltip("İşaretlersen robot yerinde kalır, sen gelip E ile etkileşirsin. İşaretlemezsen robot sana doğru yürür (varsayılan).")]
    public bool stayInPlace = false;
    public float triggerDistance = 3f;
    public float moveSpeed = 3f;
    public float runSpeed = 6f;
    public float deathAnimDuration = 1.2f;

    [Header("FallArea")]
    [Tooltip("FallArea objesinin tag'i.")]
    public string fallAreaTag = "FallArea";

    private RobotLabel _robotLabel;
    private Rigidbody2D _rb;
    private enum State { Approaching, Talking, GoingToFall, Dead }
    private State _state = State.Approaching;
    private bool _hasTriggeredDeath;

    void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _robotLabel = GetComponentInChildren<RobotLabel>(true);
        if (player == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
            if (player == null)
            {
                var asil = FindObjectOfType<AsılScript>();
                if (asil != null) player = asil.transform;
                if (player == null)
                {
                    var female = FindObjectOfType<PlayerControllerFemale>();
                    if (female != null) player = female.transform;
                }
            }
        }
    }

    void OnEnable()
    {
        _state = State.Approaching;
        _hasTriggeredDeath = false;
    }

    void Update()
    {
        if (player == null) return;

        if (anim != null)
            anim.SetBool("isWalking", (_state == State.Approaching && !stayInPlace) || _state == State.GoingToFall);

        if (_state == State.Dead) return;

        if (_state == State.Talking)
        {
            if (_robotLabel != null && _robotLabel.IsDialogueFinished && fallTarget != null)
                _state = State.GoingToFall;
            return;
        }

        if (_state == State.Approaching)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= triggerDistance && _robotLabel != null)
                _state = State.Talking;
            return;
        }

        if (_state == State.GoingToFall && fallTarget != null)
        {
            if (Vector3.Distance(transform.position, fallTarget.position) < 0.5f)
                PlayDeathAndDestroy();
        }
    }

    void FixedUpdate()
    {
        if (_state == State.Dead || _state == State.Talking) return;
        if (player == null) return;
        if (_state == State.GoingToFall && fallTarget != null)
        {
            MoveTo(fallTarget.position, runSpeed);
            return;
        }
        if (!stayInPlace && _state == State.Approaching)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > triggerDistance)
                MoveTo(player.position, moveSpeed);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasTriggeredDeath) return;
        if (!other.CompareTag(fallAreaTag)) return;
        PlayDeathAndDestroy();
    }

    void PlayDeathAndDestroy()
    {
        if (_hasTriggeredDeath) return;
        _hasTriggeredDeath = true;
        _state = State.Dead;
        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        if (_rb != null) _rb.linearVelocity = Vector2.zero;
        if (anim != null)
        {
            anim.SetBool("isDead", true);
            anim.SetTrigger("doDie");
        }
        yield return new WaitForSeconds(deathAnimDuration);
        Destroy(gameObject);
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
