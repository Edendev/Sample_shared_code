using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.ScriptableObjects.Core;
using Game.MonoBehaviours.UI;
using Game.ScriptableObjects.UI;

namespace Game.ScriptableObjects.WorldEvents.Reactions
{
    public class SetPortraitMessageWindowReaction : Reaction
    {
        /// <summary>
        /// Reference the window object that will interact with
        /// </summary>
        public UIMessageWindow MessageWindow;
        /// <summary>
        /// Reference to the dialogue portratis SO
        /// </summary>
        public DialoguePortraits DialoguePortraits;
        public DialoguePortraits.Emotion Emotion;

        public override IEnumerator React()
        {
            // Look for the portrait in the dialogue portraits
            if (DialoguePortraits != null)
            {
                Sprite lookAtSprite = DialoguePortraits.GetPortrait(Emotion);
                if (lookAtSprite != null)
                    MessageWindow.SetPortrait(lookAtSprite);
            }

            yield return null;
        }
    }
}
