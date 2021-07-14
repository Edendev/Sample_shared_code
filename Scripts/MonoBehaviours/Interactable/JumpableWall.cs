using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Interfaces.UI;
using Game.Generic.Utiles;

namespace Game.MonoBehaviours.Interactable
{
    public class JumpableWall : MonoBehaviour, IInteractable
    {
        #region Callbacks

        [SerializeField] Events.VoidEvent onInteract = new Events.VoidEvent();

        #endregion

        #region IInteractable

        public IEnumerator Interact(GameObject target)
        {
            onInteract?.Invoke();

            yield return null;
        }

        public Events.VoidEvent OnInteract() => onInteract;

        #endregion
    }
}
