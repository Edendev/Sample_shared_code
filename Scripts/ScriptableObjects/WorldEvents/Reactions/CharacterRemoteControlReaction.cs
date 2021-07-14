using System.Collections;
using UnityEngine;
using Game.MonoBehaviours;
using Game.Generic.Entities;

namespace Game.ScriptableObjects.WorldEvents.Reactions
{
    public abstract class CharacterRemoteControlReaction : Reaction
    {
        public Character Character;
        /// <summary>
        /// Must disabled the remote control after this reaction is finished?
        /// </summary>
        public bool DisableRemoteControl = false;
        bool rcActionFinished;
        bool rcDisabled;

        protected void Initialize()
        {
            if (DisableRemoteControl)
            {
                rcDisabled = false;
                // Register to on disabled
                Character.OnRemoteControlDisabled().AddListener(RemoteControlHasBeenDisabled);
            }

            rcActionFinished = false;

            // Register to on destination reached
            Character.OnRemoteControlActionFinished().AddListener(RemoteControlActionsHasFinished);
        }

        public override IEnumerator React()
        {
            // Wait until action is finished
            yield return WaitUntil_RC_ActionIIsFinished();

            // Remove litener
            Character.OnRemoteControlActionFinished().RemoveListener(RemoteControlActionsHasFinished);

            if (DisableRemoteControl)
            {
                // Disable remote control
                if (Character.DisableRemoteControlExternally())
                {
                    yield return WaitUntil_RC_IsDisabled();
                }
                else
                {
                    rcDisabled = true;
                }

                // Remove litener
                Character.OnRemoteControlDisabled().RemoveListener(RemoteControlHasBeenDisabled);
            }

            yield return null;
        }

        /// <summary>
        /// Recieve feedback from character
        /// </summary>
        void RemoteControlActionsHasFinished() => rcActionFinished = true;
        protected IEnumerator WaitUntil_RC_ActionIIsFinished()
        {
            while (!rcActionFinished)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Recieve feedback from character
        /// </summary>
        void RemoteControlHasBeenDisabled() => rcDisabled = true;
        protected IEnumerator WaitUntil_RC_IsDisabled()
        {
            while (!rcDisabled)
            {
                yield return null;
            }
        }
    }
}
