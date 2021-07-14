using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.Editor.ScriptableObjects.Core;
using Game.ScriptableObjects.WorldEvents.Conditions;

namespace Game.Editor.ScriptableObjects.WorldEvents.Conditions
{
    [CustomEditor(typeof(BoolCondition))]
    public class BoolConditionEditor : ConditionEditor
    {
        public SerializedProperty satisfiedProperty;

        private const string satisfiedPropName = "Satisfied";

        protected override void OnEnable()
        {
            base.OnEnable();

            satisfiedProperty = serializedObject.FindProperty(satisfiedPropName);
        }

        protected override void OnComparisonInspectorGUI()
        {
            base.OnComparisonInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(satisfiedProperty);
            EditorGUILayout.PropertyField(conditionToCompareProperty);

            serializedObject.ApplyModifiedProperties();
        }

        protected override void OnContainerInspectorGUI()
        {
            base.OnContainerInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(satisfiedProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
