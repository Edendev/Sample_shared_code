using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Game.ScriptableObjects.Combat;
using Game.ScriptableObjects.Core;
using Game.MonoBehaviours.Stats;
using Game.MonoBehaviours.Inventory;

namespace Game.ScriptableObjects.Inventory
{
    [System.Serializable]
    public enum ItemTypeDefinitions
    {
        WEAPON,
        ROPE,
        WEALTH
    }

    /// <summary>
    /// The item category is related to the equipment "set" of the player. 
    /// For instance, MAIN items are stored in slot "MAIN" and player can only carry one at a time.
    /// </summary>
    public enum ItemCategoryDefinitions
    {
        MAIN,
        CONSUMABLE,
    }

    public class ItemDefinition : GameSO
    {
        /// <summary>
        /// The dictionary contains the category relation of each item type.
        /// </summary>
        static public readonly Dictionary<ItemTypeDefinitions, ItemCategoryDefinitions> ItemTypeToCategory = new Dictionary<ItemTypeDefinitions, ItemCategoryDefinitions>()
        {
            {ItemTypeDefinitions.WEAPON, ItemCategoryDefinitions.MAIN },
            {ItemTypeDefinitions.ROPE, ItemCategoryDefinitions.MAIN },
            {ItemTypeDefinitions.WEALTH, ItemCategoryDefinitions.CONSUMABLE },
        };

        /// <summary>
        /// The dictionary contains the max items equipped allowed per category.
        /// For instance, only 1 main item is allow. 2 Consumables (but multple stacks of each are allowed)
        /// </summary>
        static public readonly Dictionary<ItemCategoryDefinitions, int> CategoryToMaxAllowed = new Dictionary<ItemCategoryDefinitions, int>()
        {
            {ItemCategoryDefinitions.MAIN, 1 },
            {ItemCategoryDefinitions.CONSUMABLE, 2 },
        };

        public ItemCategoryDefinitions GetCategory() => ItemTypeToCategory[ItemType];
        public int GetMaxAllowed() => CategoryToMaxAllowed[GetCategory()];

        public ItemTypeDefinitions ItemType;
        public Item ItemWorldObject;

        [Tooltip("Icone used to display the item in UI windows")]
        public Sprite ItemIcone;
        [Tooltip("Icone displayed on the character portrait when equipped. Image must match with character portrait.")]
        public Sprite ItemEquippedIcone;

        [Header("Item specific attributes")]
        public WeaponAttack weapon;
        public Rope rope;

        public int MaxStackNumber = 1;

        public bool IsEquippable = false;
        public bool IsStorable = false;
        public bool IsStackable = false;
        public bool IsConsumable = false;

        public bool UseItem(GameObject user, GameObject target)
        {
            PlayerStats playerStats = user.GetComponent<PlayerStats>();

            switch (ItemType)
            {
                case ItemTypeDefinitions.WEAPON:                    
                    if (playerStats)
                        return playerStats.TryEquipWeapon(weapon);
                    break;
                case ItemTypeDefinitions.ROPE:
                    if (playerStats)
                        return playerStats.TryEquipRope(rope); 
                    break;
                case ItemTypeDefinitions.WEALTH:
                    break;
            }

            return false;
        }
    }
}