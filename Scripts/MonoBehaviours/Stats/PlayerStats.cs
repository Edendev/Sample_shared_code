using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using Game.Generic.Stats;
using Game.ScriptableObjects.Stats;
using Game.Generic.Utiles;
using Game.ScriptableObjects.Combat;

namespace Game.MonoBehaviours.Stats
{
    public class PlayerStats : CharacterStats
    {
        [Header("Health")]
        [SerializeField, Tooltip("Health recovery per second")] float recoveryRate = 1f;

        [Header("Equipment")]
        [SerializeField] SwordAttack equippedWeapon;
        [SerializeField] bool hasRopeEquipped;
        [SerializeField] float timeBeforeEquippingWeaponAfterUsing = 5f;

        // Holds the reference to the equipped world objects
        GameObject worldObjEquippedWeapon;
        GameObject worldObjEquippedRope;

        /// <summary>
        /// Parent transform used to equip and unequip accesories
        /// </summary>
        [SerializeField] Transform rightHandTransform;
        [SerializeField] Transform leftHandTransform;
        [SerializeField] Transform headTransform;

        // Private variables
        float recoveryTimer = 0f;
        float usingEquippedWeaponTimer = 0f;
        bool usingEquippedWeapon = false;
        bool delayUsingEquippedWeapon = false;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Update()
        {
            // Recover over time
            if (CurrentResistance() < MaxResistance())
            {
                if ((Time.time - recoveryTimer) >= 1f)
                {
                    IncreaseResistance(recoveryRate);
                    recoveryTimer = Time.time;
                }
            }
        }
        
        /// <summary>
        /// Try to use the equipped weapon
        /// </summary>
        /// <param name="target"> Target </param>
        /// <param name="delay"> Delay associated to the animation before effectively using the weapon</param>
        public bool TryUseEquippedWeapon(GameObject target, float delay)
        {
            if (equippedWeapon)
            {
                if (!usingEquippedWeapon)
		{
                    StartCoroutine(UseEquippedWeapon(target, delay));
               	    return true;
		} 
            }

            return false;
        }
        IEnumerator UseEquippedWeapon(GameObject target, float delay)
        {
            usingEquippedWeapon = true;
            usingEquippedWeaponTimer = equippedWeapon.AttackRate() + timeBeforeEquippingWeaponAfterUsing;

            if (!delayUsingEquippedWeapon)
                StartCoroutine(DelayUntilEquippingWeapon());

            yield return new WaitForSeconds(delay);

            if (Player.S.CurrentTarget() == target)
                equippedWeapon.Attack(gameObject, target);

            usingEquippedWeapon = false;
        }
        IEnumerator DelayUntilEquippingWeapon()
        {
            delayUsingEquippedWeapon = true;
            SetWeaponOnUsing();

            while (usingEquippedWeaponTimer > 0f)
            {
                usingEquippedWeaponTimer -= Time.deltaTime;
                yield return null;
            }

            // Set equipped weapon on equipped mode
            SetWeaponOnEquipped();
            delayUsingEquippedWeapon = false;

            yield return null;
        }
        public bool TryEquipWeapon(WeaponAttack weapon)
        {
            // Code need optimization
            SwordAttack swordAttack = weapon as SwordAttack;
            if (swordAttack)
            {
                if (equippedWeapon != swordAttack)
                {
                    UnequipWeapon();

                    if (weapon.WeaponWorldObject != null)
                    {
                        // Instantiate world object and apply local transform
                        worldObjEquippedWeapon = Instantiate(weapon.WeaponWorldObject, headTransform);
                        equippedWeapon = weapon as SwordAttack;
                        SetWeaponOnEquipped();
                    }

                    return true;
                }
            }

            return false;
        }
        public void UnequipWeapon()
        {
            if (worldObjEquippedWeapon != null)
                Destroy(worldObjEquippedWeapon);

            equippedWeapon = null;
        }
        void SetWeaponOnEquipped()
        {
            worldObjEquippedWeapon.transform.SetParent(headTransform);
            worldObjEquippedWeapon.transform.localPosition = equippedWeapon.EquippedLocalPosition();
            worldObjEquippedWeapon.transform.localRotation = Quaternion.Euler(equippedWeapon.EquippedLocalRotation());
        }
        void SetWeaponOnUsing()
        {
            // Set equipped weapon on using mode
            worldObjEquippedWeapon.transform.SetParent(rightHandTransform);
            worldObjEquippedWeapon.transform.localPosition = equippedWeapon.UsingLocalPosition();
            worldObjEquippedWeapon.transform.localRotation = Quaternion.Euler(equippedWeapon.UsingLocalRotation());
        }

        public bool TryUseEquippedRope() => hasRopeEquipped;
        public bool TryEquipRope(Rope rope) 
        {
            if (hasRopeEquipped)
                UnequipRope();

            if (rope.RopeWorldObject != null)
            {
                worldObjEquippedRope = Instantiate(rope.RopeWorldObject, headTransform);
            }

            hasRopeEquipped = true;

            return true;
        }
        public void UnequipRope()
        {
            if (worldObjEquippedRope != null)
                Destroy(worldObjEquippedRope);

            hasRopeEquipped = false;
        }

        #region Accessors

        public SwordAttack GetEquippedWeapon() => equippedWeapon;
        public GameObject GetWorldObjEquippedRope() => worldObjEquippedRope;
        public Transform GetHeadTransform() => headTransform;
        public Transform GetLeftHandTransform() => leftHandTransform;
        public Transform GetRightHandTransform() => rightHandTransform;
        public bool HasEquippedWeapon() => (equippedWeapon != null);
        public bool HasEquippedRope() => hasRopeEquipped;

        #endregion
    }
}