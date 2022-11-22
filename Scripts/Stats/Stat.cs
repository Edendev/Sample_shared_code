using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.ScriptableObjects.Stats;
using Game.Interfaces;

namespace Game.Generic.Stats
{
    [System.Serializable]
    public struct StatStruct
    {
        public StatType StatType;
        public StatSO StatSO;
    }
    
    [System.Serializable]
    public struct StatBuff
    {
        public StatType StatType;
        public ModifierOperationType OperationType;
        public int Magnitude;
        public BuffType BuffType;
        public float BuffDuration;
    }

    public enum BuffType
    {
        Permanent, Temporary
    }

    public enum StatType
    {
        Health, Armor, Penetration, PhysicalDamage, MovementSpeed
    }

    public abstract class Stat : IResetteable
    {
        protected StatSO statSO;
        protected int m_value;
        public List<StatModifier> statModifiers = new List<StatModifier>();
        public event Action<int> OnValueChanged;
        public event Action OnModifierAdded;
        public event Action OnModifierRemoved;
                     
        public Stat(StatSO _statSO)
        {
            statSO = _statSO;
        }

        public void AddModifier(StatModifier newModifier)
        {
            statModifiers.Add(newModifier);
            CalculateValue();
            OnModifierAdded?.Invoke();
        }

        public void RemoveModifierFromSource(UnityEngine.Object source)
        {
            statModifiers = statModifiers.Where(x => x.Source.GetInstanceID() != source.GetInstanceID()).ToList();
            CalculateValue();
            OnModifierRemoved?.Invoke();
        }

        protected virtual void CalculateValue()
        {
            int finalValue = BaseValue();

            if (statModifiers.Count > 0)
            {
                // If there are override modifiers we only compute the one with highest magnitude
                List<StatModifier> overrideModifiers = statModifiers.FindAll(x => x.OperationType == ModifierOperationType.Override);
                if (overrideModifiers != null && overrideModifiers.Count > 0)
                {
                    overrideModifiers.OrderBy(x => x.Magnitude);
                    finalValue = overrideModifiers[0].Magnitude;
                }
                else
                {
                    // If there are no override modifiers. Additives modifiers are computed first
                    statModifiers.Sort((x, y) => x.OperationType.CompareTo(y.OperationType));
                    foreach (StatModifier modifier in statModifiers)
                    {
                        if (modifier.OperationType == ModifierOperationType.Additive)
                        {
                            finalValue += modifier.Magnitude;
                        }
                        else if (modifier.OperationType == ModifierOperationType.Multiplier)
                        {
                            finalValue *= modifier.Magnitude;
                        }
                        else if (modifier.OperationType == ModifierOperationType.Divison)
                        {
                            finalValue /= modifier.Magnitude;
                        }
                    }
                }
            }

            if (statSO.Capacity > 0)
            {
                finalValue = Mathf.Clamp(finalValue, 0, statSO.Capacity);
            }

            if (m_value != finalValue)
            {
                m_value = finalValue;
                OnValueChanged?.Invoke(m_value);
            }           
        }

        public virtual void Reset()
        {
            statModifiers.Clear();
            CalculateValue();
        }

        #region Properties

        public int Value { get { return m_value; } }
        public virtual int BaseValue() => statSO.BaseValue;
        public int Capacity() => statSO.Capacity;

        #endregion
    }

}
