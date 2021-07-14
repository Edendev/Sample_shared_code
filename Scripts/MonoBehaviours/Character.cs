using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Game.Interfaces.UI;
using Game.MonoBehaviours.Managers;
using Game.Generic.PathFinding;
using Game.Generic.Utiles;
using Game.ScriptableObjects.Combat;
using Game.MonoBehaviours.Stats;
using Game.MonoBehaviours.Combat;
using Game.Generic.Entities;
using Game.MonoBehaviours.Enemies;

namespace Game.MonoBehaviours
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    public abstract class Character : MonoBehaviour
    {
        /// <summary>
        /// Nave mesh agent is used to control the character motion 
        /// </summary>
        protected NavMeshAgent navMeshAgent;
        protected Animator animator;

        [Header("Set in Inspector")]
        [SerializeField] protected float walkingSpeed = 2f;
        [SerializeField] protected AnimationCurve walkingCurve;

        /// <summary>
        ///  Path to check is the last path generated in the navmeshagent from PathFinding
        ///  It is used as an "out" reference to PathFinding and to compare with the INodes path on the 
        ///  determination of the lowest weigh path to the player
        /// </summary>
        protected NavMeshPath pathToCheck;

        /// <summary>
        /// Used for player animation. The destination is given as reached when the player is at this distance of the destination.
        /// </summary>
        [SerializeField] protected float destinationDistance = 0.2f;

        // Visible only for DEBUGGING
        [SerializeField] protected GameObject currentTarget = null;
        protected Vector3 currentDestination;

        // Starting default position and facing direction
        protected Vector3 startingPosition;
        protected Quaternion startingRotation;

        protected Coroutine optimizingWalking = null;

        // Remote control check
        bool remoteControlState = false;

        // The current assigned speed to the nav mesh agent
        protected float currentSpeed;

        #region Events

        private Events.VoidEvent onDestinationReached = new Events.VoidEvent();
        private Events.VoidEvent onRemoteControlActionFinished = new Events.VoidEvent();
        private Events.VoidEvent onRemoteControlDisabled = new Events.VoidEvent();
        private Events.VoidEvent onRemoteControlEnabled = new Events.VoidEvent();

        #endregion

        #region Accessors

        public Events.VoidEvent OnDestinationReached() => onDestinationReached;
        public Events.VoidEvent OnRemoteControlActionFinished() => onRemoteControlActionFinished;
        public Events.VoidEvent OnRemoteControlDisabled() => onRemoteControlDisabled;
        public Events.VoidEvent OnRemoteControlEnabled() => onRemoteControlEnabled;

        #endregion

        protected virtual void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            currentSpeed = walkingSpeed;
            navMeshAgent.speed = walkingSpeed;

            animator = GetComponent<Animator>();
            animator.SetFloat("WalkingSpeed", walkingSpeed);

            startingPosition = transform.position;
            startingRotation = transform.rotation;
        }

        protected virtual IEnumerator OptimizeWalking()
        {
            // Wait until animator enters in the walking state
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
            {
                yield return null;
            }

            while (animator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
            {
                // Update speed according to walking curve matching the current position in the animator
                int loopTimes = (int)Mathf.Floor(animator.GetCurrentAnimatorStateInfo(0).normalizedTime / animator.GetCurrentAnimatorStateInfo(0).length);
                float progress = (animator.GetCurrentAnimatorStateInfo(0).normalizedTime - loopTimes * animator.GetCurrentAnimatorStateInfo(0).length) / animator.GetCurrentAnimatorStateInfo(0).length;
                navMeshAgent.speed = currentSpeed * walkingCurve.Evaluate(progress);

                // Update facing to prevent the nav mesh rotating wierdly
                Vector3 steeringTargetYNormalized = navMeshAgent.steeringTarget;
                steeringTargetYNormalized.y = transform.position.y;
                transform.LookAt(steeringTargetYNormalized);

                yield return null;                
            }

            yield return null;
        }
        protected virtual IEnumerator MoveToDestination(Vector3 destination)
        {
            navMeshAgent.SetDestination(destination);
            navMeshAgent.isStopped = false;

            while ((transform.position - navMeshAgent.destination).magnitude > destinationDistance)
            {
                if (!navMeshAgent.hasPath)
                {
                    destination = Vector3.MoveTowards(destination, transform.position, 0.1f);
                    RaycastHit hit;
                    if (Physics.Raycast(destination, Vector3.down.normalized, out hit, 10f, 1 << LayerMask.NameToLayer(LayerMasks.TERRAIN_LAYER_MASK)))
                        navMeshAgent.SetDestination(hit.point);
                }
                yield return null;
            }

            navMeshAgent.isStopped = true;
            onDestinationReached?.Invoke();
        }
        protected virtual IEnumerator RotateTo(Quaternion rotation, float time)
        {
            int count = 0;
            float steps = time / 0.05f;
            Quaternion iniRotation = transform.rotation;
            while (count <= steps)
            {
                transform.rotation = Quaternion.Lerp(iniRotation, rotation, count / steps);
                count++;
                yield return new WaitForSeconds(time / steps);
            }

            yield return null;
        }

        protected virtual IEnumerator MoveToStaticTarget()
        {
            Vector3 reachableDestination = PathFinding.GetReachableDestinationToTarget(gameObject, currentTarget, out pathToCheck);
            GameObject savedCurrentTarget = currentTarget;

            // Check if there is no reachable destination
            if (reachableDestination != transform.position)
            {
                while (true)
                {
                    if (currentTarget == null || currentTarget.activeSelf == false)
                        yield return null;

                    if (currentTarget != savedCurrentTarget)
                        reachableDestination = PathFinding.GetReachableDestinationToTarget(gameObject, currentTarget, out pathToCheck);

                    navMeshAgent.destination = reachableDestination;

                    // Smooth approach if destination for attacking not reached by reachable destination position
                    // This is to prevent further computation when objects are rotated and therefore the GetRechableDesintationTarget fails
                    if (!navMeshAgent.hasPath)
                        reachableDestination = Vector3.MoveTowards(reachableDestination, currentTarget.transform.position, 0.02f);

#if DEBUG_ENEMY_NAVMESHAGENT
                if (showNavMeshAgentPath)
                {
                    PathFinding.ShowPathInSceneEditor(navMeshAgent.path, Color.blue, 1f);
                }
#endif
                    yield return null;
                }
            }
        }
        protected virtual IEnumerator MoveToDynamicTarget()
        {
            while (true)
            {
                if (currentTarget == null || currentTarget.activeSelf == false)
                    yield return null;

                navMeshAgent.destination = currentTarget.transform.position;

#if DEBUG_ENEMY_NAVMESHAGENT
                if (showNavMeshAgentPath)
                {
                    PathFinding.ShowPathInSceneEditor(navMeshAgent.path, Color.blue, 1f);
                }
#endif
                yield return null;
            }
        }

        protected void StopOptimizingWalkingCoroutine()
        {
            if (optimizingWalking != null)
                StopCoroutine(optimizingWalking);
        }



        #region Remote_Control

        /// <summary>
        /// Returns true if starts enabling remote control
        /// </summary>
        /// <returns></returns>
        public bool EnableRemoteControlExternally()
        {
            if (remoteControlState)
                return false;

            StopAllCoroutines();
            StartCoroutine(EnablingRemoteControl());
            return true;
        }
        IEnumerator EnablingRemoteControl()
        {
            yield return StartCoroutine(EnableRemoteControl());
            remoteControlState = true;
            onRemoteControlEnabled?.Invoke();
        }
        /// <summary>
        /// Returns true if starts disabling remote control
        /// </summary>
        /// <returns></returns>
        public bool DisableRemoteControlExternally()
        {
            if (!remoteControlState)
                return false;

            StopAllCoroutines();
            StartCoroutine(DisablingRemoteControl());
            return true;
        }
        IEnumerator DisablingRemoteControl()
        {
            yield return StartCoroutine(DisableRemoteControl());
            remoteControlState = false;
            onRemoteControlDisabled?.Invoke();
        }

        protected abstract IEnumerator EnableRemoteControl();
        protected abstract IEnumerator DisableRemoteControl();

        public bool StartRemoteControlAction(RemoteControlAction rcAction)
        {
            if (!remoteControlState)
                return false;

            StopAllCoroutines();
            StartCoroutine(StartingRCAction(rcAction));
            return true;
        }
        IEnumerator StartingRCAction(RemoteControlAction rcAction)
        {
            if (remoteControlState)
            {
                // Perform action
                if (rcAction.GetType() == typeof(MoveToDestination_RCA))
                {
                    MoveToDestination_RCA action = rcAction as MoveToDestination_RCA;
                    transform.LookAt(action.Destination);
                    yield return StartCoroutine(MoveToDestination(action.Destination));
                }
                else if (rcAction.GetType() == typeof(UpdateStartingTransformToCurrent_RCA))
                    UpdateStartingTransformToCurrent();
                else if (rcAction.GetType() == typeof(DisablePatrolWaypoints_RCA))
                    ChangePatrolWaypointsEnabledState(false);
                else if (rcAction.GetType() == typeof(EnablePatrolWaypoints_RCA))
                    ChangePatrolWaypointsEnabledState(true);
            }

            onRemoteControlActionFinished?.Invoke();
        }

        #region Actions

        void ChangePatrolWaypointsEnabledState(bool newState)
        {
            PatrolWaypoints patrolWaypoints = GetComponent<PatrolWaypoints>();
            if (patrolWaypoints)
                patrolWaypoints.enabled = newState;
        }
        void UpdateStartingTransformToCurrent()
        {
            startingPosition = transform.position;
            startingRotation = transform.rotation;
        }

        #endregion

        #endregion
    }
}
