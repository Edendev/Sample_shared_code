using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.ScriptableObjects.Core;

namespace Game.Editor.ScriptableObjects.Core
{
    [CustomEditor(typeof(GameSO))]
    public abstract class GameSOEditor : UnityEditor.Editor
    {
        protected virtual void OnEnable()
        {
            if (target == null)
            {
                DestroyImmediate(this);
                return;
            }
        }

        protected bool showEditor = false;
        protected bool showDefaultInspector = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            showEditor = EditorGUILayout.Foldout(showEditor, GetFoldoutLabel());

            if (showEditor)
            {
                if (showDefaultInspector)
                {
                    DrawDefaultInspector();
                }
                else
                {
                    ShowSpecificInspector();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected abstract void ShowSpecificInspector();
        protected abstract string GetFoldoutLabel();

        #region Accessors

        public bool ShowEditorState() => showEditor;

        #endregion
    }
}

