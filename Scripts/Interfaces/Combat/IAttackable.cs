using UnityEngine;
using Game.Generic.Combat;

namespace Game.Interfaces
{
    public interface IAttackable
    {
        void OnAttacked(GameObject attacker, Attack attack);
    }
}
