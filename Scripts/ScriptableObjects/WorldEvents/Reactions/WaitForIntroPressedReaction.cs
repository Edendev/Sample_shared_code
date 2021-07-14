using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.ScriptableObjects.Core;
using Game.MonoBehaviours.UI;

namespace Game.ScriptableObjects.WorldEvents.Reactions
{
    public class WaitForIntroPressedReaction : WaitForKeyPressedReaction
    {
        [Tooltip("This object will be enabled on starting the reaction and disabled afterwards.")]
        public GameObject UIButtonPress;

        private void Awake()
        {
            Key = KeyCode.Return;
        }

        public override IEnumerator React()
        {
            if (UIButtonPress != null)
                UIButtonPress.SetActive(true);

            yield return base.React();

            if (UIButtonPress != null)
                UIButtonPress.SetActive(false);
        }
    }
}