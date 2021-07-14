using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.ScriptableObjects.Core;
using Game.ScriptableObjects.WorldEvents.Reactions;
using Game.ScriptableObjects.WorldEvents.EventTriggers;
using Game.ScriptableObjects.Inventory;

namespace Game.Editor.ScriptableObjects
{
    public class GameSOCreatorWindow : EditorWindow
    {
        string gameSO_Name = "";
        private List<System.Type> listTypes = new List<System.Type>();

        [MenuItem("Window/GameSO_Creator")]
        static void Init()
        {
            GetWindow(typeof(GameSOCreatorWindow), false, "GameSO_Creator");
        }

        private void OnEnable()
        {
            // Get all GameSO types
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(typeof(GameSO)) && !type.IsSubclassOf(typeof(Reaction)) && !type.IsSubclassOf(typeof(EventTrigger)) && !type.IsAbstract)
                    {
                        listTypes.Add(type);
                    }
                }
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(new GUIContent("SO unique name"));
            gameSO_Name = EditorGUILayout.TextField(gameSO_Name);

            EditorGUILayout.Space();

            foreach (System.Type type in listTypes)
            {
                if (GUILayout.Button(string.Format("Create {0}", type.Name)))
                {
                    GameSO.CreateSO(type, gameSO_Name);
                }
            }
        }
    }
}

