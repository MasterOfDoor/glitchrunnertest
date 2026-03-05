using UnityEngine;

public class RobotController : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    public float speed = 5f;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. Hareket Girdisi Al
        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        // 2. Yürüme Animasyonunu Tetikle (isWalking parametresi)
        bool isWalking = Mathf.Abs(moveInput) > 0.1f;
        anim.SetBool("isWalking", isWalking);

        // 3. Robotun Yönünü Çevir (Sağa/Sola)
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);

        // Test İçin: H tuşuna basınca hasar alma (Hurt)
        if (Input.GetKeyDown(KeyCode.H)) {
            anim.SetTrigger("doHurt");
        }
    }

    // Bu fonksiyonu başka bir yerden (Örn: mermi çarpınca) çağırabilirsin
    public void Die() {
        anim.SetBool("isDead", true);
        speed = 0; // Öldüğünde durması için
    }
}