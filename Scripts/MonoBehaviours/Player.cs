//#define DEBUG_PLAYER_NAVMESHAGENT

using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Game.MonoBehaviours.Managers;
using Game.Interfaces.UI;
using Game.Generic.PathFinding;
using Game.Generic.Data;
using Game.MonoBehaviours.Inventory;
using Game.Generic.Utiles;
using Game.MonoBehaviours.Stats;
using Game.MonoBehaviours.Interactable;
using Game.Interfaces;
using Game.ScriptableObjects.Combat;

namespace Game.MonoBehaviours
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(PlayerInventory))]
    public class Player : Character
    {
        /// <summary>
        /// Static reference to the player instance
        /// Accessible
        /// </summary>
        public static Player S { get; protected set; }

        // Necessary transforms
        [SerializeField] Transform playerMesh;
        [SerializeField] Transform playerArmature;

        // Reference to required components
        PlayerStats stats;

        [Header("Set in Inspector")]
        [SerializeField] float attackDistanceToTarget = 0.5f;
        [SerializeField] float attackDelayTime = 0.5f;
        [SerializeField] float interactDistance = 0.5f;

        /// <summary>
        /// State machine
        /// </summary>
        public enum State
        {
            Uninitialized, None, RemoteControl, Idl, MovingToDestination, MovingToTarget, InteractingWithTarget, AttackingTarget, Dead
        }

        [SerializeField] State currentState = State.Uninitialized; // ONLY visible under DEBUGGING

        #region Messages
        /// <summary>
        /// Sends a message when state changes containing the new state
        /// </summary>
        [SerializeField] Events.PlayerStateEvent onStateChanges = new Events.PlayerStateEvent();
        [SerializeField] Events.VoidEvent onPlayerDead = new Events.VoidEvent();

        #endregion

        // Debugging
#if DEBUG_PLAYER_NAVMESHAGENT
        public bool ShowNavMeshNavigation = false;
#endif

        protected override void Awake()
        {           
            // Make instance a singleton
            if (S != null)
            {
                Destroy(gameObject);
            }
            else
            {
                S = this;
            }

            base.Awake();

            // Make sure the rigidbody is kinematic
            Rigidbody rigidBody = GetComponent<Rigidbody>();
            rigidBody.isKinematic = true;

            pathToCheck = new NavMeshPath();

            stats = GetComponent<PlayerStats>();

            // Initialize state to Idl
            StartCoroutine(ChangeState(State.Idl));
        }

        protected void Start()
        {
            // Subscibe to MouseManager events
            MouseManager.GetMouseClickEnvironmentEvent().AddListener(SetDestination);
            MouseManager.GetMouseClickAttackableEvent().AddListener(AttackTarget);
            MouseManager.GetMouseClickInteractableEvent().AddListener(InteractWithTarget);

            WorldEventsManager.OnEventExecuted().AddListener(OnWorldEventExecuted);
            WorldEventsManager.OnEventFinished().AddListener(OnWorldEventFinished);

            // Subscribe to stats events
            stats.OnResistanceDropToMinimum().AddListener(OnResistanceDropsToZero);
        }

        private void OnDestroy()
        {
            // Unsubscribe to MouseManager events
            MouseManager.GetMouseClickEnvironmentEvent().RemoveListener(SetDestination);
            MouseManager.GetMouseClickAttackableEvent().RemoveListener(AttackTarget);
            MouseManager.GetMouseClickInteractableEvent().RemoveListener(InteractWithTarget);

            WorldEventsManager.OnEventExecuted().RemoveListener(OnWorldEventExecuted);
            WorldEventsManager.OnEventFinished().RemoveListener(OnWorldEventFinished);
        }

        #region Commands

        void SetDestination(Vector3 destination)
        {
            if (currentState == State.InteractingWithTarget)
                return;

            // Reset velocity if destination is to opposite direction
            if (Vector3.Angle(destination - transform.position, navMeshAgent.destination - transform.position) > 90)
                navMeshAgent.velocity = Vector3.zero;

            navMeshAgent.SetDestination(destination);
            Vector3 destinationNormY = destination;
            destinationNormY.y = transform.position.y;
            transform.LookAt(destinationNormY, Vector3.up);

            if (currentState != State.MovingToDestination)
            {
                StopAllCoroutines();
                navMeshAgent.isStopped = false;
                currentTarget = null;
                StartCoroutine(MoveToDestination(destination));
            }
        }
        void AttackTarget(GameObject target)
        {
            if (currentState == State.InteractingWithTarget)
                return;

            if (stats.GetEquippedWeapon() == null)
                return;

            StopAllCoroutines();
            navMeshAgent.isStopped = false;
            currentTarget = target;
            navMeshAgent.SetDestination(target.transform.position);
            transform.LookAt(target.transform);
            StartCoroutine(MoveToAndAttackTarget());
        }
        void InteractWithTarget(GameObject target)
        {
            if (currentState == State.InteractingWithTarget)
                return;

            StopAllCoroutines();
            navMeshAgent.isStopped = false;
            currentTarget = target;
            navMeshAgent.SetDestination(target.transform.position);
            transform.LookAt(target.transform);
            StartCoroutine(MoveToAndInteractWithTarget());
        }

        #endregion

        protected override IEnumerator MoveToDestination(Vector3 destination)
        {
            yield return StartCoroutine(ChangeState(State.MovingToDestination));

            yield return StartCoroutine(base.MoveToDestination(destination));

            yield return StartCoroutine(ChangeState(State.Idl));
        }

        IEnumerator MoveToAndInteractWithTarget()
        {
            // Move to target
            yield return StartCoroutine(MovingToTarget());

            // Stop and interact
            navMeshAgent.isStopped = true;
            transform.LookAt(currentTarget.transform);

            // Get interactables and Interact
            IInteractable[] interactables = currentTarget.GetComponentsInChildren<IInteractable>();

            yield return StartCoroutine(Interact(interactables));
        }
        IEnumerator MoveToAndAttackTarget()
        {
            // Move to target
            yield return StartCoroutine(MovingToTarget());

            // Stop and attack target
            navMeshAgent.isStopped = true;
            transform.LookAt(currentTarget.transform);

            // Attack target
            yield return StartCoroutine(AttackingTarget());

            // If lost contact to target, check if target still exists to continue the pursue
            if (currentTarget != null && currentTarget.activeSelf != false)
            {
                AttackTarget(currentTarget);
            }
            else
            {
                yield return StartCoroutine(ChangeState(State.Idl));
            }
        }
        IEnumerator MovingToTarget()
        {
            yield return StartCoroutine(ChangeState(State.MovingToTarget));
            
            Vector3 reachableDestination = PathFinding.GetReachableDestinationToTarget(gameObject, currentTarget, out pathToCheck);

            // Check if there is no reachable destination
            if (reachableDestination == transform.position)
            {
                StopAllCoroutines();
                StartCoroutine(ChangeState(State.Idl));
            }

            while ((Vector3.MoveTowards(transform.position, currentTarget.transform.position, navMeshAgent.radius) - reachableDestination).magnitude > interactDistance)
            {
                if (currentTarget == null || currentTarget.activeSelf == false)
                    break;

                navMeshAgent.destination = reachableDestination;

                // Smooth approach if destination for attacking not reached by reachable destination position
                // This is to prevent further computation when objects are rotated and therefore the GetRechableDesintationTarget fails
                if (!navMeshAgent.hasPath)
                    reachableDestination = Vector3.MoveTowards(reachableDestination, currentTarget.transform.position, 0.02f);

#if DEBUG_PLAYER_NAVMESHAGENT
                if (ShowNavMeshNavigation)
                {
                    PathFinding.ShowPathInSceneEditor(navMeshAgent.path, Color.blue, 1f);
                }
#endif
                yield return null;
            }
        }
        IEnumerator AttackingTarget()
        {
            yield return StartCoroutine(ChangeState(State.AttackingTarget));

            while ((Vector3.MoveTowards(transform.position, currentTarget.transform.position, navMeshAgent.radius) - currentTarget.transform.position).magnitude <= attackDistanceToTarget)
            {
                if (currentTarget == null || currentTarget.activeSelf == false)
                    break;

                transform.LookAt(currentTarget.transform);
                yield return StartCoroutine(PerformingAttack());
            }
        }
        IEnumerator PerformingAttack()
        {
            navMeshAgent.isStopped = true;

            if (currentTarget == null || currentTarget.activeSelf == false)
                yield return null;

            if (stats.HasEquippedWeapon())
            {
                // Perform animation and wait until done
                animator.Play("SwordAttack", 0, 0f);
                animator.speed = animator.GetCurrentAnimatorClipInfo(0).Length / stats.GetEquippedWeapon().AttackSpeed();
                yield return new WaitForEndOfFrame(); // To enable the animator entering the attack state
                float animationTime = animator.GetCurrentAnimatorStateInfo(0).length / animator.GetCurrentAnimatorStateInfo(0).speed;
                // Try to use the weapon
                if (stats.TryUseEquippedWeapon(currentTarget, animationTime))
                {
                    yield return new WaitForSeconds(animationTime);
                }

                yield return new WaitForSeconds(stats.GetEquippedWeapon().AttackRate());
            }
        }
        IEnumerator Interact(IInteractable[] interactables)
        {
            yield return StartCoroutine(ChangeState(State.InteractingWithTarget));

            foreach(IInteractable interactable in interactables)
            {
                StartCoroutine(interactable.Interact(gameObject));
            }

            yield return StartCoroutine(ChangeState(State.Idl));

            yield return null;
        }

        void OnResistanceDropsToZero()
        {
            StopAllCoroutines();
            StartCoroutine(PlayerIsDead());
        }
        IEnumerator PlayerIsDead()
        {
            onPlayerDead?.Invoke();
            yield return StartCoroutine(ChangeState(State.Dead));
        }

        #region Remote_Control

        protected override IEnumerator EnableRemoteControl()
        {
            yield return StartCoroutine(ChangeState(State.RemoteControl));
            yield return null;
        }
        protected override IEnumerator DisableRemoteControl()
        {
            yield return StartCoroutine(ChangeState(State.Idl));
            yield return null;
        }
        // TO BE ADDED TO REMOTE CONTROL ACTIONS OF CHARACTER
        public void TeleportToPosition(Vector3 destination)
        {
            if (currentState != State.RemoteControl)
                return;

            navMeshAgent.enabled = false;
            transform.position = destination;
            navMeshAgent.enabled = true;
        }

        #endregion

        #region Handle_WorldEvents

        void OnWorldEventExecuted(IEvent e)
        {
            EventInfo eventInfo = e.GetEventInfo();
            if (eventInfo.StopPlayer)
            {
                StopAllCoroutines();
                StartCoroutine(EnableRemoteControl());
            }
        }
        void OnWorldEventFinished(IEvent e)
        {
            EventInfo eventInfo = e.GetEventInfo();
            if (eventInfo.StopPlayer)
            {
                StopAllCoroutines();
                StartCoroutine(DisableRemoteControl());
            }
        }

        #endregion

        #region StateMachine

        IEnumerator ChangeState(State newState)
        {
            if (newState == currentState)
                yield break;

            // Save old state and set current state as changing 
            State oldState = currentState;
            currentState = State.None;

            // End old state
            yield return EndStateTasks(oldState);

            // Assign new state as current state
            currentState = newState;

            // Send message of state change
            onStateChanges?.Invoke(newState);

            // Start new state
            yield return StartStateTasks(newState);
        }
        IEnumerator EndStateTasks(State oldState)
        {
            switch (oldState)
            {
                case State.Idl:
                    break;
                case State.MovingToDestination:
                    break;
                case State.MovingToTarget:
                    break;
                case State.InteractingWithTarget:
                    break;
                case State.AttackingTarget:
                    break;
            }

            yield return null;
        }
        IEnumerator StartStateTasks(State newState)
        {
            // Default parameters
            animator.speed = 1f;

            switch (newState)
            {
                case State.Idl:
                    animator.CrossFade("Idl", 0.5f);
                    navMeshAgent.isStopped = true;
                    break;
                case State.MovingToDestination:
                    animator.CrossFade("Walking", 0f);
                    navMeshAgent.isStopped = false;
                    StartCoroutine(OptimizeWalking());
                    break;
                case State.MovingToTarget:
                    animator.CrossFade("Walking", 0f);
                    navMeshAgent.isStopped = false;
                    StartCoroutine(OptimizeWalking());
                    break;
                case State.InteractingWithTarget:

                    if (currentTarget.GetComponent<Item>())
                        yield return StartCoroutine(PickUpItemAnimation());

                    if (currentTarget.GetComponent<JumpableWall>())
                        if (stats.TryUseEquippedRope())
                            yield return StartCoroutine(UseEquippedRopeAnimation());      
                    
                    navMeshAgent.isStopped = true;
                    break;
                case State.AttackingTarget:
                    break;
                case State.Dead:
                    navMeshAgent.isStopped = true;
                    animator.Play("Dead", 0, 0f);
                    yield return new WaitForEndOfFrame();
                    yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length / animator.GetCurrentAnimatorStateInfo(0).speed);
                    break;
            }

            yield return null;
        }

        #endregion

        #region Animation

        IEnumerator PickUpItemAnimation()
        {
            animator.Play("PickUp", 0, 0f);
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length / animator.GetCurrentAnimatorStateInfo(0).speed / 2f);
            animator.CrossFade("Idl", 0f);
        }
        IEnumerator UseEquippedRopeAnimation()
        {
            transform.LookAt(currentTarget.transform);
            GameObject ropeWorldObject = stats.GetWorldObjEquippedRope();
            Vector3 savedLocalPosition = ropeWorldObject.transform.localPosition;
            Quaternion savedLocalRotation = ropeWorldObject.transform.localRotation;
            ropeWorldObject.transform.SetParent(null);
            ropeWorldObject.transform.position = transform.position + transform.forward.normalized*0.1f;
            ropeWorldObject.transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            Animator ropeAnimator = ropeWorldObject.GetComponent<Animator>();
            if (ropeAnimator != null)
                ropeAnimator.Play("ToJumpOverWall", 0, 0f);
            animator.Play("JumpOverWall", 0, 0f);
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length / animator.GetCurrentAnimatorStateInfo(0).speed/2f);
            // At half-way we translate the player transform to a save position inside the wall to prevent the enemies to keep attacking
            // To maintain the position of the animation, we displace the local position of the mesh and armature child objects
            navMeshAgent.enabled = false;
            transform.position = transform.position + transform.forward.normalized * 0.25f;
            playerArmature.localPosition = new Vector3(0f, 0f, -(Mathf.Abs(transform.forward.normalized.z) * 0.25f));
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length / animator.GetCurrentAnimatorStateInfo(0).speed / 2f);
            // Translate the player the whole way to the other side and get back the radius to normal
            transform.position = transform.position + transform.forward.normalized * 0.25f;
            // Get the mesh and amaturue back to zero local position
            playerArmature.localPosition = Vector3.zero;
            animator.CrossFade("Idl", 0f);
            if (ropeAnimator != null)
                ropeAnimator.CrossFade("Idl", 0f);
            ropeWorldObject.transform.SetParent(stats.GetHeadTransform());
            ropeWorldObject.transform.localPosition = savedLocalPosition;
            ropeWorldObject.transform.localRotation = savedLocalRotation;
            navMeshAgent.enabled = true;
        }

        #endregion

        #region Accessors

        public GameObject CurrentTarget() => currentTarget;
        public float NavMeshBodyRadius() => navMeshAgent.radius;
        public float NavMeshBodyHeight() => navMeshAgent.height;
        public Events.VoidEvent OnPlayerDead() => onPlayerDead;
        public bool HasEquippedWeapon() => stats.HasEquippedWeapon();
        public bool HasEquippedRope() => stats.HasEquippedRope();
        public Events.DoubleFloatEvent OnResistanceChanged() => stats.OnResistanceChanged();
        public float CurrentResistance() => stats.CurrentResistance();
        public float MaxResistance() => stats.MaxResistance();

        #endregion

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            // Show neighbour connections
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up);
            Gizmos.DrawIcon(transform.position + Vector3.up, "Player_icone.tif", false);
        }
#endif
    }
}
