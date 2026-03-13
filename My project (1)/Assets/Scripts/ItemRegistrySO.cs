using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Item tanımları (ScriptableObject). Market ve envanter buradan ikon/isim/fiyat okur.
/// </summary>
[CreateAssetMenu(fileName = "ItemRegistry", menuName = "Game/Item Registry")]
public class ItemRegistrySO : ScriptableObject
{
    public enum ItemCategory { Weapon, Spear }

    [System.Serializable]
    public class Entry
    {
        public string itemId;
        public string displayName;
        public Sprite icon;
        public decimal price = 0m;
        public ItemCategory category = ItemCategory.Weapon;
    }

    public List<Entry> entries = new List<Entry>();

    public Sprite GetIcon(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return null;
        var e = entries.Find(x => x.itemId == itemId);
        return e?.icon;
    }

    public string GetDisplayName(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return "";
        var e = entries.Find(x => x.itemId == itemId);
        return e?.displayName ?? itemId;
    }

    public decimal GetPrice(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return 0m;
        var e = entries.Find(x => x.itemId == itemId);
        return e?.price ?? 0m;
    }

    public ItemCategory GetCategory(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return ItemCategory.Weapon;
        var e = entries.Find(x => x.itemId == itemId);
        return e?.category ?? ItemCategory.Weapon;
    }

    /// <summary>Markette listelenecek tüm itemlar (silah/mızrak).</summary>
    public IReadOnlyList<Entry> GetMarketEntries() => entries;
}
