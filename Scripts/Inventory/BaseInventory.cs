using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Game.Interfaces;
using Game.ScriptableObjects.Inventory;

namespace Game.MonoBehaviours.Inventory
{
    public enum InventorySlotType
    {
        Head, Body, MainHand, SecondaryHand, Misc
    }

    [System.Serializable]
    public class InventorySlot
    {
        public InventorySlot() { }
        public InventorySlot(Item _itemObject, ItemSO _item_SO, ItemConfig _config)
        {
            ItemObject = _itemObject;
            ItemSO = _item_SO;
            Config = _config;
        }

        public Item ItemObject;
        public ItemSO ItemSO;
        public ItemConfig Config;

        public bool IsEmpty()
        {
            return (ItemObject == null && ItemSO == null);
        }
    }

    [System.Serializable]
    public class ItemConfig
    {
        public Transform Parent;
        public Vector3 LocalPosition;
        public Vector3 LocalRotation;
    }

    [System.Serializable]
    public class UsableItemConfig : ItemConfig
    {
        public Transform InUseParent;
        public Vector3 InUseLocalPosition;
        public Vector3 InUseLocalRotation;
    }

    [System.Serializable]
    public class BagSlot
    {
        public BagSlot(ItemSO _itemSO)
        {
            ItemSO = _itemSO;
            StackNum = 1;
        }

        public ItemSO ItemSO { get; private set; }
        public int StackNum { get; private set; }

        public bool CanAdd(int amount)
        {
            if (!ItemSO.IsStackable()) return false;
            if (StackNum + amount > ItemSO.MaxStackNum()) return false;
            return true;
        }
        public void Add(int amount)
        {
            if (!CanAdd(amount)) return;
            StackNum += amount;

        }
        public void Remove(int amount)
        {
            StackNum = Mathf.Clamp(StackNum - amount, 0, StackNum);
        }
    }

    public class BaseInventory : MonoBehaviour, IResetteable
    {
        private const int MAX_HEAD_ITEMS_IN_BAG = 1;
        private const int MAX_BODY_ITEMS_IN_BAG = 1;
        private const int MAX_MAIN_HAND_ITEMS_IN_BAG = 1;
        private const int MAX_SECOND_HAND_ITEMS_IN_BAG = 1;
        private const int MAX_MISC_ITEMS_IN_BAG = 3;
        private const int MAX_CONSUMABLES_IN_BAG = 5;

        [SerializeField] ItemConfig headConfig;
        [SerializeField] ItemConfig bodyConfig;
        [SerializeField] UsableItemConfig mainHandConfig;
        [SerializeField] UsableItemConfig secondaryHandConfig;
        [SerializeField] UsableItemConfig miscConfig;

        Dictionary<InventorySlotType, ItemConfig> itemConfigs = new Dictionary<InventorySlotType, ItemConfig>();

        Dictionary<InventorySlotType, InventorySlot> inventory = new Dictionary<InventorySlotType, InventorySlot>()
        {
            { InventorySlotType.Head, new InventorySlot() },
            { InventorySlotType.Body, new InventorySlot()},
            { InventorySlotType.MainHand, new InventorySlot()},
            { InventorySlotType.SecondaryHand, new InventorySlot()},
            { InventorySlotType.Misc, new InventorySlot()}
        };

        Dictionary<InventorySlotType, int> bagItemsAllowed = new Dictionary<InventorySlotType, int>()
        {
            { InventorySlotType.Head, MAX_HEAD_ITEMS_IN_BAG},
            { InventorySlotType.Body, MAX_BODY_ITEMS_IN_BAG},
            { InventorySlotType.MainHand, MAX_MAIN_HAND_ITEMS_IN_BAG},
            { InventorySlotType.SecondaryHand, MAX_SECOND_HAND_ITEMS_IN_BAG},
            { InventorySlotType.Misc, MAX_MISC_ITEMS_IN_BAG},
        };

        [SerializeField] List<BagSlot> bag = new List<BagSlot>(); //serialize for debuggimg purposes

        public event Action<ItemSO> OnItemAdded;
        public event Action<ItemSO> OnItemRemoved;
        public event Action<ItemSO> OnItemEquipped;
        public event Action<ItemSO> OnItemInUse;

        Item currentUsingItem = null;

