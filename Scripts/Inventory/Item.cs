using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.ScriptableObjects.Inventory;
using Game.Interfaces;
using Game.Generic.ObjectPooling;

namespace Game.MonoBehaviours.Inventory
{
    public class Item : MonoBehaviour, IResetteable, ITearDown
    {
        protected GameObject user;
        protected ItemSO itemSO;

        public virtual void Initialize(ItemSO _itemSO)
        {
            itemSO = _itemSO;
        }

        /*     public virtual void AddToInventory(GameObject user)
             {
                 BaseInventory inventory = user.GetComponent<BaseInventory>();
                 if (inventory == null) return;
                 inventory.TryAddItem(itemSO.GetInventorySlotType(), this);
                 itemSO.ApplyStatBuffs(user, this);
                 currentUser = user;
             }
             public virtual void RemoveFromInventory(GameObject user)
             {
                 BaseInventory inventory = user.gameObject.GetComponent<BaseInventory>();
                 if (inventory == null) return;
                 inventory.TryRemoveItem(itemSO.GetInventorySlotType(), this);
                 itemSO.RemoveStatBuffsFromSource(user, this);
                 currentUser = null;
                 TearDown();
             }
             public virtual bool CanAddToInventory(GameObject user)
             {
                 BaseInventory inventory = user.gameObject.GetComponent<BaseInventory>();
                 if (inventory == null) return false;
                 if (!inventory.CanAddItem(ISlotType(), this)) return false;
                 return true;
             }*/

        public virtual void SetUser(GameObject _user)
        {
            user = _user;
        }

        public virtual void Reset()
        {
            itemSO = null;
        }

        public virtual void TearDown()
        {
            RemoveObjectFromScene();
        }

        /// <summary>
        /// Child equipments can implement other behaviours, i.e. TearDown when pooling objects. By default destory GO
        /// </summary>
        protected virtual void RemoveObjectFromScene()
        {
            Destroy(gameObject);
        }

        #region Accessors

        public string GetItemName() => itemSO.GetItemName();
        public string GetAnimClipNameSufix() => itemSO.GetAnimClipNameSufix();
        public int GetAnimLayerIndex() => itemSO.GetAnimLayerIndex();
        public Sprite GetItemIcon() => itemSO.GetItemIcon();
        public InventorySlotType ISlotType() => itemSO.GetInventorySlotType();

        #endregion
    }
}

