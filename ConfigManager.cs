using BepInEx.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

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

            // Inject About section if needed
            InjectAboutSection(configPath);
        }

        private void InjectAboutSection(string configPath)
        {
            // Hex-encoded lines for the About section
            string[] aboutSectionHex = new[]
            {
                "5b41626f75745d", // [About]
                "2323204e65787573204d6f6473204c696e6b2e", // ## Nexus Mods Link.
                "232053657474696e6720747970653a20537472696e67", // # Setting type: String
                "232044656661756c742076616c75653a2068747470733a2f2f7777772e6e657875736d6f64732e636f6d2f7265706f2f6d6f64732f3439", // # Default value: https://www.nexusmods.com/repo/mods/49
                "4e65787573203d2068747470733a2f2f7777772e6e657875736d6f64732e636f6d2f7265706f2f6d6f64732f3439", // Nexus = https://www.nexusmods.com/repo/mods/49
                "",
                "2323205468756e64657273746f7265204c696e6b2e", // ## Thunderstore Link.
                "232053657474696e6720747970653a20537472696e67", // # Setting type: String
                "232044656661756c742076616c75653a2068747470733a2f2f7468756e64657273746f72652e696f2f632f7265706f2f702f5061636d616e6e696e6a613939382f436f6e74726f6c6c65725f537570706f72742f", // # Default value: https://thunderstore.io/c/repo/p/Pacmanninja998/Controller_Support/
                "5468756e64657273746f7265203d2068747470733a2f2f7468756e64657273746f72652e696f2f632f7265706f2f702f5061636d616e6e696e6a613939382f436f6e74726f6c6c65725f537570706f72742f", // Thunderstore = https://thunderstore.io/c/repo/p/Pacmanninja998/Controller_Support/
                "",
                "232320476974687562204c696e6b2e", // ## Github Link.
                "232053657474696e6720747970653a20537472696e67", // # Setting type: String
                "232044656661756c742076616c75653a2068747470733a2f2f6769746875622e636f6d2f5061636d616e6e696e6a612f522e452e502e4f2e436f6e74726f6c6c6572537570706f7274", // # Default value: https://github.com/Pacmanninja/R.E.P.O.ControllerSupport
                "476974687562203d2068747470733a2f2f6769746875622e636f6d2f5061636d616e6e696e6a612f522e452e502e4f2e436f6e74726f6c6c6572537570706f7274", // Github = https://github.com/Pacmanninja/R.E.P.O.ControllerSupport
                ""
            };

            // Only inject if not present
            try
            {
                if (!File.Exists(configPath))
                    return;

                var lines = File.ReadAllLines(configPath).ToList();

                // Remove any existing [About] section
                int aboutStart = lines.FindIndex(l => l.Trim().Equals("[About]", StringComparison.OrdinalIgnoreCase));
                if (aboutStart != -1)
                {
                    int nextSection = lines.FindIndex(aboutStart + 1, l => l.StartsWith("[") && !l.Trim().Equals("[About]", StringComparison.OrdinalIgnoreCase));
                    if (nextSection == -1) nextSection = lines.Count;
                    lines.RemoveRange(aboutStart, nextSection - aboutStart);
                }

                // Insert decoded About section at the top (after initial ## or empty lines)
                int insertAt = 0;
                while (insertAt < lines.Count && (lines[insertAt].StartsWith("##") || string.IsNullOrWhiteSpace(lines[insertAt])))
                    insertAt++;
                lines.InsertRange(insertAt, aboutSectionHex.Select(DecodeHexLine));

                File.WriteAllLines(configPath, lines);
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debug.WriteLine("Failed to inject About section: " + ex);
            }
        }

        private static string DecodeHexLine(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return "";
            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return Encoding.UTF8.GetString(bytes);
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
