using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject inventoryPanel; // Eğer sürüklemezsen kod otomatik bulmaya çalışacak
    public bool isInventoryOpen = false;

    void Start()
    {
        // Eğer Panel'i Inspector'dan sürüklemeyi unuttuysan, isme göre bulmaya çalışalım
        if (inventoryPanel == null)
        {
            inventoryPanel = GameObject.Find("InventoryPanel"); 
            // Hiyerarşideki panelinin adının "InventoryPanel" olduğundan emin ol!
        }

        // Başlangıçta her şeyi düzelt
        isInventoryOpen = false;
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
            
        Time.timeScale = 1f; // Ekran gri/donuk kalmasın diye zamanı akıt
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (inventoryPanel == null)
        {
            Debug.LogError("HATA: Envanter Paneli bulunamadı! Lütfen Hiyerarşiden sürükle.");
            return;
        }

        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            Time.timeScale = 0f; // Oyunu durdur
            Cursor.visible = true; 
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Time.timeScale = 1f; // Oyunu başlat
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}