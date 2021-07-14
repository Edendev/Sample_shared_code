using UnityEngine;
using System.Collections;
using Game.Generic.Utiles;

namespace Game.Interfaces.UI
{
    public interface IInteractable
    {
        /// <summary>
        /// Triggers the interact action
        /// </summary>
        /// <param name="target"> Object interacting with </param>
        /// <returns></returns>
        IEnumerator Interact(GameObject target);
        Events.VoidEvent OnInteract();
    }
}