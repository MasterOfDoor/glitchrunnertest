using UnityEngine;

/// <summary>
/// itemId -> Sprite/isim/fiyat çözümlemesi. Resources/ItemRegistry SO veya statik fallback.
/// </summary>
public static class ItemRegistry
{
    public static Sprite GetIcon(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return null;
        var reg = Resources.Load<ItemRegistrySO>("ItemRegistry");
        return reg != null ? reg.GetIcon(itemId) : null;
    }

    public static string GetDisplayName(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return "";
        var reg = Resources.Load<ItemRegistrySO>("ItemRegistry");
        return reg != null ? reg.GetDisplayName(itemId) : itemId;
    }

    public static decimal GetPrice(string itemId)
    {
        var reg = Resources.Load<ItemRegistrySO>("ItemRegistry");
        return reg != null ? reg.GetPrice(itemId) : 0m;
    }

    public static ItemRegistrySO.ItemCategory GetCategory(string itemId)
    {
        var reg = Resources.Load<ItemRegistrySO>("ItemRegistry");
        return reg != null ? reg.GetCategory(itemId) : ItemRegistrySO.ItemCategory.Weapon;
    }

    public static System.Collections.Generic.IReadOnlyList<ItemRegistrySO.Entry> GetMarketEntries()
    {
        var reg = Resources.Load<ItemRegistrySO>("ItemRegistry");
        return reg != null ? reg.GetMarketEntries() : DefaultMarketEntries;
    }

    /// <summary>Asset yoksa kullanılan varsayılan silah/mızrak listesi.</summary>
    static readonly System.Collections.Generic.List<ItemRegistrySO.Entry> DefaultMarketEntries =
        new System.Collections.Generic.List<ItemRegistrySO.Entry>
        {
            new ItemRegistrySO.Entry { itemId = "weapon_sword", displayName = "Kılıç", price = 50m, category = ItemRegistrySO.ItemCategory.Weapon },
            new ItemRegistrySO.Entry { itemId = "weapon_axe", displayName = "Balta", price = 45m, category = ItemRegistrySO.ItemCategory.Weapon },
            new ItemRegistrySO.Entry { itemId = "spear_basic", displayName = "Temel Mızrak", price = 30m, category = ItemRegistrySO.ItemCategory.Spear },
            new ItemRegistrySO.Entry { itemId = "spear_heavy", displayName = "Ağır Mızrak", price = 60m, category = ItemRegistrySO.ItemCategory.Spear },
        };
}
