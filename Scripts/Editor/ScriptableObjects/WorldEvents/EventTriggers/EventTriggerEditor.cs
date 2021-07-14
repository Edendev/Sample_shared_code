using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.Editor.ScriptableObjects.Core;
using Game.ScriptableObjects.WorldEvents.EventTriggers;

namespace Game.Editor.ScriptableObjects.WorldEvents.EventTriggers
{
    [CustomEditor(typeof(EventTrigger))]
    public class EventTriggerEditor : GameSOEditor
    {
        protected override void ShowSpecificInspector() { }

        protected override string GetFoldoutLabel()
        {
            return target.name;
        }
    }
}

