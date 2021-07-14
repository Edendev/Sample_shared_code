using System;
using Game.Generic.Utiles;

namespace Game.Generic.Stats
{
    public class Stat
    {
        // Maximum and minimum values
        public float MaxValue { get; protected set; }
        public float MinValue { get; protected set; }

        // Current value
        public float CurrentValue { get; protected set; }

        // Events
        public Events.StatChangeEvent OnStatChanged = new Events.StatChangeEvent();

        public void Deal(float value)
        {
            float oldValue = CurrentValue;

            CurrentValue = (CurrentValue - value) < MinValue ? MinValue : (CurrentValue - value);

            if (CurrentValue != oldValue)
                OnStatChanged?.Invoke(CurrentValue, MaxValue);
        }

        public void Add(float value)
        {
            float oldValue = CurrentValue;

            CurrentValue = (CurrentValue + value) > MaxValue ? MaxValue : (CurrentValue + value);

            if (CurrentValue != oldValue)
                OnStatChanged?.Invoke(CurrentValue, MaxValue);
        }

        #region Checkers

        public bool HasDroppedToZero() => (CurrentValue == MinValue) ? true : false;

        #endregion
    }
}