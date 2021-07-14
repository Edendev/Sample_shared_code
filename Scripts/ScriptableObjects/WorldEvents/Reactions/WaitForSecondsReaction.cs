using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.ScriptableObjects.Core;
using Game.MonoBehaviours.UI;

namespace Game.ScriptableObjects.WorldEvents.Reactions
{
    public class WaitForSecondsReaction : Reaction
    {
        /// <summary>
        /// Reference the window object that will interact with
        /// </summary>
        public float Delay;

        public override IEnumerator React()
        {
            yield return new WaitForSeconds(Delay);
        }
    }
}