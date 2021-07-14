using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.ScriptableObjects.Containers;

namespace Game.Editor.ScriptableObjects.Containers
{
    [RequireComponent(typeof(ConditionsContainer))]
    public class ConditionsContainerEditor : UnityEditor.Editor
    {
        ConditionsContainer conditionsContainer;

        private void OnEnable()
        {
            conditionsContainer = (ConditionsContainer)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Reset conditions"))
            {
                conditionsContainer.ResetAllConditions();
            }
        }
    }
}

