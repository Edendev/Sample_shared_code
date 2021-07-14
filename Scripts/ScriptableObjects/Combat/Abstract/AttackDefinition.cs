using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Generic.Combat;
using Game.ScriptableObjects.Core;

namespace Game.ScriptableObjects.Combat
{
    public abstract class AttackDefinition : GameSO
    {
        public float Damage = 1f;

        protected Attack CreateAttack(GameObject attacker, GameObject target)
        {
            return new Attack(Damage);
        }
    }
}

