using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.ScriptableObjects.Core;

namespace Game.ScriptableObjects.Enemies
{
    public class FaceDirectionPA : PatrolAction
    {
        /// <summary>
        /// Direction to look at, the direction is normalized and added to the current position of the 
        /// target.
        /// </summary>
        public Vector3 Direction;
        public float rotationTime = 1f;
        public float waitForSecondsAfterFacing = 0f;
        
        public override IEnumerator Execute(GameObject target)
        {
            int count = 0;
            float steps = rotationTime / 0.05f;
            Quaternion iniRotation = target.transform.rotation;
            Quaternion finalRotation = Quaternion.LookRotation(Direction.normalized);
            while (count <= steps)
            {
                target.transform.rotation = Quaternion.Lerp(iniRotation, finalRotation, count / steps);
                count++;
                yield return new WaitForSeconds(rotationTime/steps);
            }

            if (waitForSecondsAfterFacing > 0)
                yield return new WaitForSeconds(waitForSecondsAfterFacing);

            yield return null;
        }
    }
}
