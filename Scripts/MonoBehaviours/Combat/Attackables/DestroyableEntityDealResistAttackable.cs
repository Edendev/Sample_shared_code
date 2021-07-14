using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.MonoBehaviours.Stats;
using Game.ScriptableObjects.Stats;

namespace Game.MonoBehaviours.Combat
{
    [RequireComponent(typeof(DestroyableEntityStats))]
    public class DestroyableEntityDealResistAttackable : DealResistanceAttackable<DestroyableEntityStats, DestroyableEntityStatsSO>
    {

    }
}
