#define DEBUG_PLAYER_INVENTORY

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.ScriptableObjects.Inventory;
using Game.Generic.Utiles;
using Game.MonoBehaviours.Stats;

namespace Game.MonoBehaviours.Inventory
{
    public class PlayerInventory : MonoBehaviour
    {
        public static PlayerInventory S;

        Dictionary<int, InventoryEntry> inventoryItems = new Dictionary<int, InventoryEntry>();
        InventoryEntry currentInventoryEntry;

        [System.Serializable]
        public struct InventorySlot
        {
            public bool InUse; // If not will be hidden in the UI 
            public ItemTypeDefinitions ItemType;
        }

        [Header("Set in Inspector")]
        [SerializeField] InventorySlot[] inventorySlots;

        [SerializeField] Events.InventoryEntryEvent onItemStored = new Events.InventoryEntryEvent();
        [SerializeField] Events.InventoryEntryEvent onItemThrown = new Events.InventoryEntryEvent();

        private void Awake()
        {
            S = this;

            currentInventoryEntry = new InventoryEntry(null, 0, -1);
            inventoryItems.Clear();

            // Fill inventory with empty inventory entries
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                inventoryItems.Add(i, currentInventoryEntry);
            }
        }

        public bool TryStoreItem(ItemDefinition itemToStore)
        {
            if (itemToStore != null)
            {
                // Check if this item can be added to the inventory
                if(IsItemAllowedInInventory(itemToStore.ItemType))
                {
                    // Check if the item exists
                    int numItemsOfSameCategory = 0;
                    foreach (KeyValuePair<int, InventoryEntry> ie in inventoryItems)
                    {
                        // Check if entry has item and if not move to next
                        if (ie.Value.ItemSO == null)
                            continue;

                        // Check if same category (used later to check max allowed)
                        if (ie.Value.ItemSO.GetCategory() == itemToStore.GetCategory())
                            numItemsOfSameCategory++;

                        // Check if the item exists
                        if (itemToStore.GetSO_Name() == ie.Value.ItemSO.GetSO_Name())
                        {
                            // Try to add 1 to stack and destroy the new instance
                            if (!ie.Value.TryIncreaseStack(1))
                            {
#if DEBUG_PLAYER_INVENTORY
                                Debug.LogFormat("{0} is not stackable", ie.Value.ItemSO.GetSO_InGameName(), ie.Value.NumStack);
#endif

                                return false;
                            }
                            else
                            {
#if DEBUG_PLAYER_INVENTORY
                                Debug.LogFormat("{0} has been stacked and now stack num is {1}", ie.Value.ItemSO.GetSO_InGameName(), ie.Value.NumStack);
#endif
                                // Send message
                                onItemStored?.Invoke(ie.Value);

                                return true;
                            }
                        }
                    }
                    // If item does not exists, check if exceeds max allowed
                    if (itemToStore.GetMaxAllowed() <= numItemsOfSameCategory)
                    {
#if DEBUG_PLAYER_INVENTORY
                        Debug.LogFormat("{0} cannot be stored because exceeds max allowed of {1}", itemToStore.GetSO_InGameName(), itemToStore.GetMaxAllowed());
#endif
                        return false;
                    }

                    // If item does not exists, try to find an empty slot to store it
                    int slotIndex = FindFirstEmtpySlotOfType(itemToStore.ItemType);
                    if (slotIndex != -1)
                    {
                        if (StoreItemAt(itemToStore, slotIndex))
                            return true;
                    }
                    else
                    {
#if DEBUG_PLAYER_INVENTORY
                        Debug.LogFormat("{0} cannot be stored because there are no empty slots", itemToStore.GetSO_InGameName());
#endif
                        return false;
                    }
                }
            }
            // If all goes wrong
            return false;
        }

        bool StoreItemAt(ItemDefinition itemToStore, int id)
        {
            // Check that id is not out of bounds
            if (id >= inventorySlots.Length || id < 0)
                return false;

            // Check that id corresponds to slot item type
            if (inventorySlots[id].ItemType != itemToStore.ItemType)
                return false;

            // Add item to dictionary
            inventoryItems[id] = new InventoryEntry(itemToStore, 1, id);

            // Send message
            onItemStored?.Invoke(inventoryItems[id]);

#if DEBUG_PLAYER_INVENTORY
            Debug.LogFormat("{0} has been stored in slot {1}", itemToStore.GetSO_InGameName(), id);
#endif
            return true;
        }

        public bool TryUseItem(int slot)
        {
            if (inventoryItems.ContainsKey(slot))
            {
                if (inventoryItems[slot].ItemSO != null)
                {
                    if (inventoryItems[slot].NumStack > 0)
                    {
                        if (inventoryItems[slot].ItemSO.UseItem(Player.S.gameObject, Player.S.CurrentTarget()))
                        {
                            if (inventoryItems[slot].ItemSO.IsConsumable)
                            {
                                inventoryItems[slot].TryDecreaseStack(1);
                                if (inventoryItems[slot].NumStack == 0)
                                    CleanInventoryEntry(slot);
                            }
                            else
                            {
#if DEBUG_PLAYER_INVENTORY
                                Debug.LogFormat("Item {0} in slot {1} is not consumable", inventoryItems[slot].ItemSO.GetSO_InGameName(), slot);
#endif
                            }
                        }
                        else
                        {
#if DEBUG_PLAYER_INVENTORY
                            Debug.LogFormat("Item {0} in slot {1} cannot be used", inventoryItems[slot].ItemSO.GetSO_InGameName(), slot);
#endif
                        }
                    }
                    else
                    {
#if DEBUG_PLAYER_INVENTORY
                        Debug.LogFormat("Inventory slot {0} has not enough items", slot);
#endif
                    }
                }
                else
                {
#if DEBUG_PLAYER_INVENTORY
                    Debug.LogFormat("Inventory slot {0} has no itemDefinition", slot);
#endif
                }
            }
            else
            {
#if DEBUG_PLAYER_INVENTORY
                Debug.LogFormat("Inventory has not slot number {0}", slot);
#endif
            }

            return false;
        }

