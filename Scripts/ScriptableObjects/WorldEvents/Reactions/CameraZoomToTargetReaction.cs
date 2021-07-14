using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.ScriptableObjects.Core;
using Game.MonoBehaviours.UI;
using Game.MonoBehaviours;

namespace Game.ScriptableObjects.WorldEvents.Reactions
{
    public class CameraZoomToTargetReaction : Reaction
    {
        public CameraController CameraController;
        public Transform Target;
        [Range(0f, 20f), Tooltip("Speed used to move the camera to the target before zooming: in m/s")]
        public float MovingSpeed;
        [Range(0.1f, 50f), Tooltip("Speed in OrthographizSize units per second")]
        public float ZoomingSpeed;
        [Range(0.5f, 10f)]
        public float Depth;
        bool zoomDone;
        bool targetReached;

        public override IEnumerator React()
        {
            zoomDone = false;
            targetReached = false;

            // Register to camera target reached and zoom in
            CameraController.OnZoomingDone().AddListener(CameraZoomingDone);
            CameraController.OnTargetReached().AddListener(CameraTargetReached);

            if (CameraController.ZoomToTarget(Target, MovingSpeed, ZoomingSpeed, Depth))
            {
                // Wait until both processes are completed
                yield return WaitUntilZoomingIsReached();
                yield return WaitUntilTargetIsReached();
            }

            // Remove listeners
            CameraController.OnZoomingDone().RemoveListener(CameraZoomingDone);
            CameraController.OnTargetReached().RemoveListener(CameraTargetReached);

            yield return null;
        }

        /// <summary>
        /// Recieve feedback from cameraController
        /// </summary>
        void CameraZoomingDone() => zoomDone = true;
        void CameraTargetReached() => targetReached = true;

        IEnumerator WaitUntilZoomingIsReached()
        {
            while (!zoomDone)
            {
                yield return null;
            }
        }
        IEnumerator WaitUntilTargetIsReached()
        {
            while (!targetReached)
            {
                yield return null;
            }
        }
    }
}