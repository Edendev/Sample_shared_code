//#define DEBUG_SAVEGAMEMANAGER

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Game.MonoBehaviours.Managers;
using Game.ScriptableObjects.WorldEvents.Conditions;
using Game.MonoBehaviours.Settings;
using Game.MonoBehaviours;

namespace Game.Generic.Data
{
    public static class SaveGameManager
    {
        /// <summary>
        /// Saving items
        /// </summary>
        public static readonly string ConditionsFilePath = Application.persistentDataPath + "/Conditions";
        public static readonly string SettingsFilePath = Application.persistentDataPath + "/Settings";

        [System.Flags]
        public enum DataType
        {
            None = 0,
            Conditions = 1,
            Settings = 2,
        }

        static SaveGameManager()
        {
            if (!Directory.Exists(ConditionsFilePath))
            {
                Directory.CreateDirectory(ConditionsFilePath);
            }

            if (!Directory.Exists(SettingsFilePath))
            {
                Directory.CreateDirectory(SettingsFilePath);
            }
        }

        public static void Save(object obj, string path)
        {
                string jsonSaveFile = JsonUtility.ToJson(obj, true);

                File.WriteAllText(path, jsonSaveFile);

#if DEBUG_SAVEGAMEMANAGER
                Debug.Log("SaveGameManager::Save - Path: " + path);
                Debug.Log("SaveGameManager::Save - JSON: " + jsonSaveFile);
#endif            
        }  
    
        public static T Load<T>(string path)
            where T : SaveFile
        {
                T savedFile = null;

                if (File.Exists(path))
                {
                    string dataAsJson = File.ReadAllText(path);

#if DEBUG_SAVEGAMEMANAGER
                    Debug.Log("SaveGameManager::Load - File text is:\n" + dataAsJson);
#endif
                    try
                    {
                        savedFile = JsonUtility.FromJson<T>(dataAsJson);
                    }
                    catch
                    {
                        Debug.LogWarning("SaveGameManager::Load - SaveFile was malformed.\n" + dataAsJson);
                        return null;
                    }

#if DEBUG_SAVEGAMEMANAGER
                    Debug.Log("SaveGameManager::Load - Load succesfully.\n" + dataAsJson);
#endif
                }

            return savedFile;
        }

        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);

                Debug.Log("SaveGameManager::DeleSavedData - Deleted file succesfully.\n");
            }
            else
            {
                Debug.Log("SaveGameManager::DeleSavedData - No file has been created yet.\n");
            }
        }

        [System.Serializable]
        public class SaveFile { }
    }
}

