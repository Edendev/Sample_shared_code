using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.Editor.ScriptableObjects.Core;
using Game.ScriptableObjects.WorldEvents.Conditions;
using Game.ScriptableObjects.WorldEvents.Reactions;

namespace Game.Editor.ScriptableObjects.WorldEvents.Reactions
{
    [CustomEditor(typeof(ChangeConditionStateReaction))]
    public class ChangeConditionStateReactionEditor : ReactionEditor
    {
        public SerializedProperty conditionToChangeProperty;
        public SerializedProperty newConditionStateProperty;

        private const string conditionToChangePropName = "Condition";
        private const string newConditionStatePropName = "NewConditionState";

        ChangeConditionStateReaction changeCondStateReact;

        UnityEditor.Editor newConditionStateEditor;

        protected override void OnEnable()
        {
            base.OnEnable();

            conditionToChangeProperty = serializedObject.FindProperty(conditionToChangePropName);
            newConditionStateProperty = serializedObject.FindProperty(newConditionStatePropName);

            changeCondStateReact = (ChangeConditionStateReaction)target;
            CheckAndCreateNewConditionStateEditor();
            showDefaultInspector = false;
        }

        void CheckAndCreateNewConditionStateEditor()
        {
            if (newConditionStateEditor != null)
                DestroyImmediate(newConditionStateEditor);

            newConditionStateEditor = null;

            if (changeCondStateReact.NewConditionState != null)
                newConditionStateEditor = UnityEditor.Editor.CreateEditor(changeCondStateReact.NewConditionState);
        }

        protected override void ShowSpecificInspector()
        {
            base.ShowSpecificInspector();

            serializedObject.Update();

            EditorGUILayout.PropertyField(conditionToChangeProperty);

            if (changeCondStateReact.Condition != null)
            {
                if (changeCondStateReact.NewConditionState == null)
                {
                    DestroyImmediate(changeCondStateReact.NewConditionState);
                    changeCondStateReact.NewConditionState = Instantiate(changeCondStateReact.Condition);
                    CheckAndCreateNewConditionStateEditor();
                }
                else if (changeCondStateReact.Condition.GetType() != changeCondStateReact.NewConditionState.GetType())
                {
                    DestroyImmediate(changeCondStateReact.NewConditionState);
                    changeCondStateReact.NewConditionState = Instantiate(changeCondStateReact.Condition);
                    CheckAndCreateNewConditionStateEditor();
                }
            }
            else
            {
                if (changeCondStateReact.NewConditionState != null)
                {
                    DestroyImmediate(changeCondStateReact.NewConditionState);
                    CheckAndCreateNewConditionStateEditor();
                }
            }

            if (newConditionStateEditor != null)
                newConditionStateEditor.OnInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
