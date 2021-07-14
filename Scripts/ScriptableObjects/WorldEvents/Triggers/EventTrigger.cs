using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.ScriptableObjects.Core;
using Game.Generic.Utiles;

namespace Game.ScriptableObjects.WorldEvents.EventTriggers
{
    public abstract class EventTrigger : GameSO
    {
        /// <summary>
        /// Event called when the trigger is satisfied. The event is called inside the Check function.
        /// </summary>
        protected Events.VoidEvent onTrigger = new Events.VoidEvent();
        /// <summary>
        /// The owner is responsible to call the check for the trigger. 
        /// This allows for control of when the world events are checked.
        /// </summary>
        protected MonoBehaviour triggerOwner;

        /// <summary>
        /// The owner must initialize the trigger.
        /// </summary>
        public virtual void Initialize(MonoBehaviour owner)
        {
            triggerOwner = owner;
        }
        /// <summary>
        /// The owner must call the check function (i.e. in the Update function)
        /// </summary>
        public virtual void Check() { }

        #region Accessors

        public Events.VoidEvent OnTrigger() => onTrigger;

        #endregion
    }
}
