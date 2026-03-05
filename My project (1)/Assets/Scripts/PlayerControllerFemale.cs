using UnityEngine;
using System.Collections;

public class PlayerControllerFemale : MonoBehaviour {
    [Header("Hareket Ayarları")]
    public float moveSpeed = 5f;
    public float dashSpeed = 15f; 
    public float dashDuration = 0.2f; 
    public float jumpForce = 12f; // Zıplama gücü artık çalışacak!

    [Header("Zemin Kontrolü")]
    public Transform groundCheck; 
    public float checkRadius = 0.2f; 
    public LayerMask groundLayer; 

    private bool isDashing = false;
    private bool isDead = false;
    private bool isGrounded; 

    [Header("Bileşenler")]
    public Rigidbody2D rb;
    public Animator animator;

    // Artık sadece X ekseninde hareket alacağımız için Vector2 yerine float kullanıyoruz
    float moveInput; 

    void Update() {
        if (isDead) return;
        if (isDashing) return;

        // Yere değme kontrolü
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // SADECE SAĞA SOLA HAREKET (Y eksenini sildik)
        moveInput = Input.GetAxisRaw("Horizontal");

        // Animator Parametreleri
        if (moveInput != 0) {
            animator.SetFloat("Horizontal", moveInput);
            // Karakterin yönünü döndürmek için basit bir kontrol eklenebilir
        }
        animator.SetFloat("Speed", Mathf.Abs(moveInput));

        // ZIPLAMA
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) {
            Jump();
        }

        // DASH
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing) {
            StartCoroutine(Dash());
        }
    }

    void FixedUpdate() {
        if (isDashing) return;
        
        // İŞTE ÇÖZÜM BURASI!
        // X ekseninde bizim hızımız (moveInput * moveSpeed), Y ekseninde ise Unity'nin kendi fiziği (rb.linearVelocity.y) çalışsın diyoruz.
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    void Jump() {
        animator.SetTrigger("doJump"); 
        // Y hızını sıfırlayıp öyle zıplama gücü ekliyoruz ki hep aynı yükseğe zıplasın
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); 
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    IEnumerator Dash() {
        isDashing = true;
        animator.SetTrigger("doDash"); 

        // Dash yönünü belirle
        float dashDirection = (moveInput == 0) ? transform.localScale.x : moveInput;
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0); // Dash atarken yerçekimini anlık yoksay

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector2.zero;
        isDashing = false;
    }
    // Bu metodu FixedUpdate veya Dash gibi diğer metodların bittiği yere, sınıfın içine ekle
    private void OnTriggerEnter2D(Collider2D other) 
    {
        // Karakter boşluğa düşerse ve zaten ölmemişse
        if (other.CompareTag("FallArea") && !isDead) 
        {
            StartCoroutine(DieAndRespawn());
        }
    }

    IEnumerator DieAndRespawn()
    {
        isDead = true; // Karakterin öldüğünü sisteme söyle (hareket kilitlenir)
        
        // Karakterin yerçekimini ve fiziğini geçici olarak kapat ki ekrandan kayıp gitmesin
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false; 

        // 1. Ölüm animasyonunu oynat! 
        // (Animator içinde ölüm Trigger'ının adını "doDie" yaptığını varsayıyorum)
        animator.SetTrigger("doDie"); 

        // 2. 1.5 Saniye bekle
        yield return new WaitForSeconds(1.5f);

        // 3. Karakteri SpawnPoint'e ışınla
        transform.position = GameObject.Find("SpawnPoint").transform.position;
        
        // 4. Fiziği geri aç ve karakteri canlandır
        rb.simulated = true;
        isDead = false;
        
        // İsteğe bağlı: Canlanınca direkt durma animasyonuna geçsin dersen
        // animator.Play("Idle"); 
    }
}