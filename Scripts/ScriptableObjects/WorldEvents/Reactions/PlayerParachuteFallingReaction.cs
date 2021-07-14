using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.MonoBehaviours;
using Game.MonoBehaviours.WorldEvents;

namespace Game.ScriptableObjects.WorldEvents.Reactions
{
    public class PlayerParachuteFallingReaction : Reaction
    {
        /// <summary>
        /// Parachute falling object that will be instantiated
        /// </summary>
        public ParachuteFalling ParachuteFalling;
        public Vector3 StartFallingPosition;

        public override IEnumerator React()
        {
            // Move player to save location
            Player.S.TeleportToPosition(Vector3.zero);
            ParachuteFalling newParachuteFalling = Instantiate(ParachuteFalling, StartFallingPosition, Quaternion.Euler(0,-90,0));
            Camera mainCamera = Camera.main;
            mainCamera.tag = "Untagged";
            newParachuteFalling.FollowingCamera.tag = "MainCamera";

            Animator animator = newParachuteFalling.GetComponent<Animator>();

            yield return new WaitForEndOfFrame(); // To enable the animator entering the state
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length / animator.GetCurrentAnimatorStateInfo(0).speed);

            newParachuteFalling.FollowingCamera.tag = "Untagged";
            Player.S.TeleportToPosition(newParachuteFalling.transform.position);
            Destroy(newParachuteFalling.gameObject);

            mainCamera.tag = "MainCamera";

            yield return null;
        }
    }
}
