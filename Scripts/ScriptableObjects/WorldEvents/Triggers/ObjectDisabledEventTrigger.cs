using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.ScriptableObjects.Core;

namespace Game.ScriptableObjects.WorldEvents.EventTriggers
{
    public class ObjectDisabledEventTrigger : EventTrigger
    {
        /// <summary>
        /// Target to track if it is disabled
        /// </summary>
        public GameObject Target;

        public override void Check()
        {
            if (Target.activeSelf == false)
            {
                onTrigger?.Invoke();
            }
        }
    }
}

