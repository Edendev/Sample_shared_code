using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Game.Generic.Data;
using Game.ScriptableObjects.Core;
using Game.ScriptableObjects.WorldEvents.Conditions;

namespace Game.ScriptableObjects.Containers
{
    public class ConditionsContainer : GameSOContainer<Condition>
    {
        public void SaveDataToFiles()
        {
            if (SO_objects != null && SO_objects.Length > 0)
            {
                for (int i = 0; i < SO_objects.Length; i++)
                {
                    SO_objects[i].SaveFile();
                }
            }
        }
        public void LoadDataFromSavedFiles()
        {
            if (SO_objects != null && SO_objects.Length > 0)
            {
                for (int i = 0; i < SO_objects.Length; i++)
                {
                    SO_objects[i].LoadDataFromFile();
                }
            }
        }
        public void DeleteAllSavedFiles()
        {
            if (SO_objects != null && SO_objects.Length > 0)
            {
                for (int i = 0; i < SO_objects.Length; i++)
                {
                    SO_objects[i].DeleteFile();
                }
            }
        }
        public string[] GetAllSavedPathFiles()
        {
            string[] pathFiles = new string[0];
            if (SO_objects != null && SO_objects.Length > 0)
            {
                pathFiles = new string[SO_objects.Length];
                for (int i = 0; i < SO_objects.Length; i++)
                {
                    pathFiles[i] = SO_objects[i].GetPathFile();
                }
            }

            return pathFiles;
        }

        public void ResetAllConditions()
        {
            if (SO_objects != null && SO_objects.Length > 0)
            {
                for (int i = 0; i < SO_objects.Length; i++)
                {
                    SO_objects[i].Reset();
                }
            }
        }
    }
}

