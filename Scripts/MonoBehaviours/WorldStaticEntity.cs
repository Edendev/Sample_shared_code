using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Game.Interfaces;

namespace Game.MonoBehaviours
{
    [RequireComponent(typeof(NavMeshObstacle))]
    public class WorldStaticEntity : MonoBehaviour, IStaticTarget
    {
        // Reference required components
        protected NavMeshObstacle navMeshObstacle { get; private set; }

        /// <summary>
        /// Positions used for pathFinding. NavMeshAgents will look these positions and not the transform.position of this gameObject when moving towards it.
        /// Positions are set relative to transform of the gameObjects.
        /// </summary>
        [SerializeField] Vector3[] accessiblePositions = new Vector3[0];

        protected virtual void Awake()
        {
            navMeshObstacle = GetComponent<NavMeshObstacle>();
        }

        #region IStaticTarget

        /// <summary>
        /// Returns the accesible positions in world space coordinates. 
        /// </summary>
        /// <returns></returns>
        public Vector3[] AccesiblePositions()
        {
            Vector3[] worldPositions = new Vector3[accessiblePositions.Length];

            for (int i = 0; i < accessiblePositions.Length; i++)
            {
                worldPositions[i] = accessiblePositions[i] + transform.position;
            }

            return worldPositions;
        }

        #endregion

#if UNITY_EDITOR

        protected virtual void OnDrawGizmos()
        {
            // Show acccesible positions
            if (accessiblePositions != null && accessiblePositions.Length > 0)
            {
                foreach (Vector3 position in accessiblePositions)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(transform.position + position, 0.05f);
                }
            }
        }

#endif
    }
}
