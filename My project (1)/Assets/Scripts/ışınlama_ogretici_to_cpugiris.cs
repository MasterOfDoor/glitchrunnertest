using UnityEngine;

/// <summary>
/// Öğretici sahnede bir trigger alanına girip E'ye basınca,
/// aynı objede bulunan SceneTransitionTrigger üzerinden sahne değiştirir.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ışınlama_ogretici_to_cpugiris : MonoBehaviour
{
    [Tooltip("Oyuncu için beklenen tag. Varsayılan: Player")]
    [SerializeField] private string playerTag = "Player";

    private bool _inRange;
    private SceneTransitionTrigger _transition;

    void Awake()
    {
        _transition = GetComponent<SceneTransitionTrigger>();

        // Collider'ı trigger'a çevir (unutulduysa yardımcı olsun)
        var col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
            col.isTrigger = true;
    }

    void Update()
    {
        if (!_inRange) return;
        if (Input.GetKeyDown(KeyCode.E) && _transition != null)
        {
            _transition.Transition();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            _inRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            _inRange = false;
    }
}

