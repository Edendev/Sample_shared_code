using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.ScriptableObjects.Core;
using Game.MonoBehaviours.UI;
using Game.MonoBehaviours;

namespace Game.ScriptableObjects.WorldEvents.Reactions
{
    public class MoveCameraToTargetReaction : Reaction
    {
        public CameraController CameraController;
        public Transform Target;
        [Range(0.1f, 50f)]
        public float Speed;
        public bool Instantly = false;
        bool targetReached;

        public override IEnumerator React()
        {
            if (!Instantly)
            {
                targetReached = false;

                // Register to on target reached
                CameraController.OnTargetReached().AddListener(CameraHasReachedTarget);

                if (CameraController.MoveCameraToTarget(Target, Speed))
                    yield return WaitUntilTargetIsReached();

                // Remove litener
                CameraController.OnTargetReached().RemoveListener(CameraHasReachedTarget);
            }
            else
            {
                CameraController.MoveCameraToTarget(Target, Speed, true);
            }

            yield return null;
        }

        /// <summary>
        /// Recieve feedback from cameraController
        /// </summary>
        void CameraHasReachedTarget() => targetReached = true;

        IEnumerator WaitUntilTargetIsReached()
        {
            while (!targetReached)
            {
                yield return null;
            }
        }
    }
}