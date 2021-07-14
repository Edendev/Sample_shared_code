using System.Collections;
using UnityEngine;
using Game.Interfaces.UI;
using Game.MonoBehaviours.UI.Clickables;
using Game.ScriptableObjects.Inventory;
using Game.ScriptableObjects.UI;
using Game.Generic.Utiles;

namespace Game.MonoBehaviours.Inventory
{
    [RequireComponent(typeof(Clickable))]
    public class Item : MonoBehaviour, IInteractable
    {
        [SerializeField] ItemDefinition itemSO;

        #region Callbacks

        [SerializeField] Events.VoidEvent onInteract = new Events.VoidEvent();

        #endregion

        #region Accessors

        public ItemDefinition ItemSO() => itemSO;

        #endregion

        #region IInteractable

        public IEnumerator Interact(GameObject target)
        {
            if (target != Player.S.gameObject)
                yield break;

            if (itemSO.IsStorable)
            {
                if (PlayerInventory.S.TryStoreItem(itemSO))
                {
                    itemSO.UseItem(Player.S.gameObject, null);
                    Destroy(gameObject);
                }
                else
                {
                    if (itemSO.GetCategory() == ItemCategoryDefinitions.MAIN)
                    {
                        int itemCategorySlot = PlayerInventory.S.FindFirstNonEmptySlotOfCategory(ItemCategoryDefinitions.MAIN);
                         PlayerInventory.S.TryToThrowItem(itemCategorySlot);
                        if (PlayerInventory.S.TryStoreItem(itemSO))
                        {
                            itemSO.UseItem(Player.S.gameObject, null);
                            Destroy(gameObject);
                        }
                    }
                }                
            }

            onInteract?.Invoke();

            yield return null;
        }

        public Events.VoidEvent OnInteract() => onInteract;

        #endregion

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            // Show neighbour connections
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up);
            Gizmos.DrawIcon(transform.position + Vector3.up, "Item_icone.tif", false);
        }
#endif
    }
}

