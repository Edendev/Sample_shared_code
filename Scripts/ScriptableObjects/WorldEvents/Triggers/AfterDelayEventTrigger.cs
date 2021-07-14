using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.ScriptableObjects.Core;

namespace Game.ScriptableObjects.WorldEvents.EventTriggers
{
    public class AfterDelayEventTrigger : EventTrigger
    {
        /// <summary>
        /// Time to wait until the trigger is sent
        /// </summary>
        public float DelayTime;

        public override void Initialize(MonoBehaviour owner)
        {
            base.Initialize(owner);

            if (triggerOwner != null)
                triggerOwner.StartCoroutine(WaitUntilDelayTime());
        }

        IEnumerator WaitUntilDelayTime()
        {
            yield return new WaitForSeconds(DelayTime);
            onTrigger?.Invoke();
        }
    }
}
