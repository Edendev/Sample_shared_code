using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Game.ScriptableObjects.Combat.Attacks;
using Game.Generic.BehaviourTree;
using Game.Generic.Stats;
using Game.Interfaces;
using Game.ScriptableObjects.AI;
using Game.MonoBehaviours.Inventory;
using Game.MonoBehaviours.Combat.ObjectThrowers;
using Game.MonoBehaviours.Managers;
using Game.ScriptableObjects.Inventory;
using Game.Generic.ObjectPooling;
using Game.MonoBehaviours.VFX;

namespace Game.MonoBehaviours.AI
{
    [RequireComponent(typeof(BaseInventory))]
    public class EnemyModel3 : EnemyController
    {
        EnemyModel3BehaviourSO EM3behaviourSO;
        BaseInventory inventory;

        protected override void Awake()
        {
            base.Awake();

            inventory = GetComponent<BaseInventory>();
        }

        public override void Initialize(NavAgentSO _navAgentSO)
        {
            base.Initialize(_navAgentSO);
            EM3behaviourSO = behaviourSO as EnemyModel3BehaviourSO;

            // Create gun and add to inventory
            Gun newGun = EM3behaviourSO.GetGunSO().CreateItemAs<Gun>(null, Vector3.zero, Vector3.zero, true);
            newGun.Initialize(EM3behaviourSO.GetGunSO());
            newGun.Equip(gameObject);

            // Use gun
            inventory.UseItem(InventorySlotType.MainHand);

            // Get target sequence
            SequenceNode getTargetSeq = new SequenceNode("Get target main sequence");
            SelectorNode hasTargetWithLowestHealthSel = new SelectorNode("Target with lowest health selector");
            LeafNode getTargetWithLowerHealth = new LeafNode("Get target with lowest health", GetPlayerWithLowerHPAsTarget);
            LeafNode hasCurrentTargetTheLowestHealth = new LeafNode("Current target has lowest HP?", HasTheCurrentTargetTheLowestHealth);
            LeafNode hasPlayerAsTargetLeaf = new LeafNode("Has target?", CheckIfPlayerTargetExists);
            InverterNode hasNoTargetLeaf = new InverterNode("Has No target?");
            InverterNode hasNoCurrentTargetWithLowHealthLeaf = new InverterNode("Current target has No lowest HP?");
            hasNoTargetLeaf.AddChild(hasPlayerAsTargetLeaf);
            hasNoCurrentTargetWithLowHealthLeaf.AddChild(hasCurrentTargetTheLowestHealth);
            hasTargetWithLowestHealthSel.AddChild(hasNoTargetLeaf);
            hasTargetWithLowestHealthSel.AddChild(hasNoCurrentTargetWithLowHealthLeaf);
            getTargetSeq.AddChild(hasTargetWithLowestHealthSel);
            getTargetSeq.AddChild(getTargetWithLowerHealth);

            // --- Go to target sequence
            SequenceNode goToTargetSeq = new SequenceNode("Go to target main sequence");
            LeafNode goToTargetToAttackLeaf = new LeafNode("Go to target at attack distance", GoToTargetToAttack);
            goToTargetToAttackLeaf.AddResetMethod(ResetNavAgentPath_RM);
            LeafNode enableMovingAnimParamLeaf = new LeafNode("Enable moving anim parameter", EnableMovingAnimParam);
            LeafNode disableAimingAnimParamLeaf = new LeafNode("Disable aiming anim parameter", DisableAimingAnimParam);
            LeafNode enableReloadingAnimParamLeaf = new LeafNode("Enable reloadig anim parameter", EnableReloadingAnimParam);
            LeafNode disableReloadingAnimParamLeaf = new LeafNode("Disable reloading anim parameter", DisableReloadingAnimParam);
            LeafNode enableAimingAnimParamLeaf = new LeafNode("Enable aiming anim parameter", EnableAimingAnimParam);
            LeafNode successLeaf = new LeafNode("Success", BTProcessMethods.Success);
            LeafNode hasAmmoLeaf = new LeafNode("Has ammo", HasAmmo);
            goToTargetSeq.AddChild(hasPlayerAsTargetLeaf);
            goToTargetSeq.AddChild(disableAimingAnimParamLeaf);
            goToTargetSeq.AddChild(disableReloadingAnimParamLeaf);
            goToTargetSeq.AddChild(enableMovingAnimParamLeaf);
            goToTargetSeq.AddChild(goToTargetToAttackLeaf);

            // --- Shoot target loop dependency
            BehaviourTree shootTargetLoopDependency = new BehaviourTree("Dependency for shooting target loop");
            SequenceNode shootTargetLoopDepSeq = new SequenceNode("Shooting target loop dependency sequence");
            LeafNode isCurrentTargetAtAttackWithGunRangeLeaf = new LeafNode("Is current target at attack distance", IsCurrentTargetAtAttackWithGunRange);
            LeafNode hasVisitonToTargetLeaf = new LeafNode("Has vision to target", HasVisionToTarget);
            shootTargetLoopDepSeq.AddChild(hasAmmoLeaf);
            shootTargetLoopDepSeq.AddChild(isCurrentTargetAtAttackWithGunRangeLeaf);
            shootTargetLoopDepSeq.AddChild(hasVisitonToTargetLeaf);
            shootTargetLoopDependency.AddChild(shootTargetLoopDepSeq);

            // --- Prepare for shooting sequence
            SequenceNode prepareForShootSeq = new SequenceNode("Prepare for shooting main sequence", shootTargetLoopDependency);
            TimerNode waitForAnimLeaf = new TimerNode("Wait for aim animation to play", 0.8f);
            waitForAnimLeaf.AddChild(successLeaf);
            prepareForShootSeq.AddChild(enableAimingAnimParamLeaf);
            prepareForShootSeq.AddChild(waitForAnimLeaf);

            // --- Shoot target loop
            LoopNode shootTargetLoop = new LoopNode("Shoot target main loop", shootTargetLoopDependency);
            SequenceNode shootTargetSeq = new SequenceNode("Shoot target main sequence");
            LeafNode faceTargetLeaf = new LeafNode("Face target", FaceCurrentTarget);
            LeafNode fireLeaf = new LeafNode("Fire target", Fire);
            shootTargetSeq.AddChild(faceTargetLeaf);
            shootTargetSeq.AddChild(fireLeaf);
            shootTargetLoop.AddChild(shootTargetSeq);

            // --- Go to nearest save point while reloading sequence
            SequenceNode goToNearestSavePointSeq = new SequenceNode("Go to nearest save point sequence");
            LeafNode getNearestSavePointAtDistanceFromSavedTargetLeaf = new LeafNode("Get nearest save point at distance from saved target", GetNearestEmtpySavePointAtDistanceFromSavedTarget);
            LeafNode setDestinationToTargetFromSavedLeaf = new LeafNode("Get destination to target from saved", SetDestinationToCurrentTargetFromSaved);
            LeafNode saveCloserPlayerTargetLeaf = new LeafNode("Set closest player as saved target", SaveCloserPlayerTarget);
            LeafNode saveCurrentPositionLeaf = new LeafNode("Save current position", SaveCurrentPosition);
            LeafNode disableMovingAnimParamLeaf = new LeafNode("Disable moving anim parameter", DisableMovingAnimParam);
            LeafNode goToCurrentDestinationLeaf = new LeafNode("Go to current destination", GoToCurrentDestination);
            goToCurrentDestinationLeaf.AddResetMethod(ResetNavAgentPath_RM);
            goToNearestSavePointSeq.AddChild(saveCloserPlayerTargetLeaf);
            goToNearestSavePointSeq.AddChild(getNearestSavePointAtDistanceFromSavedTargetLeaf);
            goToNearestSavePointSeq.AddChild(setDestinationToTargetFromSavedLeaf);
            goToNearestSavePointSeq.AddChild(saveCurrentPositionLeaf);
            goToNearestSavePointSeq.AddChild(enableMovingAnimParamLeaf);
            goToNearestSavePointSeq.AddChild(goToCurrentDestinationLeaf);
            goToNearestSavePointSeq.AddChild(disableMovingAnimParamLeaf);

            // --- Reload sequence
            SequenceNode reloadSeq = new SequenceNode("Reload main sequence");
            TimerNode waitForReloadAnimLeaf = new TimerNode("Wait for aim animation to play", Animations.GetAnimationClipLength("_idl_reloading"));
            LeafNode reloadLeaf = new LeafNode("Reload gun", Reload);
            LeafNode canReloadLeaf = new LeafNode("Can reload gun", CanReload);
            InverterNode hasAmmoInverter = new InverterNode("Has ammo inverter");
            SelectorNode goToNearestPointOrReload = new SelectorNode("Go to nearest save point or reload");
            hasAmmoInverter.AddChild(hasAmmoLeaf);
            waitForReloadAnimLeaf.AddChild(successLeaf);
            goToNearestPointOrReload.AddChild(goToNearestSavePointSeq);
            goToNearestPointOrReload.AddChild(reloadLeaf);
            reloadSeq.AddChild(hasAmmoInverter);
            reloadSeq.AddChild(canReloadLeaf);
            reloadSeq.AddChild(disableAimingAnimParamLeaf);
            reloadSeq.AddChild(goToNearestSavePointSeq);
            reloadSeq.AddChild(enableReloadingAnimParamLeaf);
            reloadSeq.AddChild(waitForReloadAnimLeaf);
            reloadSeq.AddChild(reloadLeaf);
            reloadSeq.AddChild(disableReloadingAnimParamLeaf);

            // Dependency for moving around target to get vision
            BehaviourTree moveArounForVisionDependecy = new BehaviourTree("Moving around for vision dependency");
            SequenceNode moveArounForVisionDepSeq = new SequenceNode("Moving around for vision sequence dependency");
            InverterNode hasVisionToTargetInverter = new InverterNode("Has NO vision to target");
            hasVisionToTargetInverter.AddChild(hasVisitonToTargetLeaf);
            moveArounForVisionDepSeq.AddChild(hasPlayerAsTargetLeaf);
            moveArounForVisionDepSeq.AddChild(hasVisionToTargetInverter);
            moveArounForVisionDepSeq.AddChild(isCurrentTargetAtAttackWithGunRangeLeaf);
            moveArounForVisionDependecy.AddChild(moveArounForVisionDepSeq);

            // Move around target to find vision loop
            LoopNode moveAroundTargetForVisionLoop = new LoopNode("Move around target for vision main loop", moveArounForVisionDependecy);
            SequenceNode moveArounForVisionSeq = new SequenceNode("Move around for vision sequence");
            LeafNode setClosestDestinationToLeftRelativeToTargetLeaf = new LeafNode("Set destination to left relative to target", SetClosestDestinationRelativeToTarget);
            LeafNode goCloseToTargetLeaf = new LeafNode("Go close to target", GoCloseToTarget);
            moveArounForVisionSeq.AddChild(setClosestDestinationToLeftRelativeToTargetLeaf);
            moveArounForVisionSeq.AddChild(saveCurrentPositionLeaf);
            moveArounForVisionSeq.AddChild(disableAimingAnimParamLeaf);
            moveArounForVisionSeq.AddChild(disableReloadingAnimParamLeaf);
            moveArounForVisionSeq.AddChild(enableMovingAnimParamLeaf);
            moveArounForVisionSeq.AddChild(goToCurrentDestinationLeaf); 
            moveAroundTargetForVisionLoop.AddChild(moveArounForVisionSeq);

            SequenceNode goCloseToTargetSeq = new SequenceNode("Go to target main sequence", moveArounForVisionDependecy);
            goCloseToTargetSeq.AddChild(hasPlayerAsTargetLeaf);
            goCloseToTargetSeq.AddChild(disableAimingAnimParamLeaf);
            goCloseToTargetSeq.AddChild(disableReloadingAnimParamLeaf);
            goCloseToTargetSeq.AddChild(enableMovingAnimParamLeaf);
            goCloseToTargetSeq.AddChild(goCloseToTargetLeaf);

            // Go to attack target sequence
            SequenceNode goToAttackTargetSeq = new SequenceNode("Go to attack target main sequence");
            SequenceNode prepareAndShootSeq = new SequenceNode("Prepare and shoot sequence");
            SequenceNode goCloseAndMoveAroundTargetSeq = new SequenceNode("Go close and move around sequence");
            SelectorNode reloadOrShootSel = new SelectorNode("Reload or shoot selector");
            SelectorNode goToTargetOrMoveAroundSel = new SelectorNode("Go to target or move around selector");
            prepareAndShootSeq.AddChild(prepareForShootSeq);
            prepareAndShootSeq.AddChild(shootTargetLoop);
            reloadOrShootSel.AddChild(reloadSeq);
            reloadOrShootSel.AddChild(prepareAndShootSeq);
            goCloseAndMoveAroundTargetSeq.AddChild(goCloseToTargetSeq);
            goCloseAndMoveAroundTargetSeq.AddChild(moveAroundTargetForVisionLoop);
            goToTargetOrMoveAroundSel.AddChild(goCloseAndMoveAroundTargetSeq);
            goToTargetOrMoveAroundSel.AddChild(goToTargetSeq);
            goToAttackTargetSeq.AddChild(goToTargetOrMoveAroundSel);
            goToAttackTargetSeq.AddChild(reloadOrShootSel);
     

            // Main tree
            mainTree = new BehaviourTree("Root node");
            SelectorNode mainSeletor = new SelectorNode("Main sequence");
            mainSeletor.AddChild(goToAttackTargetSeq);
            mainSeletor.AddChild(getTargetSeq);
            mainTree.AddChild(mainSeletor);

            // mainTree.PrintTree();

            currentTreeStatus = mainTree.Process(); // First tick to get status
        }

