using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Interfaces;
using Game.ScriptableObjects.Core;
using Game.Generic.Data;

namespace Game.ScriptableObjects.WorldEvents.Conditions
{
    public abstract class Condition : GameSO, ISaveData, IResetteable
    {
        /// <summary>
        /// This conditions is the reference to the condition to be compared with
        /// </summary>
        public Condition ConditionToCheckWith;
        public Condition ConditionToChangeWith;

        public abstract bool Check();
        public virtual void Change() => SaveFile();

        #region IResetteable

        public abstract void Reset();

        #endregion

        #region ISaveData

        public abstract void SaveFile();
        public abstract void LoadDataFromFile();
        public abstract void DeleteFile();
        public abstract string GetPathFile();

        #endregion

        [System.Serializable]
        public class Condition_SaveFile : SaveGameManager.SaveFile
        {
            public string Name;
        }
    }
}