        private void Awake()
        {
            // Fill container
            itemConfigs.Add(InventorySlotType.Head, headConfig);
            itemConfigs.Add(InventorySlotType.Body, headConfig);
            itemConfigs.Add(InventorySlotType.MainHand, mainHandConfig);
            itemConfigs.Add(InventorySlotType.SecondaryHand, secondaryHandConfig);
            itemConfigs.Add(InventorySlotType.Misc, miscConfig);

            // Initialize inventory configs
            inventory[InventorySlotType.Head].Config = headConfig;
            inventory[InventorySlotType.Body].Config = bodyConfig;
            inventory[InventorySlotType.MainHand].Config = mainHandConfig;
            inventory[InventorySlotType.SecondaryHand].Config = secondaryHandConfig;
            inventory[InventorySlotType.Misc].Config = miscConfig;
        }

        public bool TryEquipItem(ItemSO itemSO)
        {
            if (itemSO == null) return false;
            if (!inventory.ContainsKey(itemSO.GetInventorySlotType())) return false;
            BagSlot bagSlot = bag.Find(x => x.ItemSO == itemSO);
            if (bagSlot == null) return false;

            if (bagSlot.ItemSO.GetWorldItem() != null)
            {
                Item itemObject = itemSO.CreateItemAs<Item>(inventory[bagSlot.ItemSO.GetInventorySlotType()].Config.Parent,
                    Vector3.zero, Vector3.zero, true, Space.Self);
                itemObject.Initialize(itemSO);
                itemObject.SetUser(gameObject);
                itemObject.transform.localPosition = inventory[itemSO.GetInventorySlotType()].Config.LocalPosition;
                itemObject.transform.localRotation = Quaternion.Euler(inventory[itemSO.GetInventorySlotType()].Config.LocalRotation);
                inventory[itemSO.GetInventorySlotType()].ItemObject = itemObject;
            }

            inventory[itemSO.GetInventorySlotType()].ItemSO = bagSlot.ItemSO;
            OnItemEquipped?.Invoke(itemSO);
            return true;
        }

        public bool TryAddItem(ItemSO itemSO)
        {
            if (!CanAddItem(itemSO)) return false;
            BagSlot bagSlot = bag.Find(x => x.ItemSO == itemSO);

            if (bagSlot == null)
            {
                bag.Add(new BagSlot(itemSO));
            }
            else
            {
                if (!bagSlot.CanAdd(1)) return false;
                bagSlot.Add(1);
            }

            OnItemAdded?.Invoke(itemSO);
            return true;
        }
        public bool CanAddItem(ItemSO itemSO)
        {
            if (itemSO == null) return false;
            if (!bagItemsAllowed.ContainsKey(itemSO.GetInventorySlotType())) return false;
            BagSlot bagSlot = bag.Find(x => x.ItemSO == itemSO);
            if (bagSlot != null && bagSlot.ItemSO.IsStackable() && bagSlot.StackNum < bagSlot.ItemSO.MaxStackNum()) return true;
            int itemsCount = bag.FindAll(x => x.ItemSO.GetInventorySlotType() == itemSO.GetInventorySlotType()).Count;
            if (itemsCount >= bagItemsAllowed[itemSO.GetInventorySlotType()]) return false;
            return true;
        }
        public bool TryRemoveItem(ItemSO itemSO, int amount)
        {
            if (itemSO == null) return false;
            BagSlot bagSlot = bag.Find(x => x.ItemSO == itemSO);
            if (bagSlot == null) return false;

            bagSlot.Remove(amount);
            if (bagSlot.StackNum == 0)
            {
                InventorySlotType slotType = itemSO.GetInventorySlotType();
                if (inventory.ContainsKey(slotType))
                {
                    if (inventory[slotType].ItemSO == itemSO)
                    {
                        inventory[slotType].ItemSO = null;
                        if (inventory[slotType].ItemObject != null) inventory[slotType].ItemObject.TearDown();
                        inventory[slotType].ItemObject = null;
                    }
                }

                bag.Remove(bagSlot);
                bag.TrimExcess();
            }

            OnItemRemoved?.Invoke(itemSO);

            return true;
        }
        public bool CanRemoveItem(ItemSO itemSO)
        {
            if (itemSO == null) return false;
            BagSlot bagSlot = bag.Find(x => x.ItemSO == itemSO);
            if (bagSlot == null) return false;
            return true;
        }

