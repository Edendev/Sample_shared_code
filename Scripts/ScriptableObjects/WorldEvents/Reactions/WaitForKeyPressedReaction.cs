using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.ScriptableObjects.Core;
using Game.MonoBehaviours.UI;

namespace Game.ScriptableObjects.WorldEvents.Reactions
{
    public class WaitForKeyPressedReaction : Reaction
    {
        public KeyCode Key;

        public override IEnumerator React()
        {
            while(!Input.GetKeyDown(Key))
            {
                yield return null;
            }
        }
    }
}