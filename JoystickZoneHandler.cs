using System;
using BepInEx.Logging;
using UnityEngine.InputSystem;

namespace ControllerSupport
{
    public class JoystickZoneHandler
    {
        private readonly ManualLogSource _logger;
        private readonly ConfigManager _configManager;
        private readonly KeyboardSimulator _keyboardSimulator;

        // Store last joystick state to detect changes
        private float _lastX = 0f;
        private float _lastY = 0f;
        private bool _lastWPressed = false;
        private bool _lastAPressed = false;
        private bool _lastSPressed = false;
        private bool _lastDPressed = false;

        public JoystickZoneHandler(ManualLogSource logger, ConfigManager configManager, KeyboardSimulator keyboardSimulator)
        {
            _logger = logger;
            _configManager = configManager;
            _keyboardSimulator = keyboardSimulator;
        }

        public void ProcessLeftJoystick(short x, short y)
        {
            float normalizedX = x / 32768f;
            float normalizedY = y / 32768f;

            // Store current joystick position for dev tools
            _configManager.CurrentJoystickX = normalizedX;
            _configManager.CurrentJoystickY = normalizedY;

            // Only log if position changed significantly
            bool positionChanged = Math.Abs(normalizedX - _lastX) > 0.01f || Math.Abs(normalizedY - _lastY) > 0.01f;
            _lastX = normalizedX;
            _lastY = normalizedY;

            float magnitude = (float)Math.Sqrt(normalizedX * normalizedX + normalizedY * normalizedY);

            bool shouldPressW = false;
            bool shouldPressA = false;
            bool shouldPressS = false;
            bool shouldPressD = false;

            if (magnitude < _configManager.DeadZoneRadius)
            {
                // In dead zone, all keys should be released
            }
            else
            {
                float angle = (float)Math.Atan2(normalizedY, normalizedX);
                float degrees = angle * 180f / (float)Math.PI;
                if (degrees < 0) degrees += 360f;

                // Store current angle for dev tools
                _configManager.CurrentJoystickAngle = degrees;

                // North (W)
                if (degrees > 45 && degrees < 135)
                {
                    shouldPressW = true;
                }
                // South (S)
                else if (degrees > 225 && degrees < 315)
                {
                    shouldPressS = true;
                }
                // West (A)
                else if (degrees > 135 && degrees < 225)
                {
                    shouldPressA = true;
                }
                // East (D)
                else if (degrees < 45 || degrees > 315)
                {
                    shouldPressD = true;
                }

                // Check for diagonals - adjust these based on your needs
                if (degrees > 45 && degrees < 75)
                {
                    // Northeast (WD)
                    shouldPressW = true;
                    shouldPressD = true;
                }
                else if (degrees > 105 && degrees < 135)
                {
                    // Northwest (WA)
                    shouldPressW = true;
                    shouldPressA = true;
                }
                else if (degrees > 225 && degrees < 255)
                {
                    // Southwest (SA)
                    shouldPressS = true;
                    shouldPressA = true;
                }
                else if (degrees > 285 && degrees < 315)
                {
                    // Southeast (SD)
                    shouldPressS = true;
                    shouldPressD = true;
                }

                if (positionChanged && _configManager.DebugOutput)
                {
                    _logger.LogInfo($"Joystick: X={normalizedX:F2}, Y={normalizedY:F2}, Angle={degrees:F1}, Magnitude={magnitude:F2}");
                }
            }

            // Only send key events and log when key states change
            if (shouldPressW != _lastWPressed)
            {
                _keyboardSimulator.SendKeyEvent(Key.W, shouldPressW);
                _lastWPressed = shouldPressW;
                if (_configManager.DebugOutput) _logger.LogInfo($"W key {(shouldPressW ? "pressed" : "released")}");
            }

            if (shouldPressA != _lastAPressed)
            {
                _keyboardSimulator.SendKeyEvent(Key.A, shouldPressA);
                _lastAPressed = shouldPressA;
                if (_configManager.DebugOutput) _logger.LogInfo($"A key {(shouldPressA ? "pressed" : "released")}");
            }

            if (shouldPressS != _lastSPressed)
            {
                _keyboardSimulator.SendKeyEvent(Key.S, shouldPressS);
                _lastSPressed = shouldPressS;
                if (_configManager.DebugOutput) _logger.LogInfo($"S key {(shouldPressS ? "pressed" : "released")}");
            }

            if (shouldPressD != _lastDPressed)
            {
                _keyboardSimulator.SendKeyEvent(Key.D, shouldPressD);
                _lastDPressed = shouldPressD;
                if (_configManager.DebugOutput) _logger.LogInfo($"D key {(shouldPressD ? "pressed" : "released")}");
            }
        }

        public void ReleaseAllDirectionKeys()
        {
            if (Keyboard.current == null)
                return;

            if (_lastWPressed) _keyboardSimulator.SendKeyEvent(Key.W, false);
            if (_lastAPressed) _keyboardSimulator.SendKeyEvent(Key.A, false);
            if (_lastSPressed) _keyboardSimulator.SendKeyEvent(Key.S, false);
            if (_lastDPressed) _keyboardSimulator.SendKeyEvent(Key.D, false);

            _lastWPressed = false;
            _lastAPressed = false;
            _lastSPressed = false;
            _lastDPressed = false;
        }
    }
}
