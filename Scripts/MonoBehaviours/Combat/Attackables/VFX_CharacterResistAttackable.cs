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
    [RequireComponent(typeof(CharacterStats))]
    public class VFX_CharacterResistAttackable : VFX_ResistanceAttackable<CharacterStats, CharacterStatsSO>
    {

    }
}