using System.Collections;
using UnityEngine;
using Game.MonoBehaviours;
using Game.Generic.Entities;

namespace Game.ScriptableObjects.WorldEvents.Reactions
{
    public class CharacterEnableRemoteControlReaction : Reaction
    {
        public Character Character;
        bool remoteControlEnabled;

        public override IEnumerator React()
        {
            remoteControlEnabled = false;

            // Register to on remote control enabled
            Character.OnRemoteControlEnabled().AddListener(RemoteControlHasBeenEnabled);

            if (Character.EnableRemoteControlExternally())
            {
                yield return WaitUntil_RC_IsEnabled();
            }
            else
            {
                remoteControlEnabled = true;
            }

            // Remove litener
            Character.OnRemoteControlEnabled().RemoveListener(RemoteControlHasBeenEnabled);

            yield return null;
        }

        /// <summary>
        /// Recieve feedback from character
        /// </summary>
        void RemoteControlHasBeenEnabled() => remoteControlEnabled = true;

        protected IEnumerator WaitUntil_RC_IsEnabled()
        { 
            while (!remoteControlEnabled)
            {
                yield return null;
            }
        }
    }
}
