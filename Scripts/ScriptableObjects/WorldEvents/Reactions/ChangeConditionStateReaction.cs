using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.ScriptableObjects.WorldEvents.Conditions;

namespace Game.ScriptableObjects.WorldEvents.Reactions
{
    public class ChangeConditionStateReaction : Reaction
    {
        /// <summary>
        /// Condition to change
        /// </summary>
        public Condition Condition;
        public Condition NewConditionState;

        public override IEnumerator React()
        {
            if (Condition == null)
                yield break;

            Condition.ConditionToChangeWith = NewConditionState;
            Condition.Change();
            yield return null;
        }
    }
}
