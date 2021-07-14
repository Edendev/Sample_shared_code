using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.ScriptableObjects.Core;
using Game.MonoBehaviours.UI;
using Game.MonoBehaviours;

namespace Game.ScriptableObjects.WorldEvents.Reactions
{
    public class TeleportPlayerToLocationReaction : Reaction
    {
        /// <summary>
        /// Teleport destination in world coordinates
        /// </summary>
        /// <returns></returns>
        public Vector3 Destination; 

        public override IEnumerator React()
        {
            Player.S.TeleportToPosition(Destination);

            yield return null;
        }
    }
}
