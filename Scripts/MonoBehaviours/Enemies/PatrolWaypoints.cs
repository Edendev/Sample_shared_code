using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Game.ScriptableObjects.Enemies;

namespace Game.MonoBehaviours.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class PatrolWaypoints : MonoBehaviour
    {
        // Required components
        NavMeshAgent navMeshAgent;

        [System.Serializable]
        public class PatrolWaypoint
        {
            public Vector3 position;
            public PatrolAction[] actions = new PatrolAction[0];
        }

        /// <summary>
        /// Patrolling waypoints system
        /// </summary>
        public PatrolWaypoint[] Waypoints = new PatrolWaypoint[0];

        // Used for internal calculations
        NavMeshPath path;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            path = new NavMeshPath();
        }

        public IEnumerator MoveToWaypoint(int waypointIndex)
        {
            if (Waypoints == null || Waypoints.Length == 0)
                yield break;

            if (waypointIndex < 0 || waypointIndex >= Waypoints.Length)
                yield break;

            navMeshAgent.isStopped = false;

            navMeshAgent.SetDestination(Waypoints[waypointIndex].position);

            while ((transform.position - Waypoints[waypointIndex].position).magnitude > 0.1f)
            {
                if (navMeshAgent.path.status != NavMeshPathStatus.PathComplete)
                    yield break;

                yield return null;
            }

            navMeshAgent.isStopped = true;
        }
        public IEnumerator StartWaypointActions(int waypointIndex)
        {
            if (Waypoints == null || Waypoints.Length == 0)
                yield break;

            if (waypointIndex < 0 || waypointIndex >= Waypoints.Length)
                yield break;

            if (Waypoints[waypointIndex].actions == null || Waypoints[waypointIndex].actions.Length == 0)
                yield break;

            foreach (PatrolAction patrolAction in Waypoints[waypointIndex].actions)
            {
                yield return StartCoroutine(patrolAction.Execute(gameObject));
            }
        }

        #region Utiles

        /// <summary>
        /// Returns the closes waypoint to the attached gameObject transform position. If not waypoints then returns -1.
        /// </summary>
        /// <returns></returns>
        public int FindClosestAccessibleWaypointIndex()
        {
            int index = -1;
            float minDistance = 1000.0f;
            for (int i = 0; i < Waypoints.Length; i++)
            {
                float distance = (Waypoints[i].position - transform.position).magnitude;
                if (distance < minDistance)
                {
                    NavMesh.CalculatePath(transform.position, Waypoints[i].position, NavMesh.AllAreas, path);
                    if (path.status == NavMeshPathStatus.PathComplete)
                    {
                        minDistance = distance;
                        index = i;
                    }
                }
            }

            return index;
        }

        #endregion

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            // Show patrol waypoints
            if (Waypoints != null && Waypoints.Length > 0)
            {
                for (int i = 0; i < Waypoints.Length; i++)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(Waypoints[i].position, 0.05f);
                    if (i < Waypoints.Length - 1)
                        Gizmos.DrawLine(Waypoints[i].position, Waypoints[i + 1].position);
                }
            }
        }
#endif
    }
}
