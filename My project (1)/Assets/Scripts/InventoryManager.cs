using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Boş bırakırsan Tab ile açılan envanter paneli otomatik oluşturulur.")]
    public GameObject inventoryPanel;
    public bool isInventoryOpen = false;

    void Awake()
    {
        if (inventoryPanel == null)
            inventoryPanel = GameObject.Find("InventoryPanel");

        if (inventoryPanel == null)
            inventoryPanel = InventoryPanelBuilder.Build();
    }

    void Start()
    {
        isInventoryOpen = false;
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleInventory();
    }

    public void ToggleInventory()
    {
        if (inventoryPanel == null)
        {
            inventoryPanel = InventoryPanelBuilder.Build();
            if (inventoryPanel == null) return;
        }

        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
