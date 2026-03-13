using System.Collections;
using UnityEngine;

public class TeleportZone : MonoBehaviour
{
    [SerializeField] private Transform  destination;
    [SerializeField] private string     playerTag   = "Player";
    [SerializeField] private float      fadeDuration = 0.4f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            StartCoroutine(Teleport(other.transform));
    }

    IEnumerator Teleport(Transform player)
    {
        // Ekran kara
        if (SceneFader.Instance != null)
        {
            bool done = false;
            SceneFader.Instance.FadeOut(() => done = true);
            yield return new WaitUntil(() => done);
        }

        // Teleport
        player.position = destination.position;

        // Ekran geri gel
        if (SceneFader.Instance != null)
            SceneFader.Instance.FadeIn();
    }
}
