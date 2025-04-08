using System;
using BepInEx.Logging;
using SharpDX.XInput;
using UnityEngine.InputSystem;

namespace ControllerSupport
{
    public class InputManager
    {
        private readonly ManualLogSource _logger;
        private readonly ConfigManager _configManager;
        private readonly Controller _controller;
        private readonly KeyboardSimulator _keyboardSimulator;
        private readonly MouseSimulator _mouseSimulator;
        private readonly JoystickZoneHandler _joystickZoneHandler;

        private State _previousState;
        private bool _isControllerConnected;

        public InputManager(ManualLogSource logger, ConfigManager configManager)
        {
            _logger = logger;
            _configManager = configManager;
            _controller = new Controller(UserIndex.One);
            _keyboardSimulator = new KeyboardSimulator(logger, configManager);
            _mouseSimulator = new MouseSimulator(logger, configManager);
            _joystickZoneHandler = new JoystickZoneHandler(logger, configManager, _keyboardSimulator);

            _isControllerConnected = _controller.IsConnected;
            if (_isControllerConnected)
            {
                _logger.LogInfo("Controller connected!");
                _previousState = _controller.GetState();
            }
            else
            {
                _logger.LogWarning("No controller connected. Waiting for connection...");
            }
        }

        public void Update()
        {
            // Process dev inputs first
            ProcessDevInputs();

            bool wasConnected = _isControllerConnected;
            _isControllerConnected = _controller.IsConnected;

            if (_isControllerConnected && !wasConnected)
            {
                _logger.LogInfo("Controller connected!");
                _previousState = _controller.GetState();
            }

            if (!_isControllerConnected && wasConnected)
            {
                _logger.LogWarning("Controller disconnected!");
                ReleaseAllKeys();
                return;
            }

            if (!_isControllerConnected)
                return;

            try
            {
                State state = _controller.GetState();

                // Log controller inputs if debug is enabled
                if (_configManager.DebugOutput)
                {
                    _logger.LogInfo($"Left Stick: X={state.Gamepad.LeftThumbX}, Y={state.Gamepad.LeftThumbY}");
                    _logger.LogInfo($"Right Stick: X={state.Gamepad.RightThumbX}, Y={state.Gamepad.RightThumbY}");
                    _logger.LogInfo($"Triggers: Left={state.Gamepad.LeftTrigger}, Right={state.Gamepad.RightTrigger}");
                    _logger.LogInfo($"Buttons: {state.Gamepad.Buttons}");
                }

                ProcessButtonInputs(state);
                ProcessJoystickInputs(state);
                ProcessTriggerInputs(state);

                _previousState = state;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading controller: {ex.Message}");
                _isControllerConnected = false;
                ReleaseAllKeys();
            }
        }

        private void ProcessDevInputs()
        {
            if (!_configManager.DevMode)
                return;

            // Check for numpad key presses to adjust settings
            if (Keyboard.current[Key.NumpadPlus].wasPressedThisFrame)
            {
                float newValue = _configManager.IncreaseScrollSpeed();
                _logger.LogInfo($"Increased ScrollSpeed to {newValue:F6}");
            }

            if (Keyboard.current[Key.NumpadMinus].wasPressedThisFrame)
            {
                float newValue = _configManager.DecreaseScrollSpeed();
                _logger.LogInfo($"Decreased ScrollSpeed to {newValue:F6}");
            }

            if (Keyboard.current[Key.Numpad7].wasPressedThisFrame)
            {
                float magnitude = (float)Math.Sqrt(
                    _configManager.CurrentJoystickX * _configManager.CurrentJoystickX +
                    _configManager.CurrentJoystickY * _configManager.CurrentJoystickY);

                float newValue = _configManager.UpdateDeadZoneRadius(magnitude);
                _logger.LogInfo($"Updated DeadZoneRadius to {newValue:F4} based on current joystick position");
            }

            if (Keyboard.current[Key.Numpad4].wasPressedThisFrame)
            {
                float newValue = _configManager.UpdateDiagonalZoneSize(_configManager.CurrentJoystickAngle);
                _logger.LogInfo($"Updated DiagonalZoneSize to {newValue:F4} based on current joystick angle");
            }
        }

