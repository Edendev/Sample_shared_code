using System.Collections;
using Game.MonoBehaviours.UI;

namespace Game.ScriptableObjects.WorldEvents.Reactions
{
    public class SetTextMessageWindowReaction : Reaction
    {
        /// <summary>
        /// Reference the window object that will interact with
        /// </summary>
        public UIMessageWindow MessageWindow;
        public string Text;

        public override IEnumerator React()
        {
            // Set message in window
            MessageWindow.SetMessageText(Text);

            yield return null;
        }
    }
}