        public bool TryToThrowItem(int slot)
        {
            if (inventoryItems.ContainsKey(slot))
            {
                if (inventoryItems[slot].ItemSO != null)
                {
                    if (inventoryItems[slot].NumStack > 0)
                    {
                        // Create new world object item and place it in front of the player
                        Instantiate(inventoryItems[slot].ItemSO.ItemWorldObject, Vector3.MoveTowards(transform.position, transform.forward * 2f, Player.S.NavMeshBodyRadius()), inventoryItems[slot].ItemSO.ItemWorldObject.transform.rotation);
                        // Update character stats
                        UpdateCharacterAfterThrowingItem(inventoryItems[slot].ItemSO.ItemType);
                        // Decrease stack num
                        inventoryItems[slot].TryDecreaseStack(1); 

                        onItemThrown?.Invoke(inventoryItems[slot]);

                        if (inventoryItems[slot].NumStack == 0)
                            CleanInventoryEntry(slot);
                    }
                    else
                    {
#if DEBUG_PLAYER_INVENTORY
                        Debug.LogFormat("Inventory slot {0} has not enough items", slot);
#endif
                    }
                }
                else
                {
#if DEBUG_PLAYER_INVENTORY
                    Debug.LogFormat("Inventory slot {0} has no itemDefinition", slot);
#endif
                }
            }
            else
            {
#if DEBUG_PLAYER_INVENTORY
                Debug.LogFormat("Inventory has not slot number {0}", slot);
#endif
            }

            return false;
        }

        void UpdateCharacterAfterThrowingItem(ItemTypeDefinitions itemType)
        {
            if (itemType == ItemTypeDefinitions.WEAPON)
            {
                PlayerStats playerStats = Player.S.gameObject.GetComponent<PlayerStats>();
                playerStats.UnequipWeapon();
            }

            if (itemType == ItemTypeDefinitions.ROPE)
            {
                PlayerStats playerStats = Player.S.gameObject.GetComponent<PlayerStats>();
                playerStats.UnequipRope();
            }
        }

        void CleanInventoryEntry(int slot)
        {
            if (inventoryItems.ContainsKey(slot))
            {
                inventoryItems[slot] = new InventoryEntry(null, 0, slot);
            }
        }

        /// <summary>
        /// Returns -1 if slot not found
        /// </summary>
        public int FindFirstEmtpySlotOfType(ItemTypeDefinitions itemType)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].InUse)
                {
                    if (inventorySlots[i].ItemType == itemType)
                    {
                        if (inventoryItems[i].NumStack == 0)
                            return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns -1 if slot not found
        /// </summary>
        public int FindFirstSlotOfType(ItemTypeDefinitions itemType)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].InUse)
                {
                    if (inventorySlots[i].ItemType == itemType)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns -1 if slot not found
        /// </summary>
        public int FindFirstNonEmptySlotOfType(ItemTypeDefinitions itemType)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].InUse)
                {
                    if (inventoryItems[i].NumStack > 0)
                    {
                        if (inventorySlots[i].ItemType == itemType)
                        {
                            return i;
                        }
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns -1 if slot not found
        /// </summary>
        public int FindFirstSlotOfCategory(ItemCategoryDefinitions itemCategory)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].InUse)
                {
                    if (ItemDefinition.ItemTypeToCategory[inventorySlots[i].ItemType] == itemCategory)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns -1 if slot not found
        /// </summary>
        public int FindFirstNonEmptySlotOfCategory(ItemCategoryDefinitions itemCategory)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].InUse)
                {
                    if (inventoryItems[i].NumStack > 0)
                    {
                        if (ItemDefinition.ItemTypeToCategory[inventorySlots[i].ItemType] == itemCategory)
                        {
                            return i;
                        }
                    }
                }
            }

            return -1;
        }

        bool IsItemAllowedInInventory(ItemTypeDefinitions itemType)
        {
            foreach(InventorySlot invSlot in inventorySlots)
            {
                if (invSlot.InUse)
                {
                    if (invSlot.ItemType == itemType)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #region Accessors

        public InventorySlot[] InventorySlots() => inventorySlots;
        public InventoryEntry GetInventoryEntry(int slot)
        {
            if (slot >= inventoryItems.Count || slot < 0)
                return null;

            return inventoryItems[slot];
        }
        public Events.InventoryEntryEvent OnItemStored() => onItemStored;
        public Events.InventoryEntryEvent OnItemThrown() => onItemThrown;

        #endregion
    }
}
