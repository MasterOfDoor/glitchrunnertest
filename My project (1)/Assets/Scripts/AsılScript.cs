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

    // Bileşenler ve Değişkenler
    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private float currentSpeed;
    private int weaponType = 0; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentAmmo = maxAmmo;

        // Oyun başlarken her şeyi sıfırla
        Time.timeScale = 1f;
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }

    void Update()
    {
        // --- 1. ENVANTER AÇMA/KAPATMA ---
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleInventory();
        }

        // --- 2. DURAKLATMA KONTROLÜ ---
        if (isInventoryOpen)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetFloat("Speed", 0);
            return; // Envanter açıkken aşağıyı okuma
        }

        // --- 3. HAREKET GİRDİLERİ ---
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        bool isRunning = Input.GetKey(KeyCode.R);
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        // --- 4. SİLAH DEĞİŞTİRME ---
        if (Input.GetKeyDown(KeyCode.Alpha1)) weaponType = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) weaponType = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) weaponType = 2;

        // --- 5. DASH (Shift) ---
        if (Input.GetKeyDown(KeyCode.LeftShift) && moveInput != Vector2.zero)
        {
            anim.SetTrigger("doDash");
            rb.AddForce(moveInput * dashForce, ForceMode2D.Impulse);
        }

        // --- 6. ZIPLAMA (Space) ---
        if (Input.GetKeyDown(KeyCode.Space)) anim.SetTrigger("doJump");

        // --- 7. ATEŞ / SALDIRI ---
        HandleAttack();

        // --- 8. ANIMATOR GÜNCELLE ---
        UpdateAnimator();
    }

    void ToggleInventory()
    {
        if (inventoryPanel == null)
        {
            Debug.LogError("LÜTFEN: Karakterin üstündeki Inventory Panel kutusuna Paneli sürükle!");
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
        if (weaponType == 1) // GUN
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
                StartCoroutine(ReloadRoutine());
            }
        }
        else if (weaponType == 2) // SPEAR
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
        if (!isInventoryOpen)
        {
            rb.MovePosition(rb.position + moveInput * currentSpeed * Time.fixedDeltaTime);
        }
    }

    void UpdateAnimator()
    {
        anim.SetFloat("MoveX", moveInput.x);
        anim.SetFloat("MoveY", moveInput.y);
        float animSpeed = moveInput.magnitude;
        if (animSpeed > 0) animSpeed = Input.GetKey(KeyCode.R) ? 1f : 0.5f;
        anim.SetFloat("Speed", animSpeed);
        anim.SetInteger("WeaponType", weaponType);
        anim.SetBool("isAiming", Input.GetMouseButton(1));
    }
}