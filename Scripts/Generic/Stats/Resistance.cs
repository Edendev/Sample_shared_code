
namespace Game.Generic.Stats
{
    public class Resistance : Stat
    {
        private const float MAX_RESISTANCE = 1000;
        private const float MIN_RESISTANCE = 0;

        // Constructor
        public Resistance()
        {
            MaxValue = 100; // Default value
            MinValue = MIN_RESISTANCE;
            CurrentValue = MaxValue;
        }

        public void SetMaxValue(float value)
        {
            float oldValue = MaxValue;

            MaxValue = (value > MAX_RESISTANCE) ? MAX_RESISTANCE : (value <= MinValue) ? MinValue + 1 : value;

            Add(MaxValue);

            if (MaxValue != oldValue)
                OnStatChanged?.Invoke(CurrentValue, MaxValue);
        }
    }
}
