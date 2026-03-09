using System;
using System.Collections.Generic;
using UnityEngine;
using GlitchRunner.Inventory;

/// <summary>
/// Kalıcı oyun state'i: envanter, coin bakiye, cüzdan adresi.
/// Sahneler arası taşınır (DontDestroyOnLoad).
/// </summary>
public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    [System.Serializable]
    public class InventorySlotEntry
    {
        public string itemId = "";
        public int quantity = 0;
    }

    [Header("Envanter")]
    [SerializeField] List<InventorySlotEntry> inventorySlots = new List<InventorySlotEntry>();
    [SerializeField] int inventorySlotCount = 20;

    [Header("Coin ve Cüzdan")]
    [SerializeField] decimal coinBalance = 100m;
    [SerializeField] string walletAddress = "";

    public decimal CoinBalance => coinBalance;
    public string WalletAddress => walletAddress;
    public bool IsLoggedIn => !string.IsNullOrEmpty(walletAddress);

    public event Action OnInventoryChanged;
    public event Action OnCoinChanged;
    public event Action OnWalletChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        while (inventorySlots.Count < inventorySlotCount)
            inventorySlots.Add(new InventorySlotEntry());
    }

    public int InventorySlotCount => inventorySlotCount;

    public InventorySlotEntry GetInventorySlot(int index)
    {
        if (index < 0 || index >= inventorySlots.Count) return null;
        return inventorySlots[index];
    }

    public List<InventorySlotEntry> GetAllInventorySlots()
    {
        return new List<InventorySlotEntry>(inventorySlots);
    }

    public void SwapInventorySlots(int a, int b)
    {
        if (a == b) return;
        if (a < 0 || a >= inventorySlots.Count) return;
        if (b < 0 || b >= inventorySlots.Count) return;

        var tmp = inventorySlots[a];
        inventorySlots[a] = inventorySlots[b];
        inventorySlots[b] = tmp;
        OnInventoryChanged?.Invoke();
    }

    public bool AddItemToInventory(string itemId, Sprite icon, int quantity = 1)
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (string.IsNullOrEmpty(inventorySlots[i].itemId))
            {
                inventorySlots[i].itemId = itemId;
                inventorySlots[i].quantity = quantity;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }
        return false;
    }

    /// <summary>Envanterde bu itemId'den en az bir adet var mı?</summary>
    public bool HasItemInInventory(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return false;
        for (int i = 0; i < inventorySlots.Count; i++)
            if (inventorySlots[i].itemId == itemId && inventorySlots[i].quantity > 0)
                return true;
        return false;
    }

    public void SetInventorySlot(int index, string itemId, int quantity)
    {
        if (index < 0 || index >= inventorySlots.Count) return;
        inventorySlots[index].itemId = itemId ?? "";
        inventorySlots[index].quantity = quantity;
        OnInventoryChanged?.Invoke();
    }

    public bool SpendCoins(decimal amount)
    {
        if (amount <= 0 || coinBalance < amount) return false;
        coinBalance -= amount;
        OnCoinChanged?.Invoke();
        return true;
    }

    public void AddCoins(decimal amount)
    {
        if (amount <= 0) return;
        coinBalance += amount;
        OnCoinChanged?.Invoke();
    }

    public void SetWalletAddress(string address)
    {
        walletAddress = address ?? "";
        OnWalletChanged?.Invoke();
    }

    public void SetCoinBalance(decimal amount)
    {
        coinBalance = amount < 0 ? 0 : amount;
        OnCoinChanged?.Invoke();
    }
}

// Oyun başlamadan önce GameState'i oluşturur; sahnede obje eklemeye gerek kalmaz.
internal static class GameStateBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureGameState()
    {
        if (GameState.Instance != null) return;
        var go = new GameObject("GameState");
        go.AddComponent<GameState>();
        go.AddComponent<MarketUI>();
        go.AddComponent<AvalancheWallet>();
    }
}
