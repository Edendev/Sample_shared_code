using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Generic.PlayerInput
{
    public enum InputType
    {
        Aim, Fire, Reload, Dash, Interact,
        MainItem, SecondaryItem, MeleeItem, MiscItem, NextItem, PreviousItem,
        Confirm, Cancel, MouseConfirm, Continue, Escape,
    }

    public enum InputSchema
    {
        KeyboardAndMouse, Gamepad
    }

    public enum InputAction
    {
        Down, Up, Pressed
    }

    public enum AxisType
    {
        Movement, Aim, Navigation, Fire, Throw
    }

    public enum AxisDirection
    {
        Vertical, Horizontal
    }

    public enum AxisPressedDirection
    {
        None, Left, Right, Down, Up
    }

    public class AxisDefinition
    {
        string horizontal;
        string vertical;

        public AxisDefinition(string _horizontal, string _vertical)
        {
            horizontal = _horizontal;
            vertical = _vertical;
        }

        public string GetAxisName(AxisDirection direction)
        {
            if (direction == AxisDirection.Horizontal) return horizontal;
            else return vertical;
        }
    }

    public static class PlayerInput
    {
        private static readonly Dictionary<InputType, KeyCode> key_input_schema_keyboardAndMouse = new Dictionary<InputType, KeyCode>()
        {
            {InputType.Aim, KeyCode.Mouse1},
            {InputType.Fire, KeyCode.Mouse0},
            {InputType.Confirm, KeyCode.Return},
            {InputType.MouseConfirm, KeyCode.Mouse0},
            {InputType.Continue, KeyCode.Return },
            {InputType.Cancel, KeyCode.C },
            {InputType.Escape, KeyCode.Escape },
            {InputType.Interact, KeyCode.F },
            {InputType.Reload, KeyCode.R },
            {InputType.Dash, KeyCode.LeftShift},
            {InputType.MainItem, KeyCode.Alpha1 },
            {InputType.SecondaryItem, KeyCode.Alpha2 },
            {InputType.MeleeItem, KeyCode.Alpha3 },
            {InputType.MiscItem, KeyCode.Alpha4 }
        };

        private static readonly Dictionary<InputType, string> button_input_schema_gamepad = new Dictionary<InputType, string>()
        {
            {InputType.Confirm, "XBOX_A" },
            {InputType.Continue, "XBOX_START" },
            {InputType.Cancel, "XBOX_Y" },
            {InputType.Escape, "XBOX_START" },
            {InputType.Interact, "XBOX_B" },
            {InputType.Reload, "XBOX_X" },
            {InputType.Dash, "XBOX_A"},
            {InputType.NextItem, "XBOX_RB"},
            {InputType.PreviousItem, "XBOX_LB"}
        };

        private static readonly Dictionary<AxisType, AxisDefinition> axes_input_schema_keyboardAndMouse = new Dictionary<AxisType, AxisDefinition>()
        {
            {AxisType.Movement, new AxisDefinition("Horizontal", "Vertical") },
            {AxisType.Aim, new AxisDefinition("Mouse X", "Mouse Y") },
            {AxisType.Navigation, new AxisDefinition("Horizontal", "Vertical") }
        };

        private static readonly Dictionary<AxisType, AxisDefinition> axes_input_schema_gamepad = new Dictionary<AxisType, AxisDefinition>()
        {
            {AxisType.Movement, new AxisDefinition("Gamepad_left_horizontal", "Gamepad_left_vertical") },
            {AxisType.Aim, new AxisDefinition("Gamepad_right_horizontal", "Gamepad_right_vertical") },
            {AxisType.Navigation, new AxisDefinition("Gamepad_leftright", "Gamepad_updown") },
            {AxisType.Fire, new AxisDefinition("XBOX_RT", "XBOX_RT") },
            {AxisType.Throw, new AxisDefinition("XBOX_LT", "XBOX_LT") }
        };

        public static readonly Dictionary<InputSchema, Dictionary<InputType, KeyCode>> key_input_schemas = new Dictionary<InputSchema, Dictionary<InputType, KeyCode>>()
        {
            {InputSchema.KeyboardAndMouse, key_input_schema_keyboardAndMouse},
        };

        public static readonly Dictionary<InputSchema, Dictionary<InputType, string>> button_input_schemas = new Dictionary<InputSchema, Dictionary<InputType, string>>()
        {
            {InputSchema.Gamepad, button_input_schema_gamepad},
        };

        public static readonly Dictionary<InputSchema, Dictionary<AxisType, AxisDefinition>> axes_input_schemas = new Dictionary<InputSchema, Dictionary<AxisType, AxisDefinition>>()
        {
            {InputSchema.KeyboardAndMouse, axes_input_schema_keyboardAndMouse},
            {InputSchema.Gamepad, axes_input_schema_gamepad}
        };

        public static string GetButtonName(InputSchema schema, InputType type)
        {
            if (!button_input_schemas.ContainsKey(schema)) return "";
            if (!button_input_schemas[schema].ContainsKey(type)) return "";
            return button_input_schemas[schema][type];
        }

        public static KeyCode GetKeyCode(InputSchema schema, InputType type)
        {
            if (!key_input_schemas.ContainsKey(schema)) return KeyCode.None;
            if (!key_input_schemas[schema].ContainsKey(type)) return KeyCode.None;
            return key_input_schemas[schema][type];
        }

        public static string GetAxis(InputSchema schema, AxisType type, AxisDirection direction)
        {
            if (!axes_input_schemas.ContainsKey(schema)) return "";
            if (!axes_input_schemas[schema].ContainsKey(type)) return "";
            return axes_input_schemas[schema][type].GetAxisName(direction);
        }

    }
}

