using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.Generic.Utiles;
using Game.MonoBehaviours.WorldEvents;
using Game.ScriptableObjects.Core;
using Game.ScriptableObjects.WorldEvents.EventTriggers;
using Game.ScriptableObjects.WorldEvents.Conditions;
using Game.ScriptableObjects.WorldEvents.Reactions;
using Game.Editor.ScriptableObjects.WorldEvents.Conditions;
using Game.Editor.ScriptableObjects.WorldEvents.EventTriggers;
using Game.Editor.ScriptableObjects.WorldEvents.Reactions;
using Game.Editor;

namespace Game.Editor.MonoBehaviours.WorldEvents
{
    [CustomEditor(typeof(WorldEvent))]
    public class WorldEventEditor : UnityEditor.Editor
    {
        public SerializedProperty triggerProperty;
        public SerializedProperty conditionsProperty;
        public SerializedProperty reactionsProperty;
        public SerializedProperty eventInfoProperty;

        private const string triggerPropName = "EventTrigger";
        private const string conditionsPropName = "Conditions";
        private const string reactionsPropName = "Reactions";
        private const string eventInfoPropName = "EventInfo";

        WorldEvent worldEvent;

        // GUI style
        private GUIStyle labelStyle;

        // Store all types subclass of event trigger, conditions and reactions.
        System.Type[] allEventTriggerTypes;
        System.Type[] allConditionsTypes;
        System.Type[] allReactionsTypes;

        // GUI contents to show in inspector for all types subclass of event trigger, conditions and reactions.
        GUIContent[] allEventTriggerNames;
        GUIContent[] allConditionsNames;
        GUIContent[] allReactionsNames;

        // Subeditors
        UnityEditor.Editor eventTriggerEditor;
        UnityEditor.Editor[] conditionEditors;
        UnityEditor.Editor[] reactionEditors;

        // Index selected
        int eventTriggerIndexSelected = 0;
        int conditionIndexSelected = 0;
        int reactionIndexSelected = 0;

        protected virtual void OnEnable()
        {
            if (target == null)
            {
                DestroyImmediate(this);
                return;
            }

            worldEvent = (WorldEvent)target;

            triggerProperty = serializedObject.FindProperty(triggerPropName);
            conditionsProperty = serializedObject.FindProperty(conditionsPropName);
            reactionsProperty = serializedObject.FindProperty(reactionsPropName);
            eventInfoProperty = serializedObject.FindProperty(eventInfoPropName);

            // Get all even triggers, conditions and reactions subtypes and also names
            allEventTriggerTypes = Utiles.GetAllTypesSubclassOfT<EventTrigger>();
            allConditionsTypes = Utiles.GetAllTypesSubclassOfT<Condition>();
            allReactionsTypes = Utiles.GetAllTypesSubclassOfT<Reaction>();
            allEventTriggerNames = Utiles.GetAllTypeNamesSubclassOfT<EventTrigger>();
            allConditionsNames = Utiles.GetAllTypeNamesSubclassOfT<Condition>();
            allReactionsNames = Utiles.GetAllTypeNamesSubclassOfT<Reaction>();

            // Check and create subeditors
            CheckAndCreateConditionEditors();
            CheckAndCreateReactionEditors();
            CheckAndCreateEventTriggerEditor();
        }

        private void OnDisable()
        {
            CleanEventTriggerSubEditor();
            CleanConditionSubEditors();
            CleanReactionSubEditors();
        }

