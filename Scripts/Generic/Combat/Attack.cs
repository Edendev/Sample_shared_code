using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Generic.Combat
{
    public class Attack
    {
        public float Damage { get; private set; }

        public Attack(float damage)
        {
            Damage = damage;
        }
    }
}
