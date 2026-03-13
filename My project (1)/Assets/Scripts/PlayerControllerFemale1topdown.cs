using UnityEngine;

public class TopDownPlayerController : MonoBehaviour {
    [Header("Hareket Ayarları")]
    public float moveSpeed = 5f;
    public float dashSpeed = 15f;    // Dash hızı
    public float dashDuration = 0.2f; // Dash süresi
    public float dashCooldown = 1f;   // Dash bekleme süresi

    [Header("Bileşenler")]
    public Rigidbody2D rb;
    public Animator animator;

    Vector2 moveInput;
    bool isDashing;
    bool canDash = true;

    void Update() {
        if (isDashing) return; // Dash yaparken normal hareketi durdur

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Dash Kontrolü (Sol Shift tuşu)
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && moveInput != Vector2.zero) {
            StartCoroutine(Dash());
        }

        // Animator Parametreleri
        if (moveInput.sqrMagnitude > 0) {
            animator.SetFloat("Horizontal", moveInput.x);
            animator.SetFloat("Vertical", moveInput.y);
        }
        animator.SetFloat("Speed", moveInput.sqrMagnitude);
    }

    void FixedUpdate() {
        if (isDashing) return;
        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }

    private System.Collections.IEnumerator Dash() {
        canDash = false;
        isDashing = true;
        
        // Dash yönünü belirle ve hızı uygula
        rb.linearVelocity = moveInput.normalized * dashSpeed;

        yield return new WaitForSeconds(dashDuration);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}