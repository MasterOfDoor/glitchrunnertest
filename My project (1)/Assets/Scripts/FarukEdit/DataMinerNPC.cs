using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DataMinerNPC : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────
    // INSPECTOR
    // ─────────────────────────────────────────────────────────────────────
    [Header("Bağlantılar")]
    [SerializeField] private DialogueBubble bubble;
    [SerializeField] private GameObject     interactPrompt;
    [SerializeField] private Animator       animator;

    [Header("Ayarlar")]
    [SerializeField] private string playerTag     = "Player";
    [SerializeField] private float  greetingDelay = 0.6f;

    [Header("Dialogue — Ana Seri")]
    [TextArea(2, 4)]
    [SerializeField] private List<string> dialogueLines = new List<string>
    {
        "> UYARI: Bu RAM bölgesi kararsız.\n  Bitleri doğru sırala, sistem stabilize olur.",
        "> İPUCU: 0'lar ve 1'ler tesadüf değil.\n  Her şeyin bir dizisi var.",
        "> GEÇİT: Tüm puzzle'ları çöz.\n  Aşağıdaki kilit açılacak.",
        "> UNUTMA: CPU bölgesine girersen\n  geri dönüş yok. Hazır ol."
    };

    [TextArea(2, 3)]
    [SerializeField] private string greetingLine =
        "> DATA MINER_07 burada.\n  Konuşmak için [ E ] bas.";

    [TextArea(2, 3)]
    [SerializeField] private string repeatLine =
        "> Sana söyleyeceklerimi söyledim.\n  Git işini yap, PROCESS.";

    [Header("Puzzle Çözülünce Açılan Dialogue")]
    [SerializeField] private bool         hasBonusDialogue   = false;
    [TextArea(2, 4)]
    [SerializeField] private List<string> bonusDialogueLines = new List<string>
    {
        "> İyi iş. Kapı açıldı.\n  CPU seni bekliyor."
    };

    [Header("Eventler")]
    public UnityEvent OnDialogueStart;
    public UnityEvent OnDialogueEnd;
    public UnityEvent OnAllLinesShown;

    // ─────────────────────────────────────────────────────────────────────
    // PRIVATE STATE
    // ─────────────────────────────────────────────────────────────────────
    private bool      _playerInRange = false;
    private bool      _talking       = false;
    private int       _lineIndex     = 0;
    private bool      _allLinesShown = false;
    private bool      _bonusUnlocked = false;
    private int       _bonusIndex    = 0;
    private Coroutine _greetRoutine;

    // ─────────────────────────────────────────────────────────────────────
    // LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (interactPrompt != null) interactPrompt.SetActive(false);
        if (animator == null)       animator = GetComponent<Animator>();
        if (bubble == null)         bubble   = GetComponentInChildren<DialogueBubble>(true);
    }

    void Update()
    {
        if (!_playerInRange) return;
        if (Input.GetKeyDown(KeyCode.E)) Interact();
    }

    // ─────────────────────────────────────────────────────────────────────
    // TRIGGER
    // ─────────────────────────────────────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        _playerInRange = true;
        if (interactPrompt != null) interactPrompt.SetActive(true);

        if (_greetRoutine != null) StopCoroutine(_greetRoutine);
        _greetRoutine = StartCoroutine(ShowGreetingDelayed());
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        _playerInRange = false;
        _talking       = false;

        if (interactPrompt != null) interactPrompt.SetActive(false);
        if (_greetRoutine  != null) { StopCoroutine(_greetRoutine); _greetRoutine = null; }
        if (bubble         != null) bubble.Hide();
    }

    // ─────────────────────────────────────────────────────────────────────
    // INTERACT
    // ─────────────────────────────────────────────────────────────────────
    void Interact()
    {
        if (bubble == null) return;

        if (_talking && bubble.IsVisible)
        {
            bubble.Hide();
            StartCoroutine(NextLineAfterFrame());
            return;
        }

        ShowNextLine();
    }

    IEnumerator NextLineAfterFrame()
    {
        yield return null;
        ShowNextLine();
    }

    void ShowNextLine()
    {
        OnDialogueStart?.Invoke();
        _talking = true;

        if (_bonusUnlocked && _bonusIndex < bonusDialogueLines.Count)
        {
            bubble.ShowLine(bonusDialogueLines[_bonusIndex++]);
            return;
        }

        if (_lineIndex < dialogueLines.Count)
        {
            bubble.ShowLine(dialogueLines[_lineIndex++]);

            if (_lineIndex >= dialogueLines.Count && !_allLinesShown)
            {
                _allLinesShown = true;
                OnAllLinesShown?.Invoke();
            }
        }
        else
        {
            bubble.ShowLine(repeatLine);
            StartCoroutine(EndTalkAfterBubble());
        }
    }

    IEnumerator EndTalkAfterBubble()
    {
        yield return new WaitUntil(() => !bubble.IsVisible);
        _talking = false;
        OnDialogueEnd?.Invoke();
    }

    IEnumerator ShowGreetingDelayed()
    {
        yield return new WaitForSeconds(greetingDelay);
        if (_playerInRange && bubble != null && !_talking)
            bubble.ShowLine(greetingLine, 2.5f);
    }

    // ─────────────────────────────────────────────────────────────────────
    // PUBLIC API
    // ─────────────────────────────────────────────────────────────────────
    public void UnlockBonusDialogue()
    {
        if (!hasBonusDialogue) return;
        _bonusUnlocked = true;
        _bonusIndex    = 0;
        if (_playerInRange && bubble != null)
            bubble.ShowLine(bonusDialogueLines[0]);
    }

    public void ResetDialogue()
    {
        _lineIndex     = 0;
        _bonusIndex    = 0;
        _allLinesShown = false;
        _bonusUnlocked = false;
    }

    public void SayLine(string line)
    {
        if (bubble != null) bubble.ShowLine(line);
    }

    // ─────────────────────────────────────────────────────────────────────
    // EDITOR
    // ─────────────────────────────────────────────────────────────────────
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        var col = GetComponent<CircleCollider2D>();
        if (col == null) return;
        Gizmos.color = new Color(0f, 1f, 0.53f, 0.25f);
        Gizmos.DrawSphere(transform.position + (Vector3)col.offset, col.radius);
        Gizmos.color = new Color(0f, 1f, 0.53f, 0.8f);
        Gizmos.DrawWireSphere(transform.position + (Vector3)col.offset, col.radius);
    }
#endif
}