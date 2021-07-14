#define DEBUG_GAMESO
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Game.ScriptableObjects.Inventory;
using Game.ScriptableObjects.Combat;
using Game.ScriptableObjects.Stats;
using Game.ScriptableObjects.UI;
using Game.ScriptableObjects.WorldEvents.Conditions;

namespace Game.ScriptableObjects.Core
{
    public abstract class GameSO : ScriptableObject
    {
        /// <summary>
        /// Unique name for the scriptable object instance. 
        /// </summary>
        public string SO_Name;
        /// <summary>
        /// Name used in game for the scriptable object
        /// </summary>
        [SerializeField] string SO_InGameName = "";

        /// <summary>
        /// Look at pathes for the creation of the different scriptable objects
        /// </summary>
        static readonly Dictionary<System.Type, string> CreationPathes = new Dictionary<System.Type, string>()
        {
            {typeof(ItemDefinition), "Assets/SO_Instances/ItemDefinitions/"},
            {typeof(WeaponAttack), "Assets/SO_Instances/AttackDefinitions/"},
            {typeof(CharacterStatsSO), "Assets/SO_Instances/Stats/"},
            {typeof(DestroyableEntityStatsSO), "Assets/SO_Instances/Stats/"},
            {typeof(MeleeAttack), "Assets/SO_Instances/AttackDefinitions/"},
            {typeof(SwordAttack), "Assets/SO_Instances/AttackDefinitions/"},
            {typeof(RangedAttack), "Assets/SO_Instances/AttackDefinitions/"},
            {typeof(BoolCondition), "Assets/SO_Instances/WorldEvents/Conditions/"},
            {typeof(Rope), "Assets/SO_Instances/Ropes/"},
            {typeof(DialoguePortraits), "Assets/SO_Instances/DialoguePortraits/"},
            {typeof(CursorTextures), "Assets/SO_Instances/CursorTextures/"},
        };

#if UNITY_EDITOR
        /// <summary>
        /// Creates an instance and store it in the assets database in the specific folder from the dictionary
        /// </summary>
        public static void CreateSO(System.Type type, string objectName)
        {
            if (!type.IsSubclassOf(typeof(GameSO)) || type.IsAbstract)
                return;

            // Check if object name exists
            if (SO_NameExists(type, objectName))
            {
#if DEBUG_GAMESO
                Debug.Log("Object name already exists, please chose another one");
#endif
                return;
            }

            GameSO GameSO_Object = CreateInstance(type) as GameSO;
            GameSO_Object.SO_Name = objectName;

            // Get creation path
            string creationPath = "";
            if(!CreationPathes.TryGetValue(type, out creationPath))
                creationPath = "Assets/SO_Instances/";

            creationPath = string.Format("{0}{1}.asset", creationPath, objectName);

            AssetDatabase.CreateAsset(GameSO_Object, creationPath);
            AssetDatabase.SaveAssets();
        }

        static bool SO_NameExists(System.Type type, string objectName)
        {
            if (!type.IsSubclassOf(typeof(GameSO)) || type.IsAbstract)
                return false;

            string[] SO_objectNames = AssetDatabase.FindAssets("t:" + type.Name);

            GameSO[] SO_objects = new GameSO[SO_objectNames.Length];
            for (int i = 0; i < SO_objectNames.Length; i++)  
            {
                string path = AssetDatabase.GUIDToAssetPath(SO_objectNames[i]);
                SO_objects[i] = AssetDatabase.LoadAssetAtPath(path, type) as GameSO;
            }

            for (int i = 0; i < SO_objects.Length; i++)
            {
                if (SO_objects[i].SO_Name == objectName)
                {
                    return true;
                }
            }

            return false;
        }

        public static T[] GetAllAssetsOfType<T>()
            where T : GameSO
        {
            string[] SO_objectNames = AssetDatabase.FindAssets("t:" + typeof(T).Name);

            T[] SO_objects = new T[SO_objectNames.Length];
            for (int i = 0; i < SO_objectNames.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(SO_objectNames[i]);
                SO_objects[i] = AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T;
            }

            return SO_objects;
        }

        #region Instances_Creation
        /// <summary>
        /// Creates and returns an instance. Return null if fails
        /// </summary>
        public static T CreateSOInstance<T>(string objectName)
            where T : GameSO
        {
            if (typeof(T).IsAbstract)
                return null;

            T newInstance = CreateInstance(typeof(T)) as T;
            newInstance.name = objectName;

            return newInstance;
        }

        /// <summary>
        /// Creates and returns an instance. Return null if fails
        /// </summary>
        public static GameSO CreateSOInstance(System.Type type, string objectName)
        {
            if (!type.IsSubclassOf(typeof(GameSO)) || type.IsAbstract)
                return null;

            GameSO newInstance = CreateInstance(type) as GameSO;
            newInstance.name = objectName;

            return newInstance;
        }
        /// <summary>
        /// Tries to create an instance of type "type" in an array of generic type T. The instance must be a subclass of GameSO
        /// </summary>
        /// <typeparam name="T"> Generic type </typeparam>
        /// <param name="array"> Array container </param>
        /// <param name="type"> Object type </param>
        public static void CreateInstanceOfTypeInArray<T>(ref T[] array, System.Type type, string objectName)
            where T : GameSO
        {
            T newInstance = CreateSOInstance(type, objectName) as T;
            if (newInstance != null)
                ArrayUtility.Add<T>(ref array, newInstance);

            // Clean array to check for null elements
            CleanArray(ref array);
        }
        public static void RemoveInstanceOfTypeInArray<T>(ref T[] array, T toRemove)
            where T : GameSO
        {
            if (toRemove != null)
                ArrayUtility.Remove<T>(ref array, toRemove);

            // Clean array to check for null elements
            CleanArray(ref array);
        }
        public static void CleanArray<T>(ref T[] array)
        {
            foreach (T element in array)
            {
                if (element == null)
                    ArrayUtility.Remove(ref array, element);
            }
        }
        public static void CreateInstanceOfTypeInContainer<T>(ref T container, System.Type type, string objectName)
            where T : GameSO
        {
            T newInstance = CreateSOInstance(type, objectName) as T;
            if (newInstance != null)
                container = newInstance;
        }

        #endregion

#endif

        #region Accessors

        public string GetSO_Name() => SO_Name;
        public string GetSO_InGameName() => SO_InGameName;

        #endregion
    }
}

