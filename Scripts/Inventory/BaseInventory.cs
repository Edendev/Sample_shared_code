using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Game.Interfaces;

namespace Game.MonoBehaviours.Inventory
{
    public enum InventorySlotType
    {
        Head, Body, MainHand, SecondaryHand, Melee, Misc
    }

    [System.Serializable]
    public class InventorySlot
    {
        public Item ItemObject;
        public ItemConfig Config;
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

    public class BaseInventory : MonoBehaviour, IResetteable
    {
        [SerializeField] ItemConfig headConfig;
        [SerializeField] ItemConfig bodyConfig;
        [SerializeField] UsableItemConfig mainHandConfig;
        [SerializeField] UsableItemConfig secondaryHandConfig;
        [SerializeField] UsableItemConfig meleeConfig;
        [SerializeField] UsableItemConfig miscConfig;

        Dictionary<InventorySlotType, InventorySlot> inventory = new Dictionary<InventorySlotType, InventorySlot>()
        {
            { InventorySlotType.Head, new InventorySlot() },
            { InventorySlotType.Body, new InventorySlot()},
            { InventorySlotType.MainHand, new InventorySlot()},
            { InventorySlotType.SecondaryHand, new InventorySlot()},
            { InventorySlotType.Melee, new InventorySlot()},
            { InventorySlotType.Misc, new InventorySlot()}
        };

        public event Action<Item> OnItemAdded;
        public event Action<Item> OnItemRemoved;

        Item currentUsingItem = null;

        private void Awake()
        {
            inventory[InventorySlotType.Head].Config = headConfig;
            inventory[InventorySlotType.Body].Config = bodyConfig;
            inventory[InventorySlotType.MainHand].Config = mainHandConfig;
            inventory[InventorySlotType.SecondaryHand].Config = secondaryHandConfig;
            inventory[InventorySlotType.Melee].Config = meleeConfig;
            inventory[InventorySlotType.Misc].Config = miscConfig;
        }

        public bool AddItem(InventorySlotType slotType, Item itemObject)
        {
            if (!inventory.ContainsKey(slotType)) return false;
            if (inventory[slotType].ItemObject != null) return false;

            inventory[slotType].ItemObject = itemObject;
            itemObject.transform.SetParent(inventory[slotType].Config.Parent);
            itemObject.transform.localPosition = inventory[slotType].Config.LocalPosition;
            itemObject.transform.localRotation = Quaternion.Euler(inventory[slotType].Config.LocalRotation);

            OnItemAdded?.Invoke(itemObject);

            return true;
        }
        public bool RemoveItem(InventorySlotType slotType)
        {
            if (!inventory.ContainsKey(slotType)) return false;
            if (inventory[slotType].ItemObject == null) return false;

            OnItemRemoved?.Invoke(inventory[slotType].ItemObject);

            if (currentUsingItem == inventory[slotType].ItemObject) currentUsingItem = null;
            inventory[slotType].ItemObject = null;

            return true;
        }

        public Equipment GetEquipment(InventorySlotType slotType)
        {
            if (!inventory.ContainsKey(slotType)) return null;
            if (inventory[slotType].ItemObject == null) return null;
            return inventory[slotType].ItemObject.GetComponent<Equipment>();
        }
        public T GetEquipmentOfType<T>(InventorySlotType slotType)
            where T : Equipment
        {
            if (!inventory.ContainsKey(slotType)) return null;
            if (inventory[slotType].ItemObject == null) return null;
            return inventory[slotType].ItemObject.GetComponent<T>();
        }

        public bool CanUseItem(InventorySlotType slotType)
        {
            if (!inventory.ContainsKey(slotType)) return false;
            if (inventory[slotType].ItemObject == null) return false;
            if (currentUsingItem == inventory[slotType].ItemObject) return false;
            UsableItemConfig usableConfig = inventory[slotType].Config as UsableItemConfig;
            if (usableConfig == null) return false;
            return true;
        }

        public bool UseItem(InventorySlotType slotType)
        {
            if (!inventory.ContainsKey(slotType)) return false;
            if (inventory[slotType].ItemObject == null) return false;
            if (currentUsingItem == inventory[slotType].ItemObject) return false;
            UsableItemConfig usableConfig = inventory[slotType].Config as UsableItemConfig;
            if (usableConfig == null) return false;
            if (currentUsingItem != null) UnUseCurrent();
            inventory[slotType].ItemObject.transform.SetParent(usableConfig.InUseParent);
            inventory[slotType].ItemObject.transform.localPosition = usableConfig.InUseLocalPosition;
            inventory[slotType].ItemObject.transform.localRotation = Quaternion.Euler(usableConfig.InUseLocalRotation);
            currentUsingItem = inventory[slotType].ItemObject;
            return true;
        }
        public bool UnUseCurrent()
        {
            if (currentUsingItem == null) return false;
            Equipment currentUsingEquipment = currentUsingItem as Equipment;
            if (currentUsingEquipment == null) return false;
            InventorySlotType slotType = currentUsingEquipment.ISlotType();
            currentUsingItem.transform.SetParent(inventory[slotType].Config.Parent);
            currentUsingItem.transform.localPosition = inventory[slotType].Config.LocalPosition;
            currentUsingItem.transform.localRotation = Quaternion.Euler(inventory[slotType].Config.LocalRotation);
            currentUsingItem = null;
            return true;
        }

        public InventorySlot GetInventorySlot(InventorySlotType slotType)
        {
            if (!inventory.ContainsKey(slotType)) return null;
            return inventory[slotType];
        }
        public Item GetCurrentUsingItem() => currentUsingItem;
        public T GetCurrentUsingItem<T>()
            where T : Item
        {
            if (currentUsingItem == null) return null;
            return currentUsingItem as T;
        }

        public void Reset()
        {
            UnUseCurrent();
            CleanInventory();
            OnItemAdded = null;
            OnItemRemoved = null;
        }

        void CleanInventory()
        {
            IEnumerator<InventorySlot> inventorySlots = inventory.Values.GetEnumerator();
            while (inventorySlots.MoveNext())
            {
                if (inventorySlots.Current.ItemObject != null)
                {
                    Equipment equipment = inventorySlots.Current.ItemObject as Equipment;
                    if (equipment != null) equipment.Unequip(gameObject);
                    if (equipment != null) equipment.TearDown();
                }
            }
        }
    }
}
