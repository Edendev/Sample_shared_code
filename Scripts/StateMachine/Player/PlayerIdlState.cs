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
            GameManager.PlayerInput.SubscribeToInputAction(inputSchema, InputAction.Down, InputType.Reload, controller.Actions.Process<PlayerReloadAction>);

            if (inputSchema == InputSchema.KeyboardAndMouse)
            {
                GameManager.PlayerInput.SubscribeToInputAction(inputSchema, InputAction.Down, InputType.Fire, controller.Actions.ProcessMainTriggerAction);
                GameManager.PlayerInput.SubscribeToInputAction(inputSchema, InputAction.Down, InputType.MainItem, controller.Actions.Process<PlayerUseMainHandItemAction>);
                GameManager.PlayerInput.SubscribeToInputAction(inputSchema, InputAction.Down, InputType.SecondaryItem, controller.Actions.Process<PlayerUseSecondaryHandItemAction>);
                GameManager.PlayerInput.SubscribeToInputAction(inputSchema, InputAction.Down, InputType.MeleeItem, controller.Actions.Process<PlayerUseMeleeItemAction>);
                GameManager.PlayerInput.SubscribeToInputAction(inputSchema, InputAction.Down, InputType.MiscItem, controller.Actions.Process<PlayerUseMiscItemAction>);
                GameManager.PlayerInput.SubscribeToInputAction(inputSchema, InputAction.Down, InputType.Aim, OnAimInputDown);
            }
            else if (inputSchema == InputSchema.Gamepad)
            {
                GameManager.PlayerInput.SubscribeToAxisAction(inputSchema, AxisType.Fire, InputAction.Down, OnFireAxisDown);
                GameManager.PlayerInput.SubscribeToInputAction(inputSchema, InputAction.Down, InputType.NextItem, controller.Actions.Process<PlayerUseNextItemAction>);
                GameManager.PlayerInput.SubscribeToInputAction(inputSchema, InputAction.Down, InputType.PreviousItem, controller.Actions.Process<PlayerUsePreviousItemAction>);
            }
        }

        public override void Exit()
        {
            GameManager.PlayerInput.UnSubscribeToInputAction(inputSchema, InputAction.Down, InputType.Dash, controller.Actions.Process<PlayerDashAction>);
            GameManager.PlayerInput.UnSubscribeToInputAction(inputSchema, InputAction.Down, InputType.Interact, controller.Actions.Process<PlayerInteractAction>);
            GameManager.PlayerInput.UnSubscribeToInputAction(inputSchema, InputAction.Down, InputType.Reload, controller.Actions.Process<PlayerReloadAction>);

            if (inputSchema == InputSchema.KeyboardAndMouse)
            {
                GameManager.PlayerInput.UnSubscribeToInputAction(inputSchema, InputAction.Down, InputType.Fire, controller.Actions.ProcessMainTriggerAction);
                GameManager.PlayerInput.UnSubscribeToInputAction(inputSchema, InputAction.Down, InputType.MainItem, controller.Actions.Process<PlayerUseMainHandItemAction>);
                GameManager.PlayerInput.UnSubscribeToInputAction(inputSchema, InputAction.Down, InputType.SecondaryItem, controller.Actions.Process<PlayerUseSecondaryHandItemAction>);
                GameManager.PlayerInput.UnSubscribeToInputAction(inputSchema, InputAction.Down, InputType.MeleeItem, controller.Actions.Process<PlayerUseMeleeItemAction>);
                GameManager.PlayerInput.UnSubscribeToInputAction(inputSchema, InputAction.Down, InputType.MiscItem, controller.Actions.Process<PlayerUseMiscItemAction>);
                GameManager.PlayerInput.UnSubscribeToInputAction(inputSchema, InputAction.Down, InputType.Aim, OnAimInputDown);
            }
            else if (inputSchema == InputSchema.Gamepad)
            {
                GameManager.PlayerInput.UnSubscribeToAxisAction(inputSchema, AxisType.Fire, InputAction.Down, OnFireAxisDown);
                GameManager.PlayerInput.UnSubscribeToInputAction(inputSchema, InputAction.Down, InputType.NextItem, controller.Actions.Process<PlayerUseNextItemAction>);
                GameManager.PlayerInput.UnSubscribeToInputAction(inputSchema, InputAction.Down, InputType.PreviousItem, controller.Actions.Process<PlayerUsePreviousItemAction>);
            }

            base.Exit();
        }

        void OnAimInputDown()
        {
            if (controller.Inventory.GetCurrentUsingItem<Gun>() != null) stateMachine.ChangeState(controller.GetPlayerAimingState());
        }

        void OnFireAxisDown(AxisPressedDirection axisPressedDirection)
        {
            controller.Actions.ProcessMainTriggerAction();
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
                float aimVerticalAxisValue = GameManager.PlayerInput.GetAxisValue(PlayerInput.PlayerInput.GetAxis(controller.GetInputSchema(), AxisType.Aim, AxisDirection.Vertical));
                float aimHorizontalAxisValue = GameManager.PlayerInput.GetAxisValue(PlayerInput.PlayerInput.GetAxis(controller.GetInputSchema(), AxisType.Aim, AxisDirection.Horizontal));

                if (Mathf.Abs(aimVerticalAxisValue) > AXIS_AIM_SENSITIVITY || Mathf.Abs(aimHorizontalAxisValue) > AXIS_AIM_SENSITIVITY)
                {
                    if (controller.Inventory.GetCurrentUsingItem<Gun>() != null) stateMachine.ChangeState(controller.GetPlayerAimingState());
                    return;
                }
            }
        }
    }
}
