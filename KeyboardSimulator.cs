using System;
using BepInEx.Logging;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace ControllerSupport
{
    public class KeyboardSimulator
    {
        private readonly ManualLogSource _logger;
        private readonly ConfigManager _configManager;
        private bool _wPressed, _aPressed, _sPressed, _dPressed, _shiftPressed, _ctrlToggled, _tabToggled;

        public bool IsWPressed => _wPressed;
        public bool IsAPressed => _aPressed;
        public bool IsSPressed => _sPressed;
        public bool IsDPressed => _dPressed;
        public bool IsShiftPressed => _shiftPressed;
        public bool IsCtrlToggled => _ctrlToggled;
        public bool IsTabToggled => _tabToggled;

        public KeyboardSimulator(ManualLogSource logger, ConfigManager configManager)
        {
            _logger = logger;
            _configManager = configManager;
        }

        public void ToggleCtrl()
        {
            _ctrlToggled = !_ctrlToggled;
            SendKeyEvent(Key.LeftCtrl, _ctrlToggled);
        }

        public void ToggleTab()
        {
            _tabToggled = !_tabToggled;
            SendKeyEvent(Key.Tab, _tabToggled);
        }

        public void ToggleShift()
        {
            _shiftPressed = !_shiftPressed;
            SendKeyEvent(Key.LeftShift, _shiftPressed);
        }

        public void SendKeyEvent(Key key, bool isPressed)
        {
            if (Keyboard.current == null)
                return;

            // Update internal state
            UpdateKeyState(key, isPressed);

            // Create a complete keyboard state with all currently pressed keys
            var keyboardState = new KeyboardState();
            if (_wPressed) keyboardState.Set(Key.W, true);
            if (_aPressed) keyboardState.Set(Key.A, true);
            if (_sPressed) keyboardState.Set(Key.S, true);
            if (_dPressed) keyboardState.Set(Key.D, true);
            if (_shiftPressed) keyboardState.Set(Key.LeftShift, true);
            if (_ctrlToggled) keyboardState.Set(Key.LeftCtrl, true);
            if (_tabToggled) keyboardState.Set(Key.Tab, true);

            // Set the specific key that triggered this event
            keyboardState.Set(key, isPressed);

            // Queue the complete state
            InputSystem.QueueStateEvent(Keyboard.current, keyboardState);

            if (_configManager.DebugOutput)
                _logger.LogInfo($"Key {key} {(isPressed ? "pressed" : "released")}");
        }

        private void UpdateKeyState(Key key, bool isPressed)
        {
            switch (key)
            {
                case Key.W: _wPressed = isPressed; break;
                case Key.A: _aPressed = isPressed; break;
                case Key.S: _sPressed = isPressed; break;
                case Key.D: _dPressed = isPressed; break;
                case Key.LeftShift: _shiftPressed = isPressed; break;
                case Key.LeftCtrl: _ctrlToggled = isPressed; break;
                case Key.Tab: _tabToggled = isPressed; break;
            }
        }

        public void ReleaseAllKeys()
        {
            try
            {
                if (Keyboard.current == null)
                    return;

                if (_wPressed) SendKeyEvent(Key.W, false);
                if (_aPressed) SendKeyEvent(Key.A, false);
                if (_sPressed) SendKeyEvent(Key.S, false);
                if (_dPressed) SendKeyEvent(Key.D, false);
                if (_shiftPressed) SendKeyEvent(Key.LeftShift, false);
                if (_ctrlToggled) SendKeyEvent(Key.LeftCtrl, false);
                if (_tabToggled) SendKeyEvent(Key.Tab, false);
                SendKeyEvent(Key.Space, false);
                SendKeyEvent(Key.E, false);
                SendKeyEvent(Key.Q, false);
                SendKeyEvent(Key.Digit1, false);
                SendKeyEvent(Key.Digit2, false);
                SendKeyEvent(Key.Digit3, false);
                SendKeyEvent(Key.Escape, false);
                SendKeyEvent(Key.Tab, false);
            }
            catch (ArgumentNullException ex)
            {
                // Log the error without using Debug.LogError since it's not available in BepInEx
                if (_configManager.DebugOutput)
                    _logger.LogError($"Error releasing keys: {ex.Message}");
            }
        }
    }
}
