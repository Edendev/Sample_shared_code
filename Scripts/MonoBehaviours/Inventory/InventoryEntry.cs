using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.ScriptableObjects.Inventory;

namespace Game.MonoBehaviours.Inventory
{
    public class InventoryEntry
    {
        public ItemDefinition ItemSO { get; private set; }
        public int Slot { get; private set; }
        public int NumStack { get; private set; }
               
        // Constructor
        public InventoryEntry(ItemDefinition item, int numStack, int slot)
        {
            NumStack = numStack;
            ItemSO = item;
            Slot = slot;
        }

        public bool TryIncreaseStack(int amount)
        {
            if (!ItemSO.IsStackable)
                return false;

            if (NumStack + amount > ItemSO.MaxStackNumber)
                return false;

            NumStack += amount;
            return true;
        }
        public bool TryDecreaseStack(int amount)
        {
            if (NumStack - amount < 0)
                return false;

            NumStack -= amount;
            return true;
        }
    }
}