        public bool TryUseItem(InventorySlotType slotType)
        {
            if (!inventory.ContainsKey(slotType)) return false;
            if (inventory[slotType].IsEmpty()) return false;
            // If it is consumable just remove one
            if (inventory[slotType].ItemSO.IsConsumable()) TryRemoveItem(inventory[slotType].ItemSO, 1);
            // If it is not the current using item check if it has usableConfig to swap with the current using item
            if (inventory[slotType].ItemObject != null && currentUsingItem != inventory[slotType].ItemObject)
            {
                UsableItemConfig usableConfig = inventory[slotType].Config as UsableItemConfig;
                if (usableConfig == null) return false;
                if (currentUsingItem != null) UnUseCurrent();
                inventory[slotType].ItemObject.transform.SetParent(usableConfig.InUseParent);
                inventory[slotType].ItemObject.transform.localPosition = usableConfig.InUseLocalPosition;
                inventory[slotType].ItemObject.transform.localRotation = Quaternion.Euler(usableConfig.InUseLocalRotation);
                inventory[slotType].ItemObject.gameObject.SetActive(true);
                currentUsingItem = inventory[slotType].ItemObject;
            }  

         //   OnItemInUse?.Invoke(currentUsingItem);
            return true;
        }

        public bool CanUseItem(InventorySlotType slotType)
        {
            if (!inventory.ContainsKey(slotType)) return false;
            if (inventory[slotType].IsEmpty()) return false;
            if (inventory[slotType].ItemObject != null && currentUsingItem == inventory[slotType].ItemObject) return false;
            UsableItemConfig usableConfig = inventory[slotType].Config as UsableItemConfig;
            if (usableConfig == null) return false;
            return true;
        }

        public bool UnUseCurrent()
        {
            if (currentUsingItem == null) return false;
            InventorySlotType slotType = currentUsingItem.ISlotType();
            if (!inventory.ContainsKey(slotType)) return false;
            currentUsingItem.transform.SetParent(inventory[slotType].Config.Parent);
            currentUsingItem.transform.localPosition = inventory[slotType].Config.LocalPosition;
            currentUsingItem.transform.localRotation = Quaternion.Euler(inventory[slotType].Config.LocalRotation);
            currentUsingItem = null;
            return true;
        }

        public InventorySlotType GetPreviousUsableInventorySlot<T>(Item from)
            where T : Item
        {
            if (from == null)
            {
                InventorySlot firstNonEmptyInvSlot = GetFirstNonEmptyInvSlot();
                if (firstNonEmptyInvSlot == null) return InventorySlotType.MainHand; // by default, not item exists in the inventory
                else from = firstNonEmptyInvSlot.ItemObject;
            }
            bool fromItemFound = false;
            bool cycleDone = false;
            IEnumerator<InventorySlot> inventorySlots = inventory.Values.Reverse().GetEnumerator();
            inventorySlots.MoveNext();
            while (true)
            {
                if (inventorySlots.Current.ItemObject != null)
                {
                    if (fromItemFound)
                    {
                        if (inventorySlots.Current.Config as UsableItemConfig != null && inventorySlots.Current.ItemObject as T != null)
                        {
                            return inventorySlots.Current.ItemObject.ISlotType();
                        }
                    }
                    if (inventorySlots.Current.ItemObject == from) fromItemFound = true;
                }

                if (!inventorySlots.MoveNext())
                {
                    if (cycleDone) return InventorySlotType.SecondaryHand; // This situation should never happen
                    inventorySlots = inventory.Values.Reverse().GetEnumerator();
                    inventorySlots.MoveNext();
                    cycleDone = true;
                }
            }
        }
        public InventorySlotType GetNextUsableInventorySlot<T>(Item from)
            where T : Item
        {
            if (from == null)
            {
                InventorySlot firstNonEmptyInvSlot = GetFirstNonEmptyInvSlot();
                if (firstNonEmptyInvSlot == null) return InventorySlotType.MainHand; // by default, not item exists in the inventory
                else from = firstNonEmptyInvSlot.ItemObject;
            }
            bool fromItemFound = false;
            bool cycleDone = false;
            IEnumerator<InventorySlot> inventorySlots = inventory.Values.GetEnumerator();
            inventorySlots.MoveNext();
            while (true)
            {
                if (inventorySlots.Current.ItemObject != null)
                {
                    if (fromItemFound)
                    {
                        if (inventorySlots.Current.Config as UsableItemConfig != null && inventorySlots.Current.ItemObject as T != null)
                        {
                            return inventorySlots.Current.ItemObject.ISlotType();
                        }
                    }
                    if (inventorySlots.Current.ItemObject == from) fromItemFound = true;
                }

                if (!inventorySlots.MoveNext())
                {
                    if (cycleDone) return InventorySlotType.SecondaryHand; // This situation should never happen
                    inventorySlots.Reset();
                    inventorySlots.MoveNext();
                    cycleDone = true;
                }
            }
        }



