using UnityEngine;
using Game.Interfaces;
using Game.Generic.Combat;
using Game.MonoBehaviours.Stats;
using Game.ScriptableObjects.Stats;

namespace Game.MonoBehaviours.Combat
{
    public abstract class DealResistanceAttackable<T1,T2> : MonoBehaviour, IAttackable
        where T1 : EntityStats<T2>
        where T2 : EntityStatsSO
    {
        private T1 entityStats;

        private void Awake()
        {
            entityStats = GetComponent<T1>();
        }

        public virtual void OnAttacked(GameObject attacker, Attack attack)
        {
            // Check if resistance is already zero
            if (entityStats.OnResistanceHasDroppedToZero())
                return;
            
            // Deal resistance
            entityStats.DealResistance(attack.Damage);

            if (entityStats.OnResistanceHasDroppedToZero())
            {
                // Destroy
                IDestroyable[] destroyables = gameObject.GetComponentsInChildren<IDestroyable>();
                foreach (IDestroyable destroyable in destroyables)
                {
                    destroyable.OnDestroyed(attacker);
                }
            }
        }
    }
}

