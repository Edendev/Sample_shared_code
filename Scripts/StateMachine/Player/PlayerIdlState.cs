using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.MonoBehaviours.Player;
using Game.MonoBehaviours.Managers;
using Game.Generic.PlayerInput;
using Game.MonoBehaviours.Inventory;
using Game.Interfaces;
using Game.Generic.PlayerActions;

namespace Game.Generic.StateMachine.PlayerStates
{
    public class PlayerIdlState : PlayerState
    {
        public PlayerIdlState(PlayerController _controller, StateMachine _stateMachine) : base(_controller, _stateMachine)
        {

        }

        public override void Enter()
        {
            base.Enter();

            GameManager.PlayerInput.SubscribeToInputAction(inputSchema, InputAction.Down, InputType.Dash, controller.Actions.Process<PlayerDashAction>);
            GameManager.PlayerInput.SubscribeToInputAction(inputSchema, InputAction.Down, InputType.Interact, controller.Actions.Process<PlayerInteractAction>);
            GameManager.PlayerInput.SubscribeToInputAction(inputSchema, InputAction.Down, InputType.Block, controller.Actions.Process<PlayerBlockAction>);
            GameManager.PlayerInput.SubscribeToInputAction(inputSchema, InputAction.Down, InputType.Skill_slot_0, controller.Actions.Process<PlayerUseSkillAtSlot0Action>);
            GameManager.PlayerInput.SubscribeToInputAction(inputSchema, InputAction.Down, InputType.Skill_slot_1, controller.Actions.Process<PlayerUseSkillAtSlot1Action>);
            GameManager.PlayerInput.SubscribeToInputAction(inputSchema, InputAction.Pressed, InputType.Attack, controller.Actions.ProcessMainTriggerAction);

            if (inputSchema == InputSchema.KeyboardAndMouse)
            {
                GameManager.PlayerInput.SubscribeToInputAction(inputSchema, InputAction.Down, InputType.PreviousItem, controller.Actions.Process<PlayerEquipPreviousMiscAction>);
                GameManager.PlayerInput.SubscribeToInputAction(inputSchema, InputAction.Down, InputType.NextItem, controller.Actions.Process<PlayerEquipNextMiscAction>);
                GameManager.PlayerInput.SubscribeToInputAction(inputSchema, InputAction.Down, InputType.UseItem, controller.Actions.Process<PlayerUseMiscItemAction>);
                GameManager.PlayerInput.SubscribeToInputAction(inputSchema, InputAction.Pressed, InputType.Attack, OnAttackInputDown);
            }
            else if (inputSchema == InputSchema.Gamepad)
            {
                GameManager.PlayerInput.SubscribeToAxisAction(inputSchema, AxisType.Attack, InputAction.Pressed, OnAttackAxisDown);
                GameManager.PlayerInput.SubscribeToAxisAction(inputSchema, AxisType.Navigation, InputAction.Down, OnNavigationAxisDown);
            }
        }

