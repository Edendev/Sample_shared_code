using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.MonoBehaviours.UI.Clickables
{
    [RequireComponent(typeof(NavMeshObstacle))]
    public class NavMeshObstacleClickable : Clickable
    {
        // Required components
        NavMeshObstacle navMeshObstacle;

        [Header("NavMeshObstacle overrides")]
        [SerializeField] bool UseExternal_VFX_radius_Input = false;

        private void Awake()
        {
            navMeshObstacle = GetComponent<NavMeshObstacle>();
        }

        public override float MouseOver_VFX_radius()
        {
            if (UseExternal_VFX_radius_Input)
                return base.MouseOver_VFX_radius();

            return navMeshObstacle.radius;
        }
    }
}
