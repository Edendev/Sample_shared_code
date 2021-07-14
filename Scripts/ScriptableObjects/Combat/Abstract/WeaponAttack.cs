using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Generic.Combat;
using Game.Interfaces;

namespace Game.ScriptableObjects.Combat
{
    public abstract class WeaponAttack : AttackDefinition
    {
        public GameObject WeaponWorldObject;
        [SerializeField, Tooltip("Local rotation applied to the weapon object when equipped on the character")] Vector3 equippedLocalRotation;
        [SerializeField, Tooltip("Local rotation applied to the weapon object when being used by the character")] Vector3 usingLocalRotation;
        [SerializeField, Tooltip("Local position applied to the weapon object when equipped on the character")] Vector3 equippedLocalPosition;
        [SerializeField, Tooltip("Local position applied to the weapon object when being used by the character")] Vector3 usingLocalPosition;
        [SerializeField, Tooltip("Frequency of attacks in times per second")] float attackRate = 0.5f;
        [SerializeField, Tooltip("Speed of attacks in times per second. This affects the animation speed")] float attackSpeed = 0.5f;

        public void Attack(GameObject attacker, GameObject target)
        {
            // Check that attacker and target are not null
            if (attacker == null || target == null)
                return;

            // Create attack on target
            IAttackable[] attackables = target.GetComponentsInChildren<IAttackable>();
            if (attackables != null)
            {
                foreach (IAttackable attackable in attackables)
                {
                    Attack newAttack = CreateAttack(attacker, target);
                    attackable.OnAttacked(attacker, newAttack);
                }
            }
        }

        #region Accessors

        public float AttackRate() => attackRate;
        public float AttackSpeed() => attackSpeed;
        public Vector3 EquippedLocalRotation() => equippedLocalRotation;
        public Vector3 UsingLocalRotation() => usingLocalRotation;
        public Vector3 EquippedLocalPosition() => equippedLocalPosition;
        public Vector3 UsingLocalPosition() => usingLocalPosition;

        #endregion
    }
}
