using System.Collections.Generic;
using UnityEngine;

namespace GlitchRunner.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        [System.Serializable]
        public class SlotData
        {
            public string itemId;
            public Sprite icon;
            public int quantity = 1;
        }

        [Header("Veri")]
        [Tooltip("Bu listedeki eleman sayısı kadar slot beklenir. GameState varsa oradan doldurulur.")]
        public List<SlotData> slots = new List<SlotData>();

        [Header("UI")]
        [Tooltip("Eğer boş bırakılırsa, çocuklar içinden otomatik toplanır.")]
        public InventorySlotUI[] slotUIs;

        void Awake()
        {
            if (slotUIs == null || slotUIs.Length == 0)
            {
                slotUIs = GetComponentsInChildren<InventorySlotUI>(true);
            }

            // Veri sayısı slot sayısından azsa doldur
            while (slots.Count < slotUIs.Length)
            {
                slots.Add(new SlotData());
            }

            // GameState varsa envanteri oradan senkronize et
            if (GameState.Instance != null)
            {
                SyncFromGameState();
                GameState.Instance.OnInventoryChanged += OnGameStateInventoryChanged;
            }

            // Slotları bağla ve göster
            for (int i = 0; i < slotUIs.Length; i++)
            {
                slotUIs[i].Bind(this, i);
                slotUIs[i].Refresh(GetSlot(i));
            }
        }

        void OnDestroy()
        {
            if (GameState.Instance != null)
                GameState.Instance.OnInventoryChanged -= OnGameStateInventoryChanged;
        }

        void OnGameStateInventoryChanged()
        {
            SyncFromGameState();
            for (int i = 0; i < slotUIs.Length && i < slots.Count; i++)
                slotUIs[i].Refresh(slots[i]);
        }

        /// <summary>
        /// GameState'teki envanter verisini UI slot listesine kopyalar.
        /// </summary>
        public void SyncFromGameState()
        {
            if (GameState.Instance == null) return;
            int n = GameState.Instance.InventorySlotCount;
            while (slots.Count < n) slots.Add(new SlotData());
            if (slots.Count > n) slots.RemoveRange(n, slots.Count - n);
            for (int i = 0; i < n; i++)
            {
                var entry = GameState.Instance.GetInventorySlot(i);
                slots[i].itemId = entry.itemId ?? "";
                slots[i].quantity = entry.quantity;
                slots[i].icon = ItemRegistry.GetIcon(slots[i].itemId);
            }
        }

        public SlotData GetSlot(int index)
        {
            if (index < 0 || index >= slots.Count) return null;
            return slots[index];
        }

        public void SwapSlots(int a, int b)
        {
            if (a == b) return;
            if (a < 0 || a >= slots.Count) return;
            if (b < 0 || b >= slots.Count) return;

            if (GameState.Instance != null)
            {
                GameState.Instance.SwapInventorySlots(a, b);
                SyncFromGameState();
            }
            else
            {
                var tmp = slots[a];
                slots[a] = slots[b];
                slots[b] = tmp;
            }

            if (a < slotUIs.Length) slotUIs[a].Refresh(slots[a]);
            if (b < slotUIs.Length) slotUIs[b].Refresh(slots[b]);
        }
    }
}
