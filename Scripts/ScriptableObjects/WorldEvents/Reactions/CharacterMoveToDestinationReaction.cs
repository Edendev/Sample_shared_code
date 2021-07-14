using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.MonoBehaviours;
using Game.Generic.Entities;

namespace Game.ScriptableObjects.WorldEvents.Reactions
{
    public class CharacterMoveToDestinationReaction : CharacterRemoteControlReaction
    {
        public Vector3 Destination;
        bool destinationReached;

        public override IEnumerator React()
        {
            base.Initialize();

            destinationReached = false;

            // Register to on destination reached
            Character.OnDestinationReached().AddListener(CharacterHasReachedDestination);

            if (Character.StartRemoteControlAction(new MoveToDestination_RCA(Destination)))
            {
                // Start base react co-routine (this is done after starting the remote action to prevent stopping this co-routine)
                Character.StartCoroutine(base.React());

                yield return WaitUntilDdestinationIsReached();

                yield return WaitUntil_RC_ActionIIsFinished();

                if (DisableRemoteControl)
                    yield return WaitUntil_RC_IsDisabled();
            }

            // Remove litener
            Character.OnDestinationReached().RemoveListener(CharacterHasReachedDestination);

            yield return null;
        }

        /// <summary>
        /// Recieve feedback from character
        /// </summary>
        void CharacterHasReachedDestination() => destinationReached = true;

        IEnumerator WaitUntilDdestinationIsReached()
        {
            while (!destinationReached)
            {
                yield return null;
            }
        }
    }
}