        public override void Exit()
        {
            GameManager.PlayerInput.UnSubscribeToInputAction(inputSchema, InputAction.Down, InputType.Dash, controller.Actions.Process<PlayerDashAction>);
            GameManager.PlayerInput.UnSubscribeToInputAction(inputSchema, InputAction.Down, InputType.Interact, controller.Actions.Process<PlayerInteractAction>);
            GameManager.PlayerInput.UnSubscribeToInputAction(inputSchema, InputAction.Down, InputType.Block, controller.Actions.Process<PlayerBlockAction>);
            GameManager.PlayerInput.UnSubscribeToInputAction(inputSchema, InputAction.Down, InputType.Skill_slot_0, controller.Actions.Process<PlayerUseSkillAtSlot0Action>);
            GameManager.PlayerInput.UnSubscribeToInputAction(inputSchema, InputAction.Down, InputType.Skill_slot_1, controller.Actions.Process<PlayerUseSkillAtSlot1Action>);
            GameManager.PlayerInput.UnSubscribeToInputAction(inputSchema, InputAction.Pressed, InputType.Attack, controller.Actions.ProcessMainTriggerAction);

            if (inputSchema == InputSchema.KeyboardAndMouse)
            {
                GameManager.PlayerInput.UnSubscribeToInputAction(inputSchema, InputAction.Down, InputType.PreviousItem, controller.Actions.Process<PlayerEquipPreviousMiscAction>);
                GameManager.PlayerInput.UnSubscribeToInputAction(inputSchema, InputAction.Down, InputType.NextItem, controller.Actions.Process<PlayerEquipNextMiscAction>);
                GameManager.PlayerInput.UnSubscribeToInputAction(inputSchema, InputAction.Down, InputType.UseItem, controller.Actions.Process<PlayerUseMiscItemAction>);
                GameManager.PlayerInput.UnSubscribeToInputAction(inputSchema, InputAction.Pressed, InputType.Attack, OnAttackInputDown);
            }
            else if (inputSchema == InputSchema.Gamepad)
            {
                GameManager.PlayerInput.UnSubscribeToAxisAction(inputSchema, AxisType.Attack, InputAction.Pressed, OnAttackAxisDown);
                GameManager.PlayerInput.UnSubscribeToAxisAction(inputSchema, AxisType.Navigation, InputAction.Down, OnNavigationAxisDown);
            }

            base.Exit();
        }

        void OnAttackInputDown()
        {
            if (controller.Inventory.GetCurrentUsingItem<Gun>() != null) stateMachine.ChangeState(controller.GetPlayerAimingState());
        }

        void OnAttackAxisDown(AxisPressedDirection axisPressedDirection)
        {
            controller.Actions.ProcessMainTriggerAction();
        }

        void OnNavigationAxisDown(AxisPressedDirection axisPressedDirection)
        {
            if (axisPressedDirection == AxisPressedDirection.Right) controller.Actions.Process<PlayerEquipNextMiscAction>();
            else if (axisPressedDirection == AxisPressedDirection.Left) controller.Actions.Process<PlayerEquipPreviousMiscAction>();
            else if (axisPressedDirection == AxisPressedDirection.Up) controller.Actions.Process<PlayerUseMiscItemAction>();
        }

        public override void HandleAxisInput()
        {
            base.HandleAxisInput();

            float verticalAxisValue = GameManager.PlayerInput.GetAxisValue(PlayerInput.PlayerInput.GetAxis(controller.GetInputSchema(), AxisType.Movement, AxisDirection.Vertical));
            float horizontalAxisValue = GameManager.PlayerInput.GetAxisValue(PlayerInput.PlayerInput.GetAxis(controller.GetInputSchema(), AxisType.Movement, AxisDirection.Horizontal));
            
            if (Mathf.Abs(verticalAxisValue) > AXIS_MOTION_SENSITIVITY || Mathf.Abs(horizontalAxisValue) > AXIS_MOTION_SENSITIVITY)
            {
                stateMachine.ChangeState(controller.GetPlayerMovingState());
                return;
            }

            if (inputSchema == InputSchema.Gamepad)
            {
                float aimVerticalAxisValue = GameManager.PlayerInput.GetAxisValue(PlayerInput.PlayerInput.GetAxis(controller.GetInputSchema(), AxisType.Attack, AxisDirection.Vertical));
                float aimHorizontalAxisValue = GameManager.PlayerInput.GetAxisValue(PlayerInput.PlayerInput.GetAxis(controller.GetInputSchema(), AxisType.Attack, AxisDirection.Horizontal));

                if (Mathf.Abs(aimVerticalAxisValue) > AXIS_AIM_SENSITIVITY || Mathf.Abs(aimHorizontalAxisValue) > AXIS_AIM_SENSITIVITY)
                {
                    if (controller.Inventory.GetCurrentUsingItem<Gun>() != null) stateMachine.ChangeState(controller.GetPlayerAimingState());
                    return;
                }
            }
        }
    }
}
