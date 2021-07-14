using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Game.MonoBehaviours.UI;
using Game.Interfaces;
using Game.Generic.Combat;
using Game.MonoBehaviours.Stats;
using Game.ScriptableObjects.Stats;

namespace Game.MonoBehaviours.Combat
{
    public abstract class VFX_ResistanceAttackable<T1,T2> : VFX_ProgressBar, IAttackable
        where T1 : EntityStats<T2>
        where T2 : EntityStatsSO
    {
        T1 entityStats;

        private void Awake()
        {
            entityStats = GetComponent<T1>();
        }

        protected override void Start()
        {
            base.Start();

            // Pass delegate to statBar to update when resistance changes
            ProgressBar.Initialize(entityStats.OnResistanceChanged());
        }

        void OnDisable()
        {
            if (ProgressBar != null)
                ProgressBar.gameObject.SetActive(false);
        }

        public virtual void OnAttacked(GameObject attacker, Attack attack)
        {
            // Check if resistance is already zero
            if (entityStats.OnResistanceHasDroppedToZero())
                return;

            if (actualShowingTime > 0f)
            {
                actualShowingTime = ShowingTime;
            }
            else
            {
                StopAllCoroutines(); // To be sure the coroutine is not running 
                actualShowingTime = ShowingTime;
                StartCoroutine(ShowProgressBar());
            }
        }
    }
}

