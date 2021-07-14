using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.ScriptableObjects.Core;
using Game.Generic.Data;

namespace Game.ScriptableObjects.WorldEvents.Conditions
{
    public class BoolCondition : Condition
    {
        public bool Satisfied;

        public override bool Check()
        {
            BoolCondition toCompare = ConditionToCheckWith as BoolCondition;
            if (toCompare != null)
                return (Satisfied == toCompare.Satisfied);

            return false;
        }

        public override void Change()
        {
            BoolCondition toChange = ConditionToChangeWith as BoolCondition;
            if (toChange != null)
                Satisfied = toChange.Satisfied;

            base.Change();
        }

        #region IResetteable

        public override void Reset() => Satisfied = false;

        #endregion

        #region ISaveData

        public override void SaveFile()
        {
            BoolCondition_SaveFile saveFile = new BoolCondition_SaveFile();
            saveFile.Name = name;
            saveFile.Satisfied = Satisfied;
            SaveGameManager.Save(saveFile, GetPathFile());
        }
        public override void LoadDataFromFile()
        {
            BoolCondition_SaveFile savedFile = SaveGameManager.Load<BoolCondition_SaveFile>(GetPathFile());
            if (savedFile != null)
                Satisfied = savedFile.Satisfied;
        }
        public override void DeleteFile() => SaveGameManager.DeleteFile(GetPathFile());
        public override string GetPathFile() => SaveGameManager.ConditionsFilePath + "/" + SO_Name + ".asset";

        #endregion

        [System.Serializable]
        public class BoolCondition_SaveFile : Condition_SaveFile
        {
            public bool Satisfied;
        }
    }
}