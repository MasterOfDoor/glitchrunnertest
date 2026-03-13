using UnityEngine;
using System.Collections;

public class PlayerControllerFemale : MonoBehaviour {
    [Header("Hareket Ayarları")]
    public float moveSpeed = 5f;
    public float dashSpeed = 15f; 
    public float dashDuration = 0.2f; 
    public float jumpForce = 12f;

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

    float moveInput; 

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 1. ADIM: Yer çekimi kontrolünü her şeyden önce yapıyoruz
        // Eğer karakter 3. sahne bölgesindeyse (X > 115) yer çekimini kapat
       // Sadece 3. sahne bölgesindeyken (X > 115) yer çekimini kapatıyoruz.
    // Diğer yerlerde yer çekimi kaçsa (Inspector'dan ne ayarladıysan) öyle kalır.
    if (transform.position.x > 47) 
    {
        rb.gravityScale = 0; 
    }
    else if (transform.position.x <= 47 && rb.gravityScale == 0)
    {
        rb.gravityScale = 1; 
    }

        // 2. ADIM: Ölüm veya Dash sırasında diğer hareketleri engelle
        if (isDead || isDashing) return;

        // --- TOP-DOWN (3. SAHNE) HAREKETİ ---
        if (rb.gravityScale == 0) 
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            
            rb.linearVelocity = new Vector2(moveX, moveY).normalized * moveSpeed;

            if (moveX != 0 || moveY != 0) {
                animator.SetFloat("Horizontal", moveX);
                animator.SetFloat("Vertical", moveY);
                animator.SetFloat("Speed", 1);
            } else {
                animator.SetFloat("Speed", 0);
            }
        }
        // --- PLATFORMER (DİĞER SAHNELER) HAREKETİ ---
        else 
        {
            moveInput = Input.GetAxisRaw("Horizontal");
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
            
            if (moveInput != 0) {
                animator.SetFloat("Horizontal", moveInput);
            }
            animator.SetFloat("Speed", Mathf.Abs(moveInput));

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded) {
                Jump();
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing) {
            StartCoroutine(Dash());
        }
    }

    void Jump() {
        animator.SetTrigger("doJump"); 
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    IEnumerator Dash() {
        isDashing = true;
        animator.SetTrigger("doDash"); 

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0; 

        // Dash yönü
        float dashX = Input.GetAxisRaw("Horizontal");
        float dashY = (originalGravity == 0) ? Input.GetAxisRaw("Vertical") : 0;
        
        Vector2 dashDir = new Vector2(dashX, dashY).normalized;
        if(dashDir == Vector2.zero) dashDir = new Vector2(transform.localScale.x, 0);

        rb.linearVelocity = dashDir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity; 
        rb.linearVelocity = Vector2.zero;
        isDashing = false;
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("FallArea") && !isDead) 
        {
            StartCoroutine(DieAndRespawn());
        }
    }

   IEnumerator DieAndRespawn()
    {
        isDead = true; 
        animator.SetBool("isDead", true);
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false; 

        animator.SetTrigger("doDie"); 

        yield return new WaitForSeconds(1.07f); 
        
        GameObject spawn = GameObject.Find("SpawnPoint");
        if(spawn != null) transform.position = spawn.transform.position;
        
        rb.simulated = true;
        isDead = false; 
        animator.SetBool("isDead", false); 
    }
}