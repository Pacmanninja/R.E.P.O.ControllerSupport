using BepInEx.Configuration;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace ControllerSupport
{
    public class ConfigManager
    {
        private ConfigFile _config;

        // Mouse Controls
        private ConfigEntry<float> _scrollSpeedConfig;
        public float MouseSensitivity { get; private set; }
        public float ScrollSpeed { get; private set; }

        // Movement Controls
        private ConfigEntry<float> _deadZoneRadiusConfig;
        private ConfigEntry<float> _diagonalZoneSizeConfig;
        public float DeadZoneRadius { get; private set; }
        public float DiagonalZoneSize { get; private set; }

        // Toggle Controls
        public bool EnableTabToggle { get; private set; }
        public bool EnableCtrlToggle { get; private set; }
        public bool EnableShiftToggle { get; private set; }

        // Key Interactions
        public bool ReleaseTabOnShift { get; private set; }
        public bool ReleaseCtrlOnShift { get; private set; }
        public bool ReleaseCtrlOnA { get; private set; }
        public bool AutoReleaseShiftAfterWASD { get; private set; }
        public float ShiftReleaseDelay { get; private set; }

        // Debug Options
        public bool DebugOutput { get; private set; }
        public bool DevMode { get; private set; }

        // Dev tool properties
        public float CurrentJoystickX { get; set; }
        public float CurrentJoystickY { get; set; }
        public float CurrentJoystickAngle { get; set; }

        public ConfigManager(ConfigFile config)
        {
            _config = config;
            string configPath = config.ConfigFilePath;

            // Mouse Controls
            MouseSensitivity = config.Bind("Mouse Controls", "MouseSensitivity", 10.0f,
                "Sensitivity of the right stick for mouse movement. Higher values make the mouse move faster.").Value;
            _scrollSpeedConfig = config.Bind("Mouse Controls", "ScrollSpeed", 0.0021f,
                "Adjust the scroll speed for bumper buttons (RB/LB). Lower values result in slower scrolling, higher values in faster scrolling.");
            ScrollSpeed = _scrollSpeedConfig.Value;

            // Movement Controls
            _deadZoneRadiusConfig = config.Bind("Movement Controls", "DeadZoneRadius", 0.25f,
                "Radius of the center dead zone for the left joystick (0.0 to 1.0). Higher values require more joystick movement before keys are pressed.");
            _diagonalZoneSizeConfig = config.Bind("Movement Controls", "DiagonalZoneSize", 0.7071f,
                "Size of the diagonal zones for the left joystick (0.5 to 1.0). Higher values make diagonal movement more likely.");
            DeadZoneRadius = _deadZoneRadiusConfig.Value;
            DiagonalZoneSize = _diagonalZoneSizeConfig.Value;

            // Toggle Controls
            EnableTabToggle = config.Bind("Toggle Controls", "EnableMapToggle", true,
                "Enable toggle functionality for the Map key (Back button).").Value;
            EnableCtrlToggle = config.Bind("Toggle Controls", "EnableCrouchToggle", true,
                "Enable toggle functionality for the Crouch key (B button).").Value;
            EnableShiftToggle = config.Bind("Toggle Controls", "EnableRunToggle", false,
                "Enable toggle functionality for the Run key (L3 button).").Value;

            // Key Interactions
            ReleaseTabOnShift = config.Bind("Key Interactions", "ReleaseMapOnRun", true,
                "When enabled, pressing Run will automatically release Map if active.").Value;
            ReleaseCtrlOnShift = config.Bind("Key Interactions", "ReleaseCrouchOnRun", true,
                "When enabled, activating Run will automatically release Crouch.").Value;
            ReleaseCtrlOnA = config.Bind("Key Interactions", "ReleaseCrouchOnJump", true,
                "When enabled, pressing Jump will automatically release Crouch.").Value;
            AutoReleaseShiftAfterWASD = config.Bind("Key Interactions", "AutoReleaseRunAfterWASD", true,
                "Automatically release Run after WASD inactivity.").Value;
            ShiftReleaseDelay = config.Bind("Key Interactions", "RunReleaseDelay", 0.5f,
                "Delay before auto-releasing Run (seconds).").Value;

            // Debug Options
            DebugOutput = config.Bind("Debug", "EnableDebugOutput", false,
                "Enable detailed debug logging.").Value;
            DevMode = config.Bind("Debug", "DevMode", false,
                "Enable developer tools with numpad controls.").Value;

            // Custom comment injection with hex protection
            ApplyCustomComments(configPath);
        }

        private void ApplyCustomComments(string configPath)
        {
            try
            {
                if (!File.Exists(configPath)) return;

                var existingLines = File.ReadAllLines(configPath);
                if (existingLines.Any(l => l.Contains("Nexus") || l.Contains("repo"))) return;

                var newLines = new List<string>();
                int commentLineCount = 0;

                foreach (var line in existingLines)
                {
                    if (line.StartsWith("##"))
                    {
                        newLines.Add(line);
                        commentLineCount++;
                    }
                    else break;
                }

                // Hex-encoded custom comments
                newLines.AddRange(new[]
                {
                    Decode("2323204E6578757320646F776E6C6F61643A2068747470733A2F2F7777772E6E657875736D6F64732E636F6D2F7265706F2F6D6F64732F3439"),
                    Decode("2323205468756E64657273746F726520646F776E6C6F61643A2068747470733A2F2F7468756E64657273746F72652E696F2F632F7265706F2F702F5061636D616E6E696E6A613939382F436F6E74726F6C6C65725F537570706F72742F"),
                    Decode("2323207265706F3A2068747470733A2F2F6769746875622E636F6D2F5061636D616E6E696E6A612F522E452E502E4F2E436F6E74726F6C6C6572537570706F7274")
                });

                newLines.AddRange(existingLines.Skip(commentLineCount));
                File.WriteAllLines(configPath, newLines.ToArray());
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Trace.WriteLine($"Custom comment injection failed: {ex.Message}");
            }
        }

        private static string Decode(string hexInput)
        {
            var bytes = new byte[hexInput.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hexInput.Substring(i * 2, 2), 16);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        public float IncreaseScrollSpeed()
        {
            ScrollSpeed += 0.0001f;
            _scrollSpeedConfig.Value = ScrollSpeed;
            return ScrollSpeed;
        }

        public float DecreaseScrollSpeed()
        {
            ScrollSpeed -= 0.0001f;
            if (ScrollSpeed < 0.0001f) ScrollSpeed = 0.0001f;
            _scrollSpeedConfig.Value = ScrollSpeed;
            return ScrollSpeed;
        }

        public float UpdateDeadZoneRadius(float magnitude)
        {
            DeadZoneRadius = magnitude;
            _deadZoneRadiusConfig.Value = DeadZoneRadius;
            return DeadZoneRadius;
        }

        public float UpdateDiagonalZoneSize(float angle)
        {
            DiagonalZoneSize = angle;
            _diagonalZoneSizeConfig.Value = DiagonalZoneSize;
            return DiagonalZoneSize;
        }
    }
}
