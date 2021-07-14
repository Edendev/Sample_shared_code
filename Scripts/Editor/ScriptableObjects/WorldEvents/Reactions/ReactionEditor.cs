using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.Editor.ScriptableObjects.Core;
using Game.ScriptableObjects.WorldEvents.Reactions;

namespace Game.Editor.ScriptableObjects.WorldEvents.Reactions
{
    [CustomEditor(typeof(Reaction))]
    public class ReactionEditor : GameSOEditor
    {
        protected override void ShowSpecificInspector() { }

        protected override string GetFoldoutLabel()
        {
            return target.name;
        }
    }
}