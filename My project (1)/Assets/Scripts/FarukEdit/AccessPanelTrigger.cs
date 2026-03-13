using UnityEngine;

/// BoxCollider2D (IsTrigger: ON) olan herhangi bir objeye ekle.
public class AccessPanelTrigger : MonoBehaviour
{
    [SerializeField] private AccessPanel panel;
    [SerializeField] private string      playerTag = "Player";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            panel.Dismiss();
    }
}
