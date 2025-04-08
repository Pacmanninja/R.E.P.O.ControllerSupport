using BepInEx.Logging;
using System;
using System.Runtime.InteropServices;

namespace ControllerSupport
{
    public class MouseSimulator
    {
        private readonly ManualLogSource _logger;
        private readonly ConfigManager _configManager;
        private float _scrollAccumulator = 0f;

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [Flags]
        private enum MouseEventFlags
        {
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            Wheel = 0x0800,
            Move = 0x0001
        }

        public MouseSimulator(ManualLogSource logger, ConfigManager configManager)
        {
            _logger = logger;
            _configManager = configManager;
        }

        public void MoveMouse(short x, short y)
        {
            float normalizedX = x / 32768f;
            float normalizedY = y / 32768f;

            if (Math.Abs(normalizedX) > _configManager.DeadZoneRadius || Math.Abs(normalizedY) > _configManager.DeadZoneRadius)
            {
                int dx = (int)(normalizedX * _configManager.MouseSensitivity);
                int dy = (int)(-normalizedY * _configManager.MouseSensitivity);
                mouse_event((int)MouseEventFlags.Move, dx, dy, 0, 0);

                if (_configManager.DebugOutput)
                    _logger.LogInfo($"Mouse move: {dx}, {dy}");
            }
        }

        public void LeftMouseDown()
        {
            mouse_event((int)MouseEventFlags.LeftDown, 0, 0, 0, 0);
        }

        public void LeftMouseUp()
        {
            mouse_event((int)MouseEventFlags.LeftUp, 0, 0, 0, 0);
        }

        public void RightMouseDown()
        {
            mouse_event((int)MouseEventFlags.RightDown, 0, 0, 0, 0);
        }

        public void RightMouseUp()
        {
            mouse_event((int)MouseEventFlags.RightUp, 0, 0, 0, 0);
        }

        public void ProcessTriggers(byte leftTrigger, byte rightTrigger)
        {
            if (rightTrigger > 128)
                mouse_event((int)MouseEventFlags.LeftDown, 0, 0, 0, 0);
            else
                mouse_event((int)MouseEventFlags.LeftUp, 0, 0, 0, 0);

            if (leftTrigger > 128)
                mouse_event((int)MouseEventFlags.RightDown, 0, 0, 0, 0);
            else
                mouse_event((int)MouseEventFlags.RightUp, 0, 0, 0, 0);
        }

        public void Scroll(float amount)
        {
            _scrollAccumulator += amount;

            // Only trigger a scroll event when we've accumulated enough
            if (Math.Abs(_scrollAccumulator) >= 0.0084f)
            {
                // Use the minimum threshold value that works
                float scrollValue = Math.Sign(_scrollAccumulator) * 0.0084f;
                int scrollAmount = (int)(scrollValue * 120);

                mouse_event((int)MouseEventFlags.Wheel, 0, 0, scrollAmount, 0);

                // Reduce the accumulator but don't reset completely
                // This preserves any remainder for the next frame
                _scrollAccumulator -= scrollValue;
            }
        }

        public void ReleaseAllButtons()
        {
            mouse_event((int)MouseEventFlags.LeftUp, 0, 0, 0, 0);
            mouse_event((int)MouseEventFlags.RightUp, 0, 0, 0, 0);
        }
    }
}