        private void ProcessButtonInputs(State state)
        {
            // A Button - Space
            if ((state.Gamepad.Buttons & GamepadButtonFlags.A) != 0 && (_previousState.Gamepad.Buttons & GamepadButtonFlags.A) == 0)
            {
                _keyboardSimulator.SendKeyEvent(Key.Space, true);
                if (_configManager.DebugOutput) _logger.LogInfo("A pressed - Space");
            }
            else if ((state.Gamepad.Buttons & GamepadButtonFlags.A) == 0 && (_previousState.Gamepad.Buttons & GamepadButtonFlags.A) != 0)
            {
                _keyboardSimulator.SendKeyEvent(Key.Space, false);
            }

            // X Button - E
            if ((state.Gamepad.Buttons & GamepadButtonFlags.X) != 0 && (_previousState.Gamepad.Buttons & GamepadButtonFlags.X) == 0)
            {
                _keyboardSimulator.SendKeyEvent(Key.E, true);
                if (_configManager.DebugOutput) _logger.LogInfo("X pressed - E");
            }
            else if ((state.Gamepad.Buttons & GamepadButtonFlags.X) == 0 && (_previousState.Gamepad.Buttons & GamepadButtonFlags.X) != 0)
            {
                _keyboardSimulator.SendKeyEvent(Key.E, false);
            }

            // B Button - Ctrl (Toggle)
            if ((state.Gamepad.Buttons & GamepadButtonFlags.B) != 0 && (_previousState.Gamepad.Buttons & GamepadButtonFlags.B) == 0)
            {
                if (_configManager.EnableCtrlToggle)
                {
                    _keyboardSimulator.ToggleCtrl();
                    if (_configManager.DebugOutput) _logger.LogInfo($"B pressed - Ctrl {(_keyboardSimulator.IsCtrlToggled ? "ON" : "OFF")}");
                }
                else
                {
                    _keyboardSimulator.SendKeyEvent(Key.LeftCtrl, true);
                    if (_configManager.DebugOutput) _logger.LogInfo("B pressed - Ctrl");
                }
            }
            else if ((state.Gamepad.Buttons & GamepadButtonFlags.B) == 0 && (_previousState.Gamepad.Buttons & GamepadButtonFlags.B) != 0)
            {
                if (!_configManager.EnableCtrlToggle)
                {
                    _keyboardSimulator.SendKeyEvent(Key.LeftCtrl, false);
                }
            }

            // Y Button - Q
            if ((state.Gamepad.Buttons & GamepadButtonFlags.Y) != 0 && (_previousState.Gamepad.Buttons & GamepadButtonFlags.Y) == 0)
            {
                _keyboardSimulator.SendKeyEvent(Key.Q, true);
                if (_configManager.DebugOutput) _logger.LogInfo("Y pressed - Q");
            }
            else if ((state.Gamepad.Buttons & GamepadButtonFlags.Y) == 0 && (_previousState.Gamepad.Buttons & GamepadButtonFlags.Y) != 0)
            {
                _keyboardSimulator.SendKeyEvent(Key.Q, false);
            }

            // Left Stick Press (L3) - Shift
            if ((state.Gamepad.Buttons & GamepadButtonFlags.LeftThumb) != 0 && (_previousState.Gamepad.Buttons & GamepadButtonFlags.LeftThumb) == 0)
            {
                if (_configManager.EnableShiftToggle)
                {
                    _keyboardSimulator.ToggleShift();
                    if (_configManager.DebugOutput) _logger.LogInfo($"L3 pressed - Shift {(_keyboardSimulator.IsShiftPressed ? "ON" : "OFF")}");
                }
                else
                {
                    _keyboardSimulator.SendKeyEvent(Key.LeftShift, true);
                    if (_configManager.DebugOutput) _logger.LogInfo("L3 pressed - Shift ON");
                }
            }
            else if ((state.Gamepad.Buttons & GamepadButtonFlags.LeftThumb) == 0 && (_previousState.Gamepad.Buttons & GamepadButtonFlags.LeftThumb) != 0)
            {
                if (!_configManager.EnableShiftToggle)
                {
                    _keyboardSimulator.SendKeyEvent(Key.LeftShift, false);
                    if (_configManager.DebugOutput) _logger.LogInfo("L3 released - Shift OFF");
                }
            }

            // Right Shoulder - Scroll Up (continuous)
            if ((state.Gamepad.Buttons & GamepadButtonFlags.RightShoulder) != 0)
            {
                _mouseSimulator.Scroll(_configManager.ScrollSpeed);
                if (_configManager.DebugOutput) _logger.LogInfo($"RB held - Scroll Up ({_configManager.ScrollSpeed})");
            }

            // Left Shoulder - Scroll Down (continuous)
            if ((state.Gamepad.Buttons & GamepadButtonFlags.LeftShoulder) != 0)
            {
                _mouseSimulator.Scroll(-_configManager.ScrollSpeed);
                if (_configManager.DebugOutput) _logger.LogInfo($"LB held - Scroll Down ({-_configManager.ScrollSpeed})");
            }

            // D-Pad Left - 1
            if ((state.Gamepad.Buttons & GamepadButtonFlags.DPadLeft) != 0 && (_previousState.Gamepad.Buttons & GamepadButtonFlags.DPadLeft) == 0)
            {
                _keyboardSimulator.SendKeyEvent(Key.Digit1, true);
                if (_configManager.DebugOutput) _logger.LogInfo("D-Pad Left - 1");
            }
            else if ((state.Gamepad.Buttons & GamepadButtonFlags.DPadLeft) == 0 && (_previousState.Gamepad.Buttons & GamepadButtonFlags.DPadLeft) != 0)
            {
                _keyboardSimulator.SendKeyEvent(Key.Digit1, false);
            }

            // D-Pad Up - 2
            if ((state.Gamepad.Buttons & GamepadButtonFlags.DPadUp) != 0 && (_previousState.Gamepad.Buttons & GamepadButtonFlags.DPadUp) == 0)
            {
                _keyboardSimulator.SendKeyEvent(Key.Digit2, true);
                if (_configManager.DebugOutput) _logger.LogInfo("D-Pad Up - 2");
            }
            else if ((state.Gamepad.Buttons & GamepadButtonFlags.DPadUp) == 0 && (_previousState.Gamepad.Buttons & GamepadButtonFlags.DPadUp) != 0)
            {
                _keyboardSimulator.SendKeyEvent(Key.Digit2, false);
            }

            // D-Pad Right - 3
            if ((state.Gamepad.Buttons & GamepadButtonFlags.DPadRight) != 0 && (_previousState.Gamepad.Buttons & GamepadButtonFlags.DPadRight) == 0)
            {
                _keyboardSimulator.SendKeyEvent(Key.Digit3, true);
                if (_configManager.DebugOutput) _logger.LogInfo("D-Pad Right - 3");
            }
            else if ((state.Gamepad.Buttons & GamepadButtonFlags.DPadRight) == 0 && (_previousState.Gamepad.Buttons & GamepadButtonFlags.DPadRight) != 0)
            {
                _keyboardSimulator.SendKeyEvent(Key.Digit3, false);
            }

            // Start Button - Escape
            if ((state.Gamepad.Buttons & GamepadButtonFlags.Start) != 0 && (_previousState.Gamepad.Buttons & GamepadButtonFlags.Start) == 0)
            {
                _keyboardSimulator.SendKeyEvent(Key.Escape, true);
                if (_configManager.DebugOutput) _logger.LogInfo("Start pressed - Escape");
            }
            else if ((state.Gamepad.Buttons & GamepadButtonFlags.Start) == 0 && (_previousState.Gamepad.Buttons & GamepadButtonFlags.Start) != 0)
            {
                _keyboardSimulator.SendKeyEvent(Key.Escape, false);
            }

            // Back Button - Tab
            if ((state.Gamepad.Buttons & GamepadButtonFlags.Back) != 0 && (_previousState.Gamepad.Buttons & GamepadButtonFlags.Back) == 0)
            {
                if (_configManager.EnableTabToggle)
                {
                    _keyboardSimulator.ToggleTab();
                    if (_configManager.DebugOutput) _logger.LogInfo($"Back pressed - Tab {(_keyboardSimulator.IsTabToggled ? "ON" : "OFF")}");
                }
                else
                {
                    _keyboardSimulator.SendKeyEvent(Key.Tab, true);
                    if (_configManager.DebugOutput) _logger.LogInfo("Back pressed - Tab");
                }
            }
            else if ((state.Gamepad.Buttons & GamepadButtonFlags.Back) == 0 && (_previousState.Gamepad.Buttons & GamepadButtonFlags.Back) != 0)
            {
                if (!_configManager.EnableTabToggle)
                {
                    _keyboardSimulator.SendKeyEvent(Key.Tab, false);
                }
            }
        }

