using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.ScriptableObjects.Core;

namespace Game.ScriptableObjects.Enemies
{
    [System.Serializable]
    public abstract class PatrolAction : GameSO
    {
        public abstract IEnumerator Execute(GameObject target);
    }
}

