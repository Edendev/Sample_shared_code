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
    [RequireComponent(typeof(DestroyableEntityStats))]
    public class VFX_DestroyableEntityResistAttackable : VFX_ResistanceAttackable<DestroyableEntityStats,DestroyableEntityStatsSO>
    {

    }
}

