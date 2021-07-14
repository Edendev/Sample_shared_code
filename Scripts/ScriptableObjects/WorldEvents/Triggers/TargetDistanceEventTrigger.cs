using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.ScriptableObjects.Core;

namespace Game.ScriptableObjects.WorldEvents.EventTriggers
{
    public class TargetDistanceEventTrigger : EventTrigger
    {
        /// <summary>
        /// Target to track distance from. 
        /// </summary>
        public Transform Target;
        public float Distance;

        public override void Check()
        {
            if ((Target.position - triggerOwner.transform.position).magnitude < Distance)
            {
                onTrigger?.Invoke();
            }
        }
    }
}
