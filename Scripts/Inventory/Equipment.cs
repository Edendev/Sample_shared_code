using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Interfaces;
using Game.MonoBehaviours.Player;

namespace Game.MonoBehaviours.Inventory
{
    public class Equipment : Item
    {
        [SerializeField] protected InventorySlotType iSlotType;

        public virtual void Equip(GameObject user)
        {
            BaseInventory inventory = user.GetComponent<BaseInventory>();
            if (inventory == null) return;
            inventory.AddItem(iSlotType, this);
            itemSO.ApplyStatBuffs(user, this);
        }

        public virtual void Unequip(GameObject user)
        {
            BaseInventory inventory = user.gameObject.GetComponent<BaseInventory>();
            if (inventory == null) return;
            inventory.RemoveItem(iSlotType);
            itemSO.RemoveStatBuffsFromSource(user, this);
            TearDown();
        }

        public virtual bool CanEquip(GameObject user)
        {
            BaseInventory inventory = user.gameObject.GetComponent<BaseInventory>();
            if (inventory == null) return false;
            if (inventory.GetEquipment(iSlotType) != null) return false;
            return true;
        }

        public InventorySlotType ISlotType() => iSlotType;
    }
}
