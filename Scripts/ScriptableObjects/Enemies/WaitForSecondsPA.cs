using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.ScriptableObjects.Core;

namespace Game.ScriptableObjects.Enemies
{
    public class WaitForSecondsPA : PatrolAction
    {
        public float Delay;
        
        public override IEnumerator Execute(GameObject target)
        {
            yield return new WaitForSeconds(Delay);
        }
    }
}