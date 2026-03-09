using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Kolon collider'ına girildiğinde Puzzle prefabını açar.
/// Kazanılırsa → winTarget'a ışınla, kolon kalıcı devre dışı.
/// Kaybedilirse → lossTarget'a ışınla, tekrar denenebilir.
/// </summary>
public class KolonTrigger : MonoBehaviour
{
    [Header("Bağlantılar")]
    [SerializeField] private string        playerTag   = "Player";
    [SerializeField] private PuzzleManager puzzleObject;

    [Header("Işınlanma Noktaları")]
    [SerializeField] private Transform winTarget;    // Kazanınca buraya
    [SerializeField] private Transform lossTarget;   // Kaybedince buraya

    [Header("Eventler")]
    public UnityEvent OnPuzzleOpened;
    public UnityEvent OnPuzzleWon;
    public UnityEvent OnPuzzleLost;

    private bool _solved    = false;
    private bool _triggered = false;

    // ─────────────────────────────────────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (_triggered) return;
        if (_solved)    return;   // Kalıcı kapalı

        _triggered = true;
        OpenPuzzle();
    }

    // ─────────────────────────────────────────────────────────────────────
    void OpenPuzzle()
    {
        if (puzzleObject == null)
        {
            Debug.LogWarning($"[KolonTrigger] {name}: puzzleObject atanmamış!");
            return;
        }

        puzzleObject.OnPuzzleCompleted.RemoveAllListeners();
        puzzleObject.OnPuzzleFailed.RemoveAllListeners();
        puzzleObject.OnPuzzleCompleted.AddListener(HandleWin);
        puzzleObject.OnPuzzleFailed.AddListener(HandleLoss);

        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.FadeOut(() =>
            {
                StartCoroutine(ActivateAndBegin());
                SceneFader.Instance.FadeIn();
            });
        }
        else
        {
            StartCoroutine(ActivateAndBegin());
        }

        OnPuzzleOpened?.Invoke();
    }

    // SetActive(true) sonrası bir frame bekle — PuzzleManager coroutine başlatabilsin
    IEnumerator ActivateAndBegin()
    {
        puzzleObject.transform.root.gameObject.SetActive(true);
        yield return null;   // bir frame bekle
        puzzleObject.BeginPuzzle();
    }

    // ─────────────────────────────────────────────────────────────────────
    void HandleWin()
    {
        _solved    = true;    // Kalıcı kapalı — bir daha açılmaz
        _triggered = false;

        puzzleObject.ClosePuzzle();
        puzzleObject.transform.root.gameObject.SetActive(false);

        OnPuzzleWon?.Invoke();

        if (SceneFader.Instance != null)
            SceneFader.Instance.FadeOut(() => { Teleport(winTarget); SceneFader.Instance.FadeIn(); });
        else
            Teleport(winTarget);
    }

    void HandleLoss()
    {
        _triggered = false;   // Tekrar denenebilir

        puzzleObject.transform.root.gameObject.SetActive(false);

        OnPuzzleLost?.Invoke();

        if (SceneFader.Instance != null)
            SceneFader.Instance.FadeOut(() => { Teleport(lossTarget); SceneFader.Instance.FadeIn(); });
        else
            Teleport(lossTarget);
    }

    // ─────────────────────────────────────────────────────────────────────
    void Teleport(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning($"[KolonTrigger] {name}: hedef nokta atanmamış!");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
            player.transform.position = target.position;
    }
}