        #region BTResetMethods

        public BehaviourTreeStatus SetClosestDestinationRelativeToTarget()
        {
            return BTProcessMethods.SetClosestDestinationAtDistRelativeToTarget(GetCurrentTarget(), this, 3f, enemySO.GetVisionLayerMask());
        }
        public BehaviourTreeStatus HasVisionToTarget()
        {
            return BTProcessMethods.HasVisionToTarget(inventory.GetInventorySlot(InventorySlotType.MainHand).Config.Parent.transform.position,
                GetCurrentTarget(), enemySO.GetVisionLayerMask());
        }
        public BehaviourTreeStatus GoToCurrentDestination() { return BTProcessMethods.GoToDestination(GetCurrentDestination(), agent); }
        public BehaviourTreeStatus GoToTargetToAttack() { return BTProcessMethods.GoToTarget(agent, GetCurrentTarget(), 
            EM3behaviourSO.GetAttackWithGunRange() - 2f, NavMesh.AllAreas); } // less 3 meter to prevent issues being at the edge
        public BehaviourTreeStatus GoCloseToTarget() { return BTProcessMethods.GoToTarget(agent, GetCurrentTarget(), 2f, NavMesh.AllAreas); }
        public BehaviourTreeStatus EnableMovingAnimParam() { return BTProcessMethods.SetAnimatorBoolParameter(Animations.Animator, EM3behaviourSO.GetMovingParamName(), true); }
        public BehaviourTreeStatus DisableMovingAnimParam() { return BTProcessMethods.SetAnimatorBoolParameter(Animations.Animator, EM3behaviourSO.GetMovingParamName(), false); }
        public BehaviourTreeStatus EnableReloadingAnimParam() { return BTProcessMethods.SetAnimatorBoolParameter(Animations.Animator, EM3behaviourSO.GetReloadingParamName(), true); }
        public BehaviourTreeStatus DisableReloadingAnimParam() { return BTProcessMethods.SetAnimatorBoolParameter(Animations.Animator, EM3behaviourSO.GetReloadingParamName(), false); }
        public BehaviourTreeStatus EnableAimingAnimParam() { return BTProcessMethods.SetAnimatorBoolParameter(Animations.Animator, EM3behaviourSO.GetAimingParamName(), true); }
        public BehaviourTreeStatus DisableAimingAnimParam() { return BTProcessMethods.SetAnimatorBoolParameter(Animations.Animator, EM3behaviourSO.GetAimingParamName(), false); }
        public BehaviourTreeStatus Fire() { return BTProcessMethods.Fire(inventory.GetCurrentUsingItem<Gun>()); }
        public BehaviourTreeStatus Reload() { return BTProcessMethods.Reload(inventory.GetCurrentUsingItem<Gun>()); }
        public BehaviourTreeStatus CanReload() { return BTProcessMethods.CanReload(inventory.GetCurrentUsingItem<Gun>()); }
        public BehaviourTreeStatus HasAmmo() { return BTProcessMethods.HasAmmo(inventory.GetCurrentUsingItem<Gun>()); }
        public BehaviourTreeStatus IsCurrentTargetAtAttackWithGunRange() { return BTProcessMethods.IsTargetAtDistance(GetCurrentTarget(), gameObject, EM3behaviourSO.GetAttackWithGunRange()); }

        public BehaviourTreeStatus GetNearestEmtpySavePointAtDistanceFromSavedTarget()
        {
            return BTProcessMethods.SetNearestEmptyTargetOfTypeAtDistanceOfOtherAsCurrent(Interfaces.TargetType.SavePoint,
            this, GetSavedTarget(), EM3behaviourSO.GetEscapeMinDistance());
        }

        #endregion

        #region BTProcessMethods

        #endregion

        public override void TearDown()
        {
            inventory.Reset();
            base.TearDown();
        }

        #region Accessors

        #endregion
    }
}
