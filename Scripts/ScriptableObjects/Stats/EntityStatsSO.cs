using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.ScriptableObjects.Core;

namespace Game.ScriptableObjects.Stats
{
    public abstract class EntityStatsSO : GameSO
    {
        [Header("_Entity Stats")]
        [SerializeField] private float resistance = 10;

        #region Accessors

        public float Resistance() => resistance;

        #endregion
    }
}

