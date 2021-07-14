using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.MonoBehaviours.UI.Clickables
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshAgentClickable : Clickable
    {
        // Required components
        NavMeshAgent navMeshAgent;

        [Header("NavMeshAgent overrides")]
        [SerializeField] bool UseExternal_VFX_radius_Input = false;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>(); 
        }

        public override float MouseOver_VFX_radius()
        {
            if (UseExternal_VFX_radius_Input)
                return base.MouseOver_VFX_radius();

            return navMeshAgent.radius;
        }
    }
}
