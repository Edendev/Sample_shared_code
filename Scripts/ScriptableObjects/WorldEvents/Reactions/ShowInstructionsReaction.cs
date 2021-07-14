using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.ScriptableObjects.Core;
using Game.MonoBehaviours.UI;

namespace Game.ScriptableObjects.WorldEvents.Reactions
{
    public class ShowInstructionsReaction : Reaction
    {
        /// <summary>
        /// Reference the window object that will interact with
        /// </summary>
        public UIInstructionsWindow InstructionsWindow;
        public string Text;
        public float ShowingTime = 5f;
        bool windowVisibleState;

        public override IEnumerator React()
        {
            // Set instructions in window
            InstructionsWindow.SetInstructionsText(Text);

            // Reset window state checker
            windowVisibleState = InstructionsWindow.WindowVisibleState();

            if (windowVisibleState == false)
            {
                // Resgister to on window is visible event and call pop up
                InstructionsWindow.OnWindowIsVisible().AddListener(WindowIsVisible);
                InstructionsWindow.PopUp();

                yield return WaitUntilWindowIsOnState(true);

                InstructionsWindow.OnWindowIsVisible().RemoveListener(WindowIsVisible);
            }

            yield return new WaitForSeconds(ShowingTime);

            // Resgister to on window is hidden event and call pop down
            InstructionsWindow.OnWindowIsHidden().AddListener(WindowIsHidden);
            InstructionsWindow.PopDown();

            yield return WaitUntilWindowIsOnState(false);

            InstructionsWindow.OnWindowIsHidden().RemoveListener(WindowIsHidden);
        }

        /// <summary>
        /// Recieve feedback from message window
        /// </summary>
        void WindowIsVisible() => windowVisibleState = true;
        void WindowIsHidden() => windowVisibleState = false;

        IEnumerator WaitUntilWindowIsOnState(bool state)
        {
            while (windowVisibleState != state)
            {
                yield return null;
            }
        }
    }
}
