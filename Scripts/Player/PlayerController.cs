using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Game.MonoBehaviours.Managers;
using Game.Generic.StateMachine;
using Game.Generic.StateMachine.PlayerStates;
using Game.ScriptableObjects.Player;
using Game.Generic.ObjectPooling;
using Game.Interfaces;
using Game.MonoBehaviours.Stats;
using Game.Generic.PlayerInput;
using Game.MonoBehaviours.Inventory;
using Game.MonoBehaviours.Animations;
using Game.MonoBehaviours.PlayerActions;
using Game.Generic.PlayerActions;
using Game.MonoBehaviours.Targets;
using Game.MonoBehaviours.VFX;

namespace Game.MonoBehaviours.Player
{
    [RequireComponent(typeof(PlayerVFX))]
    [RequireComponent(typeof(PlayerAnimation))]
    [RequireComponent(typeof(PlayerActionsController))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(StatsController))]
    public class PlayerController : MonoBehaviour, IPooleable<PlayerController>, IDynamicTarget
    {
        #region StateMachine

        StateMachine mainStateMachine;

        PlayerInactiveState pInactiveState;
        PlayerDeadState pDeadState;
        PlayerMovingState pMovingState;
        PlayerIdlState pIdlState;
        PlayerReloadState pReloadState;
        PlayerAimingState pAimingState;
        PlayerAimingAndMovingState pAimingAndMovingState;
        PlayerThrowObjectPrepState pThrowObjectPrepState;
        PlayerThrowingObjectState pThrowingObjectState;
        PlayerInteractWithReparableState pInteractWithReparableState;
        PlayerInteractWithShopState pInteractWithShopState;
        PlayerGoToCoverPointState pGoToCoverPointState;
        PlayerMoveToNextCoverPointState pMoveToNextCoverPointState;
        PlayerCoverAnimationState pCoverAnimationState;
        PlayerCoveringState pCoveringState;
        PlayerAttackState pAttackState;
        PlayerDashState pDashState;

        #endregion

        public StatsController Stats { get; private set; }
        public PlayerInventory Inventory { get; private set; }
        public PlayerAnimation Animations { get; private set; }
        public PlayerActionsController Actions { get; private set; }
        public CharacterController Character { get; private set; }
        public PlayerVFX VFX { get; private set; }

        public ObjectPool<PlayerController> PoolOrigin { get; set; }

        PlayerSO playerSO;

        // Bone used for rotating the upper body of the player while aiming
        [SerializeField] Transform spineTransform;

        #region Blackboard

        IInteractable currentInteractable;
        ITarget currentTarget;
        CoverEntity currentCoverEnity;
        CoverEntity.CoverPoint targetCoverPoint;
        Vector3 savedLocation;

        #endregion

        private void Awake()
        {
            Stats = GetComponent<StatsController>();
            Inventory = GetComponent<PlayerInventory>();
            Animations = GetComponent<PlayerAnimation>();
            Character = GetComponent<CharacterController>();
            VFX = GetComponent<PlayerVFX>();
            Actions = GetComponent<PlayerActionsController>();
        }

        public void Initialize(PlayerSO _playerSO)
        {
            playerSO = _playerSO;

            // Create and equip default inventory items
            Gun defaultPistol = playerSO.GetDefaultPistolSO().CreateItemAs<Gun>(null, Vector3.zero, Vector3.zero, true);
            defaultPistol.Initialize(playerSO.GetDefaultPistolSO());
            defaultPistol.Equip(gameObject);

            Equipment defaultSword = playerSO.GetDefaultMeleeSO().CreateItemAs<Equipment>(null, Vector3.zero, Vector3.zero, true);
            defaultSword.Initialize(playerSO.GetDefaultMeleeSO());
            defaultSword.Equip(gameObject);

            Stats.SubscribeToOnCurrentValueIsZero(Generic.Stats.StatType.Health, HandleDeath);

            Animations.Initialize();

            mainStateMachine = new StateMachine();
            pInactiveState = new PlayerInactiveState(this, mainStateMachine);
            pDeadState = new PlayerDeadState(this, mainStateMachine);
            pMovingState = new PlayerMovingState(this, mainStateMachine);
            pIdlState = new PlayerIdlState(this, mainStateMachine);
            pReloadState = new PlayerReloadState(this, mainStateMachine);
            pAimingState = new PlayerAimingState(this, mainStateMachine);
            pAimingAndMovingState = new PlayerAimingAndMovingState(this, mainStateMachine);
            pThrowObjectPrepState = new PlayerThrowObjectPrepState(this, mainStateMachine);
            pThrowingObjectState = new PlayerThrowingObjectState(this, mainStateMachine);
            pInteractWithReparableState = new PlayerInteractWithReparableState(this, mainStateMachine);
            pInteractWithShopState = new PlayerInteractWithShopState(this, mainStateMachine);
            pGoToCoverPointState = new PlayerGoToCoverPointState(this, mainStateMachine);
            pMoveToNextCoverPointState = new PlayerMoveToNextCoverPointState(this, mainStateMachine);
            pCoverAnimationState = new PlayerCoverAnimationState(this, mainStateMachine);
            pCoveringState = new PlayerCoveringState(this, mainStateMachine);
            pDashState = new PlayerDashState(this, mainStateMachine);
            pAttackState = new PlayerAttackState(this, mainStateMachine);

            mainStateMachine.Initialize(pIdlState);

            Actions.Initialize(this, mainStateMachine);
        }

        public void FrameUpdate()
        {
            mainStateMachine.CurrentState().HandleLogic();
            mainStateMachine.CurrentState().HandleAxisInput();
        }

        public void PhysicsUpdate()
        {
            mainStateMachine.CurrentState().HandlePhysics();
        }

        public void LastUpdate()
        {
            mainStateMachine.CurrentState().HandleLateUpdate();
        }

        public void OnPulledOut()
        {
            GameManager.S.AddPlayerController(this);
        }

        public void Reset() { }

        void HandleDeath()
        {
            IDestroyable[] destroyables = GetComponents<IDestroyable>();
            for (int i = 0; i < destroyables.Length; i++) destroyables[i].OnDestroyed();

            TearDown();
        }

        public void TearDown()
        {
            if (!isActiveAndEnabled) return;
            
            mainStateMachine.ChangeState(pDeadState); // PLACEHOLDER: this might be different

            Stats.UnSubscribeToOnCurrentValueIsZero(Generic.Stats.StatType.Health, HandleDeath);

            GameManager.S.RemovePlayerController(this);

            Inventory.Reset();
            Stats.Reset();
            Actions.Reset();

            // Placeholder until find a solution for this. When player dies the VFXs are disables as they are childs and will be enabled after
            // pooling back the player showing the particles effect. This way we teardown them before they get disabled.
            BaseVFX[] vfxs = GetComponentsInChildren<BaseVFX>();
            for (int i = 0; i < vfxs.Length; i++) vfxs[i].TearDown();

            if (PoolOrigin != null)
            {
                // Return object to pool
                Reset();
                PoolOrigin.ReturnObject(this);
            }
            else Destroy(gameObject);
        }

        #region NavAgentTarget

        public GameObject Target() => gameObject;
        public TargetType Type() => TargetType.Player;
        public float UnaccesibleRadius() => 1f;

        #endregion

        #region ExternalStatesControl

        public void ChangeStateToInactive()
        {
            mainStateMachine.ChangeState(pInactiveState);
        }
        public void ChangeStateToIdl()
        {
            mainStateMachine.ChangeState(pIdlState);
        }
        public void ChangeStateToInteractWithReparableState()
        {
            mainStateMachine.ChangeState(pInteractWithReparableState);
        }
        public void ChangeStateToInteractWithShopState()
        {
            mainStateMachine.ChangeState(pInteractWithShopState);
        }
        public void ChangeStateToGoToCoverPointState()
        {
            mainStateMachine.ChangeState(pGoToCoverPointState);
        }

        #endregion

        #region Setters

        public void SetInteractable(IInteractable interactable)
        {
            currentInteractable = interactable;
        }
        public void SetTarget(ITarget target)
        {
            currentTarget = target;
        }
        public void SetSavedLocation(Vector3 location)
        {
            savedLocation = location;
        }
        public void SetTargetCoverPoint(CoverEntity.CoverPoint point)
        {
            targetCoverPoint = point;
        }
        public void SetCoverEntity(CoverEntity coverEntity)
        {
            currentCoverEnity = coverEntity;
        }

        #endregion

        #region Accessors

        public PlayerDeadState GetPlayerDeadState() => pDeadState;
        public PlayerIdlState GetPlayerIdlState() => pIdlState;
        public PlayerMovingState GetPlayerMovingState() => pMovingState;
        public PlayerAimingState GetPlayerAimingState() => pAimingState;
        public PlayerAimingAndMovingState GetPlayerAimingAndMovingState() => pAimingAndMovingState;
        public PlayerReloadState GetPlayerReloadState() => pReloadState;
        public PlayerThrowObjectPrepState GetPlayerThrowObjectPrepState() => pThrowObjectPrepState;
        public PlayerThrowingObjectState GetPlayerThrowingObjectState() => pThrowingObjectState;
        public PlayerInteractWithReparableState GetPlayerInteractWithReparableState() => pInteractWithReparableState;
        public PlayerInteractWithShopState GetPlayerInteractWithShopState() => pInteractWithShopState;
        public PlayerGoToCoverPointState GetPlayerGoToCoverPointState() => pGoToCoverPointState;
        public PlayerCoverAnimationState GetPlayerCoverAnimationState() => pCoverAnimationState;
        public PlayerMoveToNextCoverPointState GetPlayerMoveToNextCoverPointState() => pMoveToNextCoverPointState;
        public PlayerCoveringState GetPlayerCoveringState() => pCoveringState;
        public PlayerAttackState GetPlayerAttackState() => pAttackState;
        public PlayerDashState GetPlayerDashState() => pDashState;

        public Transform GetBoneSpineTransform() => spineTransform;
        public Item GetCurrentUsingItem() => Inventory.GetCurrentUsingItem();
        public Gun GetCurrentUsingGun() => Inventory.GetCurrentUsingItem<Gun>();
        public Equipment GetEquipmentAtSlotType(InventorySlotType slotType) => Inventory.GetEquipment(slotType);
        public InputSchema GetInputSchema() => playerSO.InputSchema;
        public string GetPlayerName() => playerSO.PlayerName;
        public int GetPlayerID() => playerSO.PlayerID();
        public int CurrentCredits() => playerSO.Credits();
        public bool AddCredits(int amount) => playerSO.AddCredits(amount);
        public bool UseCredits(int amount) => playerSO.UseCredits(amount);
        public bool HasCreditsToPay(int amount) => playerSO.HasCreditsToPay(amount);
        public void SubscribeToOnCreditsHasChanged(System.Action<int> listener) => playerSO.OnCreditsChanged += listener;
        public void UnSubscribeToOnCreditsHasChanged(System.Action<int> listener) => playerSO.OnCreditsChanged -= listener;

        public float GetInteractDistance() => playerSO.InteractDistance();
        public LayerMask GetInteractLayerMask() => playerSO.InteractLayerMask();
        public IInteractable GetCurrentInteractable() => currentInteractable;
        public ITarget GetCurrentTarget() => currentTarget;
        public Vector3 GetSavedLocation() => savedLocation;
        public CoverEntity.CoverPoint GetTargetCoverPoint() => targetCoverPoint;
        public CoverEntity GetCurrentCoverEntity() => currentCoverEnity;

        #endregion
    }
}

