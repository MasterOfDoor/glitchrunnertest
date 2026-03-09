#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class CreateItemRegistryAsset
{
    [MenuItem("Game/Create Item Registry (Resources)")]
    public static void Create()
    {
        if (Resources.Load<ItemRegistrySO>("ItemRegistry") != null)
        {
            Debug.Log("ItemRegistry already exists in Resources.");
            return;
        }
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        var so = ScriptableObject.CreateInstance<ItemRegistrySO>();
        so.entries.Add(new ItemRegistrySO.Entry { itemId = "weapon_sword", displayName = "Kılıç", price = 50m, category = ItemRegistrySO.ItemCategory.Weapon });
        so.entries.Add(new ItemRegistrySO.Entry { itemId = "weapon_axe", displayName = "Balta", price = 45m, category = ItemRegistrySO.ItemCategory.Weapon });
        so.entries.Add(new ItemRegistrySO.Entry { itemId = "spear_basic", displayName = "Temel Mızrak", price = 30m, category = ItemRegistrySO.ItemCategory.Spear });
        so.entries.Add(new ItemRegistrySO.Entry { itemId = "spear_heavy", displayName = "Ağır Mızrak", price = 60m, category = ItemRegistrySO.ItemCategory.Spear });

        AssetDatabase.CreateAsset(so, "Assets/Resources/ItemRegistry.asset");
        AssetDatabase.SaveAssets();
        Debug.Log("Created Assets/Resources/ItemRegistry.asset");
    }
}
#endif
