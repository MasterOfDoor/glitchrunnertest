using UnityEngine;

/// <summary>
/// Puzzle açma/kapama ve oyun pauselama işlemlerini yönetir.
/// Sahnenin root'una koy, singleton olarak çalışır.
/// </summary>
public class PuzzleSessionManager : MonoBehaviour
{
    public static PuzzleSessionManager Instance { get; private set; }

    [Header("Bağlantılar")]
    [SerializeField] private GameObject       puzzleRoot;
    [SerializeField] private RAMPuzzleManager puzzleManager;

    [Header("Ayarlar")]
    [SerializeField] private bool pauseGameOnOpen = true;

    // RAMTerminalTrigger tarafından set edilir
    public System.Action OnPuzzleWon;
    public System.Action OnPuzzleLost;

    private bool _isOpen = false;

    // ─────────────────────────────────────────────
    // LIFECYCLE
    // ─────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (puzzleRoot != null)
            puzzleRoot.SetActive(false);
    }

    // ─────────────────────────────────────────────
    // PUBLIC API
    // ─────────────────────────────────────────────
    public void StartPuzzle()
    {
        if (_isOpen) return;
        _isOpen = true;

        if (pauseGameOnOpen)
            Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        DisablePlayerInput();

        if (puzzleRoot != null)
            puzzleRoot.SetActive(true);

        if (puzzleManager != null)
        {
            puzzleManager.OnPuzzleCompleted.AddListener(HandleWin);
            puzzleManager.OnPuzzleFailed.AddListener(HandleLoss);
            puzzleManager.BeginPuzzle();
        }
        else
        {
            Debug.LogError("[PuzzleSession] RAMPuzzleManager atanmamış!");
        }
    }

    public void ClosePuzzle()
    {
        Cleanup();
    }

    // ─────────────────────────────────────────────
    // HANDLERS
    // ─────────────────────────────────────────────
    private void HandleWin()
    {
        Cleanup();
        OnPuzzleWon?.Invoke();
        OnPuzzleWon = null;
    }

    private void HandleLoss()
    {
        Cleanup();
        OnPuzzleLost?.Invoke();
        OnPuzzleLost = null;
    }

    // ─────────────────────────────────────────────
    // CLEANUP
    // ─────────────────────────────────────────────
    private void Cleanup()
    {
        _isOpen = false;

        if (puzzleManager != null)
        {
            puzzleManager.OnPuzzleCompleted.RemoveListener(HandleWin);
            puzzleManager.OnPuzzleFailed.RemoveListener(HandleLoss);
        }

        if (pauseGameOnOpen)
            Time.timeScale = 1f;

        EnablePlayerInput();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = false;

        if (puzzleRoot != null)
            puzzleRoot.SetActive(false);
    }

    // ─────────────────────────────────────────────
    // PLAYER INPUT — kendi sistemine göre düzenle
    // ─────────────────────────────────────────────
    private void DisablePlayerInput()
    {
        // Seçenek A — kendi PlayerController'ın varsa:
        // PlayerController.Instance?.SetInputEnabled(false);

        // Seçenek B — New Input System:
        // var p = FindObjectOfType<UnityEngine.InputSystem.PlayerInput>();
        // if (p != null) p.DeactivateInput();

        // Seçenek C — Rigidbody2D dondur:
        // var rb = FindObjectOfType<Rigidbody2D>();
        // if (rb != null) rb.simulated = false;
    }

    private void EnablePlayerInput()
    {
        // Seçenek A:
        // PlayerController.Instance?.SetInputEnabled(true);

        // Seçenek B:
        // var p = FindObjectOfType<UnityEngine.InputSystem.PlayerInput>();
        // if (p != null) p.ActivateInput();

        // Seçenek C:
        // var rb = FindObjectOfType<Rigidbody2D>();
        // if (rb != null) rb.simulated = true;
    }
}