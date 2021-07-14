using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.Editor.ScriptableObjects.Core;
using Game.ScriptableObjects.WorldEvents.Reactions;

namespace Game.Editor.ScriptableObjects.WorldEvents.Reactions
{
    [CustomEditor(typeof(SetTextMessageWindowReaction))]
    public class SetTextMessageWindowReactionEditor : ReactionEditor
    {
        public SerializedProperty messageWindowProperty;
        public SerializedProperty textProperty;

        private const string messageWindowPropName = "MessageWindow";
        private const string textPropName = "Text";

        protected override void OnEnable()
        {
            base.OnEnable();

            messageWindowProperty = serializedObject.FindProperty(messageWindowPropName);
            textProperty = serializedObject.FindProperty(textPropName);

            showDefaultInspector = false;
        }

        protected override void ShowSpecificInspector()
        {
            base.ShowSpecificInspector();

            serializedObject.Update();

            EditorGUILayout.PropertyField(messageWindowProperty);
            textProperty.stringValue = GUILayout.TextArea(textProperty.stringValue, 300);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
