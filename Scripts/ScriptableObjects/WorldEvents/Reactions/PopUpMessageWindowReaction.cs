using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.ScriptableObjects.Core;
using Game.MonoBehaviours.UI;

namespace Game.ScriptableObjects.WorldEvents.Reactions
{
    public class PopUpMessageWindowReaction : Reaction
    {
        /// <summary>
        /// Reference the window object that will interact with
        /// </summary>
        public UIMessageWindow MessageWindow;
        bool windowIsVisible;

        public override IEnumerator React()
        {
            // Reset window hidden state checker
            windowIsVisible = false;
            // Resgister to on window is hidden event and call pop down
            MessageWindow.OnWindowIsVisible().AddListener(WindowHasFinished);
            MessageWindow.PopUp();

            yield return WaitUntilWindowIsFinished();

            MessageWindow.OnWindowIsVisible().RemoveListener(WindowHasFinished);
        }

        /// <summary>
        /// Recieve feedback from message window
        /// </summary>
        void WindowHasFinished() => windowIsVisible = true;

        IEnumerator WaitUntilWindowIsFinished()
        {
            while (!windowIsVisible)
            {
                yield return null;
            }
        }
    }
}
