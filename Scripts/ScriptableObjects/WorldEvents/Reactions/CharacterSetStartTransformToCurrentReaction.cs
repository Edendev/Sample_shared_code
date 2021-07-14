using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.MonoBehaviours;
using Game.Generic.Entities;

namespace Game.ScriptableObjects.WorldEvents.Reactions
{
    public class CharacterSetStartTransformToCurrentReaction : CharacterRemoteControlReaction
    {
        public override IEnumerator React()
        {
            base.Initialize();

            if (Character.StartRemoteControlAction(new UpdateStartingTransformToCurrent_RCA()))
            {
                // Start base react co-routine (this is done after starting the remote action to prevent stopping this co-routine)
                Character.StartCoroutine(base.React());

                yield return WaitUntil_RC_ActionIIsFinished();

                if (DisableRemoteControl)
                    yield return WaitUntil_RC_IsDisabled();
            }

            yield return null;
        }
    }
}
