using UnityEngine;
using TMPro;
using System.Collections;

public class RobotDialogue : MonoBehaviour
{
    public Transform player;
    public float triggerDistance = 3f;

    public GameObject bubbleUI;
    public TMP_Text bubbleText;

    [TextArea]
    public string[] dialogueLines;

    private bool isTalking = false;

    private RobotPatrol patrol;
    private Transform bubbleAnchor;

    void Start()
    {
        bubbleUI.SetActive(false);

        patrol = GetComponent<RobotPatrol>();
        bubbleAnchor = bubbleUI.transform.parent; // BubbleAnchor referansı
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);

        if (dist <= triggerDistance && !isTalking)
        {
            StartCoroutine(StartDialogue());
        }

        // Balonun dönmesini engelle
        if (bubbleAnchor != null)
        {
            bubbleAnchor.rotation = Quaternion.identity;
        }
    }

    IEnumerator StartDialogue()
    {
        isTalking = true;

        // Robotu durdur
        if (patrol != null)
            patrol.isFrozen = true;

        foreach (string line in dialogueLines)
        {
            bubbleText.text = line;
            bubbleUI.SetActive(true);

            float duration = Mathf.Max(1f, line.Length * 0.3f);
            yield return new WaitForSeconds(duration);
        }

        bubbleUI.SetActive(false);
        isTalking = false;

        // Robotu tekrar hareket ettir
        if (patrol != null)
            patrol.isFrozen = false;
    }
}
