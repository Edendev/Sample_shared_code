using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.ScriptableObjects.Core;
using Game.MonoBehaviours.UI;

namespace Game.ScriptableObjects.WorldEvents.Reactions
{
    public class PopDownMessageWindowReaction : Reaction
    {
        /// <summary>
        /// Reference the window object that will interact with
        /// </summary>
        public UIMessageWindow MessageWindow;
        bool windowIsHidden;

        public override IEnumerator React()
        {
            // Reset window hidden state checker
            windowIsHidden = false;
            // Resgister to on window is hidden event and call pop down
            MessageWindow.OnWindowIsHidden().AddListener(WindowHasFinished);
            MessageWindow.PopDown();

            yield return WaitUntilWindowIsFinished();
        }

        /// <summary>
        /// Recieve feedback from message window
        /// </summary>
        void WindowHasFinished() => windowIsHidden = true;

        IEnumerator WaitUntilWindowIsFinished()
        {
            while(!windowIsHidden)
            {
                yield return null;
            }
        }
    }
}
