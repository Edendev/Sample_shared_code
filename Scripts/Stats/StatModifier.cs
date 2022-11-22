using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Generic.Stats
{
    public enum ModifierOperationType
    {
        Additive,
        Multiplier,
        Divison,
        Override
    }

    public class StatModifier
    {
        public StatModifier() { }
        public StatModifier(Object _source, int _magnitude, ModifierOperationType _operationType)
        {
            Source = _source;
            Magnitude = _magnitude;
            OperationType = _operationType;
        }

        public Object Source { get; set; }
        public int Magnitude { get; set; }
        public ModifierOperationType OperationType { get; set; }
    }
}

