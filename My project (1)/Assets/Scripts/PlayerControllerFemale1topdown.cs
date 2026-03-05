using UnityEngine;

public class TopDownPlayerController : MonoBehaviour {
    [Header("Hareket Ayarları")]
    public float moveSpeed = 5f;

    [Header("Bileşenler")]
    public Rigidbody2D rb;
    public Animator animator;

    Vector2 moveInput;

    void Update() {
        // 1. Girişleri alıyoruz (W-A-S-D veya Ok Tuşları)
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // 2. Yön Değiştirme (Karakterin sağa/sola bakması)
        if (moveInput.x > 0) {
            transform.localScale = new Vector3(1, 1, 1); // Sağa bak
        } 
        else if (moveInput.x < 0) {
            transform.localScale = new Vector3(-1, 1, 1); // Sola bak
        }

        // 3. Animator Parametreleri
        if (moveInput != Vector2.zero) {
            animator.SetFloat("Horizontal", moveInput.x);
            animator.SetFloat("Vertical", moveInput.y);
        }
        // Hareket edip etmediğimizi Speed parametresine gönderiyoruz
        animator.SetFloat("Speed", moveInput.sqrMagnitude);
    }

    void FixedUpdate() {
        // Fiziksel hareket (Hem X hem Y ekseni)
        // Karakter çapraz giderken hızlanmasın diye .normalized kullanıyoruz
        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }
}