        void CheckAndCreateEventTriggerEditor()
        {
            // Clean editor if exists
            if (eventTriggerEditor != null)
                DestroyImmediate(eventTriggerEditor);

            eventTriggerEditor = null;

            if (worldEvent.EventTrigger == null)
                return;

            // Create new editor
            eventTriggerEditor = UnityEditor.Editor.CreateEditor(worldEvent.EventTrigger);
            // If not editor for this specific editor exists, it will create a generic inspector not macthing with the EventTriggerEditor
            // is so, create a EventTriggerEditor
            if (!eventTriggerEditor.GetType().IsSubclassOf(typeof(EventTriggerEditor)))
                eventTriggerEditor = UnityEditor.Editor.CreateEditor(worldEvent.EventTrigger, typeof(EventTriggerEditor));
        }
        void CheckAndCreateConditionEditors()
        {
            // Clean editor if exists
            if (conditionEditors != null && conditionEditors.Length > 0)
            {
                foreach(UnityEditor.Editor conditionEditor in conditionEditors)
                {
                    DestroyImmediate(conditionEditor);
                }
            }

            conditionEditors = null;

            // Create new editors
            conditionEditors = new UnityEditor.Editor[worldEvent.Conditions.Length];
            for (int i = 0; i < conditionEditors.Length; i++)
            {
                if (worldEvent.Conditions[i] != null)
                {
                    conditionEditors[i] = UnityEditor.Editor.CreateEditor(worldEvent.Conditions[i]);
                    // If not editor for this specific editor exists, it will create a generic inspector not macthing with the ConditionEditor
                    // is so, create a ConditionEditor
                    if (!conditionEditors[i].GetType().IsSubclassOf(typeof(ConditionEditor)))
                        conditionEditors[i] = UnityEditor.Editor.CreateEditor(worldEvent.Conditions[i], typeof(ConditionEditor));

                    ConditionEditor conditionEditor = conditionEditors[i] as ConditionEditor;
                    conditionEditor.editorType = ConditionEditor.ConditionEditorType.Comparison;
                }
            }
        }
        void CheckAndCreateReactionEditors()
        {
            // Clean editor if exists
            if (reactionEditors != null && reactionEditors.Length > 0)
            {
                foreach (UnityEditor.Editor reactionEditor in reactionEditors)
                {
                    DestroyImmediate(reactionEditor);
                }
            }
            
            reactionEditors = null;

            // Create new editors
            reactionEditors = new UnityEditor.Editor[worldEvent.Reactions.Length];
            for (int i = 0; i < reactionEditors.Length; i++)
            {
                reactionEditors[i] = UnityEditor.Editor.CreateEditor(worldEvent.Reactions[i]);
                // If not editor for this specific editor exists, it will create a generic inspector not macthing with the ReactionEditor
                // is so, create a ReactionEditor
                if (!reactionEditors[i].GetType().IsSubclassOf(typeof(ReactionEditor)))
                    reactionEditors[i] = UnityEditor.Editor.CreateEditor(worldEvent.Reactions[i], typeof(ReactionEditor));
            }
        }
        void CleanEventTriggerSubEditor()
        {
            // Clean editor if exists
            if (eventTriggerEditor != null)
                DestroyImmediate(eventTriggerEditor);

            eventTriggerEditor = null;
        }
        void CleanConditionSubEditors()
        {
            // Clean editor if exists
            if (conditionEditors != null && conditionEditors.Length > 0)
            {
                foreach (UnityEditor.Editor conditionEditor in conditionEditors)
                {
                    DestroyImmediate(conditionEditor);
                }
            }

            conditionEditors = null;
        }
        void CleanReactionSubEditors()
        {
            // Clean editor if exists
            if (reactionEditors != null && reactionEditors.Length > 0)
            {
                foreach (UnityEditor.Editor reactionEditor in reactionEditors)
                {
                    DestroyImmediate(reactionEditor);
                }
            }

            reactionEditors = null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;

            // Create label style
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold
            };

            EditorGUI.indentLevel++;

