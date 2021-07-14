using UnityEngine;
using System.Collections;
using Game.Interfaces;
using Game.Generic.Combat;
using Game.MonoBehaviours.Stats;
using Game.ScriptableObjects.Stats;

namespace Game.MonoBehaviours.Combat
{
    public class FaceAttackerAttackable : MonoBehaviour, IAttackable
    {
        public virtual void OnAttacked(GameObject attacker, Attack attack)
        {
            transform.LookAt(attacker.transform);
        }
    }
}

