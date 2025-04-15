using BepInEx;
using BepInEx.Logging;
using SharpDX.XInput;
using UnityEngine.InputSystem;

namespace ControllerSupport
{
    [BepInPlugin("pacmanninja998.nexus.ControllerSupport", "Controller Support", "1.0.0")]
    public class ControllerSupportPlugin : BaseUnityPlugin
    {
        private ManualLogSource _logger;
        private InputManager _inputManager;
        private ConfigManager _configManager;

        private void Awake()
        {
            _logger = Logger;
            _logger.LogInfo("Controller Support plugin loaded!");
            _configManager = new ConfigManager(Config);
            _inputManager = new InputManager(_logger, _configManager);
            InputSystem.onBeforeUpdate += OnInputSystemUpdate;
        }

        private void Update()
        {
            _inputManager.Update();
        }

        private void OnInputSystemUpdate()
        {
            // Move your input processing here if possible
            if (_inputManager != null)
            {
                _inputManager.Update();
            }
        }

        private void OnDestroy()
        {
            if (Keyboard.current != null && _inputManager != null)
            {
                _inputManager.ReleaseAllKeys();
            }
        }
    }
}