        public ItemSO GetPreviousMiscItem(ItemSO from)
        {
            if (bag.Count <= 1) return from;
            int index = bag.Count - 1;
            if (from != null) index = bag.FindIndex(x => x.ItemSO == from);
            if (index == -1) index = bag.Count - 1;
            if (index == 0) index = bag.Count - 1;
            bool loop = index == bag.Count - 1 ? true : false;
            while(true)
            {
                if (bag[index].ItemSO != from && 
                    bag[index].ItemSO.GetInventorySlotType() == InventorySlotType.Misc) return bag[index].ItemSO;

                if (!loop && index == 0)
                {
                    index = bag.Count - 1;
                    loop = true;
                    continue;
                }
                else if (loop && index == 0) break;

                index--;
            }
            return from;
        }
        public ItemSO GetNextMiscItem(ItemSO from)
        {
            if (bag.Count <= 1) return from;
            int index = 0;
            if (from != null) index = bag.FindIndex(x => x.ItemSO == from);
            if (index == -1) index = 0;
            if (index == bag.Count - 1) index = 0;
            bool loop = index == 0 ? true : false;
            while(true)
            { 
                if (bag[index].ItemSO != from && 
                    bag[index].ItemSO.GetInventorySlotType() == InventorySlotType.Misc) return bag[index].ItemSO;

                if (!loop && index == bag.Count-1)
                {
                    index = 0;
                    loop = true;
                    continue;
                }
                else if (loop && index == bag.Count - 1) break;

                index++;
            }
            return from;
        }

        InventorySlot GetFirstNonEmptyInvSlot()
        {
            IEnumerator<InventorySlot> inventorySlots = inventory.Values.GetEnumerator();
            while (inventorySlots.MoveNext())
            {
                if (inventorySlots.Current.ItemObject != null) return inventorySlots.Current;
            }
            return null;
        }

        public T GetInventorySlotConfig<T>(InventorySlotType slotType)
            where T : ItemConfig
        {
            if (!inventory.ContainsKey(slotType)) return null;
            return inventory[slotType].Config as T;
        }
        public int GetBagSlotStackNum(ItemSO itemSO)
        {
            if (itemSO == null) return 0;
            BagSlot bagSlot = bag.Find(x => x.ItemSO == itemSO);
            if (bagSlot == null) return 0;
            return bagSlot.StackNum;
        }
        public Item GetCurrentUsingItem() => currentUsingItem;
        public T GetCurrentUsingItem<T>()
            where T : Item
        {
            if (currentUsingItem == null) return null;
            return currentUsingItem as T;
        }
        public Item GetItemAtSlotType(InventorySlotType slotType)
        {
            if (!inventory.ContainsKey(slotType)) return null;
            return inventory[slotType].ItemObject;
        }
        public ItemSO GetItemSOAtSlotType(InventorySlotType slotType)
        {
            if (!inventory.ContainsKey(slotType)) return null;
            return inventory[slotType].ItemSO;
        }

        public void Reset()
        {
            UnUseCurrent();
            CleanInventory();
        //    OnItemAdded = null;
        //    OnItemRemoved = null;
        }

        void CleanInventory()
        {
            IEnumerator<InventorySlot> inventorySlots = inventory.Values.GetEnumerator();
            while (inventorySlots.MoveNext())
            {
                if (inventorySlots.Current.ItemObject != null) inventorySlots.Current.ItemObject.TearDown();
            }

            CleanBag();
        }
        void CleanBag()
        {
            bag.Clear();
            bag.TrimExcess();
        }
    }
}
