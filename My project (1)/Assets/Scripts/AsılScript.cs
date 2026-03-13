using UnityEngine;
using System.Collections;

public class AsılScript : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float dashForce = 20f;

    [Header("Silah ve Mermi Ayarları")]
    public int maxAmmo = 30;
    public float reloadTime = 2f;
    private int currentAmmo;
    private bool isReloading = false;

    [Header("Envanter Sistemi")]
    public GameObject inventoryPanel; // LÜTFEN INSPECTOR'DAN PANELİ BURAYA SÜRÜKLE
    private bool isInventoryOpen = false;

    [Header("VFX (Child - gölge/toz)")]
    [Tooltip("Child objesinin Animator'ı (Jump_Dust, Dash_Shadow, Death_Shadow vb.).")]
    public Animator vfxAnimator;
    [Tooltip("Child objesinin SpriteRenderer'ı (yön ile senkron için).")]
    public SpriteRenderer vfxSr;

    private bool isDead = false;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Vector2 moveInput;
    private float currentSpeed;
    private int weaponType = 0; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        currentAmmo = maxAmmo;

        // Oyun başlarken her şeyi sıfırla
        Time.timeScale = 1f;
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }

    void Update()
    {
        // --- 1. ENVANTER AÇMA/KAPATMA (Tab) ---
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }

        // --- 2. DURAKLATMA KONTROLÜ ---
        if (isInventoryOpen || isDead)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetFloat("Speed", 0);
            return;
        }

        // --- 3. HAREKET GİRDİLERİ ---
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        // --- 4. SİLAH DEĞİŞTİRME (envanterde varsa F = silah, X = mızrak; 1 = silahsız) ---
        if (Input.GetKeyDown(KeyCode.Alpha1)) weaponType = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2) && GameState.Instance != null && GameState.Instance.HasItemInInventory("gun")) weaponType = 2;
        if (Input.GetKeyDown(KeyCode.Alpha3) && GameState.Instance != null && GameState.Instance.HasItemInInventory("spear")) weaponType = 1;
        if (Input.GetKeyDown(KeyCode.F) && GameState.Instance != null && GameState.Instance.HasItemInInventory("gun")) weaponType = 2;
        if (Input.GetKeyDown(KeyCode.X) && GameState.Instance != null && GameState.Instance.HasItemInInventory("spear")) weaponType = 1;

        // --- 5. DASH (Ctrl) ---
        if (Input.GetKeyDown(KeyCode.LeftControl) && moveInput != Vector2.zero)
        {
            anim.SetInteger("WeaponType", weaponType);
            anim.SetTrigger("doDash");
            rb.AddForce(moveInput * dashForce, ForceMode2D.Impulse);
            if (vfxAnimator != null) vfxAnimator.SetTrigger("playDashShadow");
        }

        // --- 6. ZIPLAMA (Space) ---
        if (Input.GetKeyDown(KeyCode.Space))
        {
            anim.SetTrigger("doJump");
            if (vfxAnimator != null)
            {
                vfxAnimator.SetTrigger("playJumpDust");
                vfxAnimator.SetTrigger("playJumpShadow");
            }
        }

        // --- 7. ATEŞ / SALDIRI ---
        HandleAttack();

        // --- 8. ANIMATOR GÜNCELLE ---
        UpdateAnimator();
    }

    void ToggleInventory()
    {
        if (inventoryPanel == null)
        {
            inventoryPanel = GameObject.Find("InventoryPanel");
            if (inventoryPanel == null)
                inventoryPanel = InventoryPanelBuilder.Build();
        }
        if (inventoryPanel == null)
        {
            Debug.LogError("Inventory panel could not be found or created.");
            return;
        }

        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            Time.timeScale = 0f; // Zamanı durdur
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Time.timeScale = 1f; // Zamanı başlat
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void HandleAttack()
    {
        if (weaponType == 2) // GUN
        {
            if (Input.GetMouseButton(0) && currentAmmo > 0 && !isReloading)
            {
                anim.SetBool("isShooting", true);
                if (Time.frameCount % 10 == 0) currentAmmo--;
            }
            else
            {
                anim.SetBool("isShooting", false);
            }

            if ((currentAmmo <= 0 || (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)) && !isReloading)
            {
                anim.SetTrigger("doReload");
                StartCoroutine(ReloadRoutine());
            }
        }
        else if (weaponType == 1) // SPEAR
        {
            if (Input.GetMouseButtonDown(0)) anim.SetTrigger("doAttack");
        }
    }

    IEnumerator ReloadRoutine()
    {
        isReloading = true;
        anim.SetBool("isShooting", false);
        anim.SetBool("isReloading", true);
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        anim.SetBool("isReloading", false);
        isReloading = false;
    }

    void FixedUpdate()
    {
        if (!isInventoryOpen && !isDead)
        {
            rb.MovePosition(rb.position + moveInput * currentSpeed * Time.fixedDeltaTime);
        }
        else if (isDead && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("FallArea") && !isDead)
            StartCoroutine(DieAndRespawn());
    }

    IEnumerator DieAndRespawn()
    {
        isDead = true;
        if (vfxAnimator != null)
        {
            if (weaponType == 0) vfxAnimator.SetTrigger("playDeathNormal");
            else if (weaponType == 1) vfxAnimator.SetTrigger("playDeathSpear");
            else if (weaponType == 2) vfxAnimator.SetTrigger("playDeathGun");
        }
        if (anim != null)
        {
            anim.SetBool("isDead", true);
            anim.SetTrigger("doDie");
        }
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }
        yield return new WaitForSeconds(1.07f);
        GameObject spawn = GameObject.Find("SpawnPoint");
        if (spawn != null)
            transform.position = new Vector3(spawn.transform.position.x, spawn.transform.position.y, transform.position.z);
        if (rb != null) rb.simulated = true;
        isDead = false;
        if (anim != null) anim.SetBool("isDead", false);
    }

    void UpdateAnimator()
    {
        if (moveInput.x != 0f || moveInput.y != 0f)
        {
            // Tam yatay (sadece sağ/sol) → MoveY = 0, böylece sağ-yukarı değil düz sağ kullanılır
            if (moveInput.y == 0f)
            {
                anim.SetFloat("MoveX", moveInput.x);
                anim.SetFloat("MoveY", 0f);
            }
            // Tam dikey (sadece yukarı/aşağı) → MoveX = 0
            else if (moveInput.x == 0f)
            {
                anim.SetFloat("MoveX", 0f);
                anim.SetFloat("MoveY", moveInput.y);
            }
            // Çapraz (sağ-aşağı, sol-yukarı vb.) → ikisini de ver, right-down vb. animasyonlar çalışsın
            else
            {
                anim.SetFloat("MoveX", moveInput.x);
                anim.SetFloat("MoveY", moveInput.y);
            }
        }
        float animSpeed = moveInput.magnitude;
        if (animSpeed > 0) animSpeed = Input.GetKey(KeyCode.LeftShift) ? 1f : 0.5f;
        anim.SetFloat("Speed", animSpeed);
        anim.SetInteger("WeaponType", weaponType);
        anim.SetBool("isAiming", Input.GetMouseButton(1));

        if (vfxSr != null && sr != null)
            vfxSr.flipX = sr.flipX;
    }
}