        private void ProcessJoystickInputs(State state)
        {
            // Process left joystick for WASD movement
            _joystickZoneHandler.ProcessLeftJoystick(state.Gamepad.LeftThumbX, state.Gamepad.LeftThumbY);

            // Process right joystick for mouse movement
            _mouseSimulator.MoveMouse(state.Gamepad.RightThumbX, state.Gamepad.RightThumbY);
        }

        private void ProcessTriggerInputs(State state)
        {
            // Right Trigger - Left Mouse Button
            if (state.Gamepad.RightTrigger > 128 && _previousState.Gamepad.RightTrigger <= 128)
            {
                _mouseSimulator.LeftMouseDown();
                if (_configManager.DebugOutput) _logger.LogInfo("RT pressed - Left Mouse Down");
            }
            else if (state.Gamepad.RightTrigger <= 128 && _previousState.Gamepad.RightTrigger > 128)
            {
                _mouseSimulator.LeftMouseUp();
            }

            // Left Trigger - Right Mouse Button
            if (state.Gamepad.LeftTrigger > 128 && _previousState.Gamepad.LeftTrigger <= 128)
            {
                _mouseSimulator.RightMouseDown();
                if (_configManager.DebugOutput) _logger.LogInfo("LT pressed - Right Mouse Down");
            }
            else if (state.Gamepad.LeftTrigger <= 128 && _previousState.Gamepad.LeftTrigger > 128)
            {
                _mouseSimulator.RightMouseUp();
            }
        }

        public void ReleaseAllKeys()
        {
            _keyboardSimulator.ReleaseAllKeys();
            _mouseSimulator.ReleaseAllButtons();
        }
    }
}
