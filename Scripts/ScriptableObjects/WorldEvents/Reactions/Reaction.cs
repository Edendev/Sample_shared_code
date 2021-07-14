using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.ScriptableObjects.Core;

namespace Game.ScriptableObjects.WorldEvents.Reactions
{
    public abstract class Reaction : GameSO
    {
        public abstract IEnumerator React();
    }
}