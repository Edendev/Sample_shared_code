using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.Editor.ScriptableObjects.Core;
using Game.ScriptableObjects.WorldEvents.Conditions;

namespace Game.Editor.ScriptableObjects.WorldEvents.Conditions
{
    [CustomEditor(typeof(Condition))]
    public abstract class ConditionEditor : GameSOEditor
    {
        /// <summary>
        /// Depending on the usage of the condition, the editor is defined differently
        /// Container are all the persistent conditions during the game.
        /// Comparison refers to the conditions that are used to check if one of the conditions in the container is satisfied.
        /// </summary>
        public enum ConditionEditorType
        {
            Container,
            Comparison,
        }

        public ConditionEditorType editorType = ConditionEditorType.Container;

        public SerializedProperty conditionToCompareProperty;

        private const string conditionToComparePropName = "ConditionToCheckWith";

        protected override void OnEnable()
        {
            base.OnEnable();

            conditionToCompareProperty = serializedObject.FindProperty(conditionToComparePropName);

            showDefaultInspector = false;
        }

        protected override sealed void ShowSpecificInspector()
        {
            serializedObject.Update();

            if (showEditor)
            {
                if (editorType == ConditionEditorType.Container)
                    OnContainerInspectorGUI();

                if (editorType == ConditionEditorType.Comparison)
                    OnComparisonInspectorGUI();
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnContainerInspectorGUI() { }
        protected virtual void OnComparisonInspectorGUI() { }

        protected override string GetFoldoutLabel()
        {
            return target.name;
        }
    }
}