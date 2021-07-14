using UnityEngine;
using UnityEngine.Events;
using System;
using Game.Generic.Stats;
using Game.ScriptableObjects.Stats;
using Game.Generic.Utiles;

namespace Game.MonoBehaviours.Stats
{
    public abstract class EntityStats<T> : MonoBehaviour
        where T : EntityStatsSO
    {
        /// <summary>
        /// Entity stats data object
        /// </summary>
        [SerializeField] private T entityStatsSO;

        // Common stats for all entities
        Resistance resistance;

        // Events
        Events.DoubleFloatEvent onResistanceChanged = new Events.DoubleFloatEvent();
        Events.VoidEvent onResistanceDropToMinimum = new Events.VoidEvent();

        #region Accessors

        public float CurrentResistance() => resistance.CurrentValue;
        public float MaxResistance() => resistance.MaxValue;
        public Events.DoubleFloatEvent OnResistanceChanged() => onResistanceChanged;
        public Events.VoidEvent OnResistanceDropToMinimum() => onResistanceDropToMinimum;

        #endregion

        #region Monobehaviour

        protected virtual void Awake()
        {
            // Create entity stats
            resistance = new Resistance();

            // Subscribe to events
            resistance.OnStatChanged.AddListener(OnResistanceHasChanged);

            // Set max resistance value
            resistance.SetMaxValue(entityStatsSO.Resistance());
        }

        private void OnDestroy()
        {
            // Unsubscribe to events
            resistance.OnStatChanged.RemoveListener(OnResistanceHasChanged);
        }

        #endregion

        public void IncreaseResistance(float value) => resistance.Add(value); 

        public void DealResistance(float value)
        {
            resistance.Deal(value);

            // Check if drop to zero and send message
            if (resistance.HasDroppedToZero())
                onResistanceDropToMinimum?.Invoke();
        }

        #region Checkers

        public bool OnResistanceHasDroppedToZero() => resistance.HasDroppedToZero();

        #endregion

        #region Messages

        public void OnResistanceHasChanged(float currentValue, float maxValue) => onResistanceChanged?.Invoke(currentValue, maxValue);

        #endregion
    }
}
