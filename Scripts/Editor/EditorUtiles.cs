using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.ScriptableObjects.Core;

namespace Game.Editor
{
    public static class EditorUtiles
    {
        // GUI style
        private const float addButtonWidth = 80f;
        private const float removeButtonWidth = 80f;
        private const float upAndDownButtonWidth = 30f;

        #region GameSO_Utiles

        public static bool AddElementFromArrayGUI<T>(ref T[] array, System.Type[] listTypes, int index, string objectName)
            where T : GameSO
        {
            if (GUILayout.Button("Add", GUILayout.Width(addButtonWidth)))
            {
                System.Type type = listTypes[index];
                GameSO.CreateInstanceOfTypeInArray(ref array, type, objectName);
                return true;
            }

            return false;
        }
        public static bool AddElementGUI<T>(ref T container, System.Type[] listTypes, int index, string objectName)
             where T : GameSO
        {
            if (GUILayout.Button("Add", GUILayout.Width(addButtonWidth)))
            {
                System.Type type = listTypes[index];
                GameSO.CreateInstanceOfTypeInContainer(ref container, type, objectName);
                return true;
            }

            return false;
        }
        public static bool RemoveElementFromArrayGUI<T>(ref T[] array, T element)
           where T : GameSO
        {
            if (GUILayout.Button("Remove", GUILayout.Width(removeButtonWidth)))
            {
                GameSO.RemoveInstanceOfTypeInArray(ref array, element);
                return true;
            }

            return false;
        }
        public static bool RemoveElementGUI<T>(T element)
           where T : GameSO
        {
            if (GUILayout.Button("Remove", GUILayout.Width(removeButtonWidth)))
            {
                UnityEditor.Editor.DestroyImmediate(element);
                return true;
            }

            return false;
        }
        public static bool MoveUpElementGUI<T>(ref T[] array, T element)
           where T : GameSO
        {
            if (GUILayout.Button("↑", GUILayout.Width(upAndDownButtonWidth)))
            {
                int elementIndex = ArrayUtility.FindIndex(array, x => x == element);
                if (elementIndex > 0)
                {
                    T savedElement = array[elementIndex - 1];
                    array[elementIndex - 1] = element;
                    array[elementIndex] = savedElement;
                }
                return true;
            }

            return false;
        }
        public static bool MoveDownElementGUI<T>(ref T[] array, T element)
           where T : GameSO
        {
            if (GUILayout.Button("↓", GUILayout.Width(upAndDownButtonWidth)))
            {
                int elementIndex = ArrayUtility.FindIndex(array, x => x == element);
                if (elementIndex < array.Length - 1)
                {
                    T savedElement = array[elementIndex + 1];
                    array[elementIndex + 1] = element;
                    array[elementIndex] = savedElement;
                }
                return true;
            }

            return false;
        }

        #endregion
    }

}
