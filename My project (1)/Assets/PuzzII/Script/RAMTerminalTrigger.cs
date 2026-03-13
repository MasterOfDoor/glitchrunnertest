using UnityEngine;

public class TriggerTest : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("TriggerTest HIT: " + other.name);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("CollisionTest HIT: " + other.gameObject.name);
    }
}
/// <summary>
/// RAM terminaline yerleştir. Player collider'a girince puzzle açılır.
///
/// SETUP:
///   1. Terminal objesine bu script'i ekle.
///   2. Collider2D ekle → Is Trigger: ON
///   3. Inspector'dan istersen "E tuşuna bas" interaction modunu seç.
///   4. playerTag = Player karakterinin tag'i (default "Player")
/// </summary>
public class RAMTerminalTrigger : MonoBehaviour
{
    [Header("Bağlantılar")]
    [Tooltip("Sahnedeki PuzzleSessionManager")]
    [SerializeField] private PuzzleSessionManager sessionManager;

    [Header("Interaction Modu")]
    [Tooltip("True = range'e girince otomatik açılır / False = E tuşuna basınca açılır")]
    [SerializeField] private bool autoTrigger = false;

    [Header("Ayarlar")]
    [SerializeField] private string playerTag      = "Player";
    [Tooltip("Puzzle bittikten sonra tekrar tetiklenebilir mi?")]
    [SerializeField] private bool   repeatable     = false;
    [Tooltip("E tuşu modu: range'e girince gösterilecek UI prompt (opsiyonel)")]
    [SerializeField] private GameObject interactPrompt;

    // ── Sonuç callback'leri — Inspector'dan atayabilirsin ────────────────
    [Header("Sonuç Eventleri")]
    public UnityEngine.Events.UnityEvent OnPuzzleWon;
    public UnityEngine.Events.UnityEvent OnPuzzleLost;

    // ── Private ──────────────────────────────────────────────────────────
    private bool _playerInRange = false;
    private bool _used          = false;

    // ─────────────────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        // SessionManager bulunamazsa sahnede ara
        if (sessionManager == null)
            sessionManager = FindObjectOfType<PuzzleSessionManager>();

        // Prompt başta kapalı
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    private void Update()
    {
        // E tuşu modu
        if (!autoTrigger && _playerInRange && Input.GetKeyDown(KeyCode.E))
            TryOpen();
    }

    // ─────────────────────────────────────────────────────────────────────
    // 2D TRIGGER
    // ─────────────────────────────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        _playerInRange = true;

        if (autoTrigger)
        {
            TryOpen();
        }
        else
        {
            // "E tuşuna bas" prompt'unu göster
            if (interactPrompt != null)
                interactPrompt.SetActive(true);
        }

        Debug.Log($"Trigger girdi: {other.gameObject.name} | Tag: {other.tag}");
    
        if (!other.CompareTag(playerTag)) return;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        _playerInRange = false;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────────
    // PUZZLE AÇ
    // ─────────────────────────────────────────────────────────────────────
    private void TryOpen()
    {
        if (_used && !repeatable) return;
        if (sessionManager == null)
        {
            Debug.LogError("[RAMTerminal] PuzzleSessionManager bulunamadı!");
            return;
        }

        _used = true;

        // Prompt'u kapat
        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        // Sonuç callback'lerini bağla
        sessionManager.OnPuzzleWon  = HandleWin;
        sessionManager.OnPuzzleLost = HandleLoss;

        sessionManager.StartPuzzle();
    }

    // ─────────────────────────────────────────────────────────────────────
    // SONUÇLAR
    // ─────────────────────────────────────────────────────────────────────
    private void HandleWin()
    {
        Debug.Log($"[RAMTerminal] {gameObject.name} — PUZZLE KAZANILDI");
        OnPuzzleWon?.Invoke();

        // repeatable değilse terminal'i devre dışı bırak
        if (!repeatable)
            GetComponent<Collider2D>().enabled = false;
    }

    private void HandleLoss()
    {
        Debug.Log($"[RAMTerminal] {gameObject.name} — PUZZLE KAYBEDİLDİ");
        OnPuzzleLost?.Invoke();

        // Kaybedince tekrar denenebilir
        _used = false;
    }
}