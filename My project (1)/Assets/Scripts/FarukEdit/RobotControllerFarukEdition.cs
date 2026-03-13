using UnityEngine;

public class RobotPatrol : MonoBehaviour
{
    public float speed = 2f;
    public float moveDistance = 3f;

    public Transform spriteVisual; // SADECE sprite
    private Vector3 startPos;
    private int direction = 1;

    [HideInInspector] public bool isFrozen = false;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if (isFrozen) return;

        transform.position += Vector3.right * direction * speed * Time.deltaTime;

        if (Vector3.Distance(startPos, transform.position) >= moveDistance)
        {
            direction *= -1;

            // SADECE sprite flip
            spriteVisual.localScale = new Vector3(
                Mathf.Abs(spriteVisual.localScale.x) * direction,
                spriteVisual.localScale.y,
                spriteVisual.localScale.z
            );
        }
    }
}
