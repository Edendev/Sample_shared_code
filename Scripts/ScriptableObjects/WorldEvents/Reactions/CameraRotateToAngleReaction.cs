using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.ScriptableObjects.Core;
using Game.MonoBehaviours.UI;
using Game.MonoBehaviours;

namespace Game.ScriptableObjects.WorldEvents.Reactions
{
    public class CameraRotateToAngleReaction : Reaction
    {
        public CameraController CameraController;
        [Range(0f, 360f), Tooltip("Speed used to rotate the camera to the target: in deg/s")]
        public float RotationSpeed;
        [Range(0f, 360f), Tooltip("Angle to rotate towards in degrees. Represents the Y euler angle of the object")]
        public float Angle;
        public bool Instantly;
        bool rotationDone;

        public override IEnumerator React()
        {
            rotationDone = false;

            // Register to camera rotation done
            CameraController.OnRotationDone().AddListener(CameraRotationDone);

            if (CameraController.RotateToAngle(RotationSpeed, Angle, Instantly))
                yield return WaitUntilRotationIsDone();

            // Remove listeners
            CameraController.OnRotationDone().RemoveListener(CameraRotationDone);

            yield return null;
        }

        /// <summary>
        /// Recieve feedback from cameraController
        /// </summary>
        void CameraRotationDone() => rotationDone = true;

        IEnumerator WaitUntilRotationIsDone()
        {
            while (!rotationDone)
            {
                yield return null;
            }
        }
    }
}