            GUILayout.Label("Event info: ", labelStyle);
            EditorGUILayout.PropertyField(eventInfoProperty, true);

            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            if (worldEvent.EventTrigger == null)
            {
                GUILayout.Label("Select trigger: ", labelStyle);

                EditorGUILayout.BeginHorizontal(GUI.skin.box);

                eventTriggerIndexSelected = EditorGUILayout.Popup(eventTriggerIndexSelected, allEventTriggerNames);

                if (EditorUtiles.AddElementGUI(ref worldEvent.EventTrigger, allEventTriggerTypes, eventTriggerIndexSelected, allEventTriggerNames[eventTriggerIndexSelected].text))
                    CheckAndCreateEventTriggerEditor();

                EditorGUILayout.EndHorizontal();
            }

            if (worldEvent.EventTrigger != null)
            {
                EditorGUILayout.BeginHorizontal(GUI.skin.box);

                // Show event trigger custom inspector
                if (eventTriggerEditor != null)
                {
                    EditorGUILayout.BeginVertical();

                    eventTriggerEditor.OnInspectorGUI();

                    EditorGUILayout.EndVertical();

                    EventTriggerEditor etEditor = eventTriggerEditor as EventTriggerEditor;

                    if (!etEditor.ShowEditorState())
                    {
                        if (EditorUtiles.RemoveElementGUI(worldEvent.EventTrigger))
                        {
                            CleanEventTriggerSubEditor();
                            return;
                        }
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            if (worldEvent.Conditions != null)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);

                GUILayout.Label("Select condition: ", labelStyle);

                EditorGUILayout.BeginHorizontal();

                conditionIndexSelected = EditorGUILayout.Popup(conditionIndexSelected, allConditionsNames);

                if (EditorUtiles.AddElementFromArrayGUI(ref worldEvent.Conditions, allConditionsTypes, conditionIndexSelected, allConditionsNames[conditionIndexSelected].text))
                    CheckAndCreateConditionEditors();

                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < conditionEditors.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal(GUI.skin.box);

                    // Show condition custom inspector
                    if (conditionEditors[i] != null)
                    {
                        EditorGUILayout.BeginVertical();

                        conditionEditors[i].OnInspectorGUI();

                        EditorGUILayout.EndVertical();

                        ConditionEditor cEditor = conditionEditors[i] as ConditionEditor;

                        if (!cEditor.ShowEditorState())
                        {
                            if (EditorUtiles.RemoveElementFromArrayGUI(ref worldEvent.Conditions, worldEvent.Conditions[i]))
                            {
                                CheckAndCreateConditionEditors();
                                return;
                            }
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();

            if (worldEvent.Reactions != null)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);

                GUILayout.Label("Select reaction: ", labelStyle);

                EditorGUILayout.BeginHorizontal();

                reactionIndexSelected = EditorGUILayout.Popup(reactionIndexSelected, allReactionsNames);

                 if (EditorUtiles.AddElementFromArrayGUI(ref worldEvent.Reactions, allReactionsTypes, reactionIndexSelected, allReactionsNames[reactionIndexSelected].text))
                    CheckAndCreateReactionEditors();

                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < reactionEditors.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal(GUI.skin.box);

                    // Show reaction custom inspector
                    if (reactionEditors[i] != null)
                    {
                        EditorGUILayout.BeginVertical();

                        reactionEditors[i].OnInspectorGUI();

                        EditorGUILayout.EndVertical();

                        ReactionEditor rEditor = reactionEditors[i] as ReactionEditor;

                        if (!rEditor.ShowEditorState())
                        {
                            EditorGUI.indentLevel++;

                            if (EditorUtiles.RemoveElementFromArrayGUI(ref worldEvent.Reactions, worldEvent.Reactions[i]))
                            {
                                CheckAndCreateReactionEditors();
                                return;
                            }

                            if (EditorUtiles.MoveUpElementGUI(ref worldEvent.Reactions, worldEvent.Reactions[i]))
                            {
                                CheckAndCreateReactionEditors();
                                return;
                            }

                            if (EditorUtiles.MoveDownElementGUI(ref worldEvent.Reactions, worldEvent.Reactions[i]))
                            {
                                CheckAndCreateReactionEditors();
                                return;
                            }

                            EditorGUI.indentLevel--;
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
