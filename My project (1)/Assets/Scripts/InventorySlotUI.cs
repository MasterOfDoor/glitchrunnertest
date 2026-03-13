using GlitchRunner.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Tek bir envanter slotunun UI tarafını yönetir.
/// Sürükle-bırak ile slotlar arası item taşıma buradan tetiklenir.
/// </summary>
public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI")]
    public Image iconImage;
    [Tooltip("Item adı (Gun, Spear vb.) burada gösterilir.")]
    public Text labelText;

    [HideInInspector] public int index;
    [HideInInspector] public InventoryUI inventory;

    static InventorySlotUI draggedSlot;

    public void Bind(InventoryUI inventoryUI, int slotIndex)
    {
        inventory = inventoryUI;
        index = slotIndex;
    }

    public void Refresh(InventoryUI.SlotData data)
    {
        if (labelText != null)
            labelText.text = "";

        if (data == null || string.IsNullOrEmpty(data.itemId) || data.quantity <= 0)
        {
            iconImage.enabled = false;
            iconImage.sprite = null;
        }
        else
        {
            iconImage.enabled = true;
            iconImage.sprite = data.icon;
            if (data.icon == null)
                iconImage.color = new Color(0.3f, 0.7f, 0.35f);
            else
                iconImage.color = Color.white;
            if (labelText != null)
                labelText.text = ItemRegistry.GetDisplayName(data.itemId);
        }
    }

    bool HasItem()
    {
        var data = inventory != null ? inventory.GetSlot(index) : null;
        return data != null && !string.IsNullOrEmpty(data.itemId) && data.quantity > 0;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (inventory == null) return;
        if (!HasItem()) return;

        draggedSlot = this;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        draggedSlot = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (inventory == null) return;
        if (draggedSlot == null) return;
        if (draggedSlot == this) return;

        inventory.SwapSlots(draggedSlot.index, index);
    }
}

