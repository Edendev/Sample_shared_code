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
using Game.MonoBehaviours.SkillsSystem;
using Game.MonoBehaviours.Targets;
using Game.MonoBehaviours.VFX;
using Game.Generic.SkillsSystem;
using Game.Generic.PlayerActions;

namespace Game.MonoBehaviours.Player
{
    [RequireComponent(typeof(PlayerVFX))]
    [RequireComponent(typeof(PlayerAnimation))]
    [RequireComponent(typeof(PlayerActionsController))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SkillsController))]
    [RequireComponent(typeof(StatsController))]
    [RequireComponent(typeof(PlayerInventory))]
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
        PlayerCoveringAnimState pCoveringAnimState;
        PlayerCoveringState pCoveringState;
        PlayerCoveringJumpState pCoveringJumpState;
        PlayerCoveringToAimingAnimState pCoveringToAimingAnimState;
        PlayerCoveringAimingState pCoveringAimingState;
        PlayerAttackState pAttackState;
        PlayerDashState pDashState;
        PlayerBlockingState pBlockingState;

        PlayerUseWindAoeSkillState pUseWindAoeSkillState;
        PlayerUseSummonSwordSkillState pUseSummonSwordSkillState;
        PlayerUseMultiShootSkillState pUseMultiShootSkillState;

        #endregion

        public StatsController Stats { get; private set; }
        public PlayerInventory Inventory { get; private set; }
        public PlayerAnimation Animations { get; private set; }
        public PlayerActionsController Actions { get; private set; }
        public CharacterController Character { get; private set; }
        public PlayerVFX VFX { get; private set; }
        public SkillsController Skills { get; private set; }

        public ObjectPool<PlayerController> PoolOrigin { get; set; }

        PlayerSO playerSO;

        [Header("Transforms")]
        [SerializeField] Transform spineTransform;
        [SerializeField] Transform visionTransform;
        [SerializeField] Transform gunVisionTransform;

        public Quaternion SpineStartingRotation { get; private set; }

        [Header("Targeting")]
        [SerializeField] TargetGetter targetGetter; // Might be necessary to have it as a separate child to place the transform in specific location
        [SerializeField] BaseVFX targetVFX;
        ObjectPool<BaseVFX> targetVFXPool;
        BaseVFX targetVFXObj;

        #region Blackboard

        IInteractable currentInteractable;
        ITarget currentTarget;
        CoverEntity currentCoverEnity;
        CoverPoint currentCoverPoint;
        CoverPoint targetCoverPoint;
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
            Skills = GetComponent<SkillsController>();
        }

        private void OnDestroy()
        {
            GameManager.Pooling.ReturnObjectPool<BaseVFX>(targetVFX, 1, true);
        }

        public void Initialize(PlayerSO _playerSO)
        {
            playerSO = _playerSO;

            // Save the starting rotation for further resetting on player state exit
            SpineStartingRotation = spineTransform.rotation;

            // Create default items and add to inventory
            Inventory.TryAddItem(playerSO.CharacterSO.GetDefaultWeaponSO());
            Inventory.TryEquipItem(playerSO.CharacterSO.GetDefaultWeaponSO());
            Inventory.TryAddItem(playerSO.CharacterSO.GetDefaultConsumableSO());
            Inventory.TryAddItem(playerSO.CharacterSO.GetDefaultConsumableSO());
            Inventory.TryAddItem(playerSO.CharacterSO.GetDefaultConsumableSO());
            Inventory.TryAddItem(playerSO.CharacterSO.GetDefaultConsumable1SO());
            Inventory.TryAddItem(playerSO.CharacterSO.GetDefaultConsumable1SO());

            // Initialize stats and subscribe to death
            Stats.Initialize(playerSO.CharacterSO.GetStatsCollection());
            Stats.SubscribeToOnCurrentValueIsZero(Generic.Stats.StatType.Health, HandleDeath);

            Skills.Initialize(this);
            // Placeholder
            Skills.LearnSkill(SkillType.WindAoe);
            Skills.LearnSkill(SkillType.SwordSummon);
            Skills.LearnSkill(SkillType.MultiShoot);
         //   Skills.SetLearnedSkillInSlot(SkillType.WindAoe, 0);
            if (playerSO.CharacterSO.GetCharacterType() == ScriptableObjects.Characters.CharacterType.Melee)
                Skills.SetLearnedSkillInSlot(SkillType.SwordSummon, 0);
            if (playerSO.CharacterSO.GetCharacterType() == ScriptableObjects.Characters.CharacterType.Ranged)
                Skills.SetLearnedSkillInSlot(SkillType.MultiShoot, 0);

            Animations.Initialize(playerSO.CharacterSO.GetAnimationSO());

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
            pCoveringToAimingAnimState = new PlayerCoveringToAimingAnimState(this, mainStateMachine);
            pCoveringAimingState = new PlayerCoveringAimingState(this, mainStateMachine);
            pCoveringAnimState = new PlayerCoveringAnimState(this, mainStateMachine);
            pCoveringState = new PlayerCoveringState(this, mainStateMachine);
            pCoveringJumpState = new PlayerCoveringJumpState(this, mainStateMachine);
            pDashState = new PlayerDashState(this, mainStateMachine);
            pAttackState = new PlayerAttackState(this, mainStateMachine);
            pBlockingState = new PlayerBlockingState(this, mainStateMachine);

            pUseWindAoeSkillState = new PlayerUseWindAoeSkillState(this, mainStateMachine);
            pUseSummonSwordSkillState = new PlayerUseSummonSwordSkillState(this, mainStateMachine);
            pUseMultiShootSkillState = new PlayerUseMultiShootSkillState(this, mainStateMachine);

            mainStateMachine.Initialize(pIdlState);

            // INitialize player actions and subscribe current using item to main trigger action
            Actions.Initialize(this, mainStateMachine);
            Actions.Process<PlayerUseMainHandItemAction>();

            // Targeting system
            targetVFXPool = GameManager.Pooling.GetObjectPool<BaseVFX>(targetVFX, 1, true);
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

        public void Reset()
        {
            currentInteractable = null;
            currentTarget = null;
            currentCoverEnity = null;
            currentCoverPoint = null;
            targetCoverPoint = null;
            savedLocation = Vector3.zero;
        }

        void HandleDeath()
        {
            IDestroyable[] destroyables = GetComponents<IDestroyable>();
            for (int i = 0; i < destroyables.Length; i++) destroyables[i].OnDestroyed();

            TearDown();
        }

        public void TearDown()
        {
            mainStateMachine.ChangeState(pDeadState); // PLACEHOLDER: this might be different

            Stats.UnSubscribeToOnCurrentValueIsZero(Generic.Stats.StatType.Health, HandleDeath);

            Inventory.Reset();
            Stats.Reset();
            Actions.Reset();

            Animations.TearDown();

            // Targeting
            if (targetVFXObj != null) targetVFXObj.TearDown();

            // Placeholder until find a solution for this. When player dies the VFXs are disables as they are childs and will be enabled after
            // pooling back the player showing the particles effect. This way we teardown them before they get disabled.
            BaseVFX[] vfxs = GetComponentsInChildren<BaseVFX>();
            for (int i = 0; i < vfxs.Length; i++) vfxs[i].TearDown();

            GameManager.S.RemovePlayerController(this);

            if (PoolOrigin != null)
            {
                // Return object to pool
                Reset();
                PoolOrigin.ReturnObject(this);
            }
            else Destroy(gameObject);
        }

        #region Target

        public GameObject Target() => gameObject;
        public TargetType Type() => TargetType.Player;
        public Transform VisionPoint() => visionTransform;
        public float UnaccesibleRadius() => Character.radius*2f;

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
            if (currentTarget == target) return;
            if (targetVFXObj != null) targetVFXObj.TearDown();

            currentTarget = target;

            // If target is not null 
            if (HasCurrentTarget()) targetVFXObj = targetVFXPool.GetObject(currentTarget.Target().transform, Vector3.zero, Space.Self);
        }
        public void SetSavedLocation(Vector3 location)
        {
            savedLocation = location;
        }
        public void SetTargetCoverPoint(CoverPoint point)
        {
            targetCoverPoint = point;
        }
        public void SetCoverEntity(CoverEntity coverEntity)
        {
            currentCoverEnity = coverEntity;
        }
        public void SetCurrentCoverPoint(CoverPoint point)
        {
            currentCoverPoint = point;
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
        public PlayerMoveToNextCoverPointState GetPlayerMoveToNextCoverPointState() => pMoveToNextCoverPointState;
        public PlayerCoveringToAimingAnimState GetPlayerCoveringToAimingAnimState() => pCoveringToAimingAnimState;
        public PlayerCoveringAnimState GetPlayerCoveringAnimState() => pCoveringAnimState;
        public PlayerCoveringAimingState GetPlayerCoveringAimingState() => pCoveringAimingState;
        public PlayerCoveringState GetPlayerCoveringState() => pCoveringState;
        public PlayerCoveringJumpState GetPlayerCoveringJumpState() => pCoveringJumpState;
        public PlayerAttackState GetPlayerAttackState() => pAttackState;
        public PlayerDashState GetPlayerDashState() => pDashState;
        public PlayerUseWindAoeSkillState GetPlayerUseWindAoeSkillState() => pUseWindAoeSkillState;
        public PlayerUseSummonSwordSkillState GetPlayerUseSummonSwordSkillState() => pUseSummonSwordSkillState;
        public PlayerUseMultiShootSkillState GetPlayerUseMultiShootSkillState() => pUseMultiShootSkillState;
        public PlayerBlockingState GetPlayerBlockingState() => pBlockingState;

        public TargetGetter TargetGetter() => targetGetter;

        public Transform GetBoneSpineTransform() => spineTransform;
        public Transform GetGunVisionTransform() => gunVisionTransform;
        public Item GetCurrentUsingItem() => Inventory.GetCurrentUsingItem();
        public Item GetItemAtSlotType(InventorySlotType slotType) => Inventory.GetItemAtSlotType(slotType);
        public Gun GetCurrentUsingGun() => Inventory.GetCurrentUsingItem<Gun>();
        public InputSchema GetInputSchema() => playerSO.InputSchema;
        public string GetPlayerName() => playerSO.PlayerName;
        public int GetPlayerID() => playerSO.PlayerID();
        public Sprite GetPlayerAvatar() => playerSO.PlayerAvatar;
        public int CurrentCredits() => playerSO.Credits();
        public bool AddCredits(int amount) => playerSO.AddCredits(amount);
        public bool UseCredits(int amount) => playerSO.UseCredits(amount);
        public bool HasCreditsToPay(int amount) => playerSO.HasCreditsToPay(amount);
        public void SubscribeToOnCreditsHasChanged(System.Action<int> listener) => playerSO.OnCreditsChanged += listener;
        public void UnSubscribeToOnCreditsHasChanged(System.Action<int> listener) => playerSO.OnCreditsChanged -= listener;

        public int GetDefaultNumAttacksBlocked() => playerSO.CharacterSO.GetDefaultNumAttacksCanBlock();
        public float GetInteractDistance() => playerSO.InteractDistance();
        public LayerMask GetInteractLayerMask() => playerSO.InteractLayerMask();
        public IInteractable GetCurrentInteractable() => currentInteractable;
        public ITarget GetCurrentTarget() => currentTarget;
        /// <summary>
        /// Returns null if does not have a target
        /// </summary>
        /// <returns></returns>
        public GameObject GetCurrentTargetObject()
        {
            if (!HasCurrentTarget()) return null;
            return currentTarget.Target();
        }
        public Vector3 GetSavedLocation() => savedLocation;
        public CoverPoint GetTargetCoverPoint() => targetCoverPoint;
        public CoverPoint GetCurrentCoverPoint() => currentCoverPoint;
        public CoverEntity GetCurrentCoverEntity() => currentCoverEnity;

        /// <summary>
        /// Is the statemachine current state the given type?
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsCurrentStateThisOrSubclassOf<T>()
            where T : PlayerState
        {
            if (mainStateMachine.CurrentState().GetType() == typeof(T)) return true;
            else if (mainStateMachine.CurrentState().GetType().IsSubclassOf(typeof(T))) return true;
            return false;
        }

        public bool HasCurrentTarget()
        {
            return (currentTarget != null && currentTarget.Target() != null);
        }

        #endregion
    }
}

