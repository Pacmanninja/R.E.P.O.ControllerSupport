using BepInEx.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace ControllerSupport
{
    public class ConfigManager
    {
        private ConfigFile _config;
        private ConfigEntry<float> _scrollSpeedConfig;
        private ConfigEntry<float> _deadZoneRadiusConfig;
        private ConfigEntry<float> _diagonalZoneSizeConfig;

        public bool EnableTabToggle { get; private set; }
        public bool EnableCtrlToggle { get; private set; }
        public bool EnableShiftToggle { get; private set; }
        public float ScrollSpeed { get; private set; }
        public float DeadZoneRadius { get; private set; }
        public float DiagonalZoneSize { get; private set; }
        public float MouseSensitivity { get; private set; }
        public bool DebugOutput { get; private set; }
        public bool DevMode { get; private set; }

        // Key interaction settings
        public bool ReleaseCtrlOnShift { get; private set; }
        public bool ReleaseTabOnShift { get; private set; }
        public bool ReleaseCtrlOnA { get; private set; }
        public bool AutoReleaseShiftAfterWASD { get; private set; }
        public float ShiftReleaseDelay { get; private set; }

        // Current joystick state for dev tools
        public float CurrentJoystickX { get; set; }
        public float CurrentJoystickY { get; set; }
        public float CurrentJoystickAngle { get; set; }

        public ConfigManager(ConfigFile config)
        {
            _config = config;
            string configPath = config.ConfigFilePath;

            // First bind the configuration entries so the file is created
            // Toggle Controls
            EnableTabToggle = config.Bind("Toggle Controls", "EnableMapToggle", true,
                "Enable toggle functionality for the Map key (Back button). When true, pressing Back will toggle Map on/off instead of requiring you to hold the button.").Value;

            EnableCtrlToggle = config.Bind("Toggle Controls", "EnableCrouchToggle", true,
                "Enable toggle functionality for the Crouch key (B button). When true, pressing B will toggle Crouch on/off instead of requiring you to hold the button.").Value;

            EnableShiftToggle = config.Bind("Toggle Controls", "EnableRunToggle", false,
                "Enable toggle functionality for the Run key (L3 button). When true, pressing L3 will toggle Run on/off instead of requiring you to hold the button.").Value;

            // Key Interactions
            ReleaseCtrlOnShift = config.Bind("Key Interactions", "ReleaseCrouchOnRun", true,
                "When enabled, activating Run will automatically release Crouch if it's toggled on.").Value;

            ReleaseTabOnShift = config.Bind("Key Interactions", "ReleaseMapOnRun", true,
                "When enabled, pressing Run will automatically release Map if it's active.").Value;

            ReleaseCtrlOnA = config.Bind("Key Interactions", "ReleaseCrouchOnJump", true,
                "When enabled, pressing Jump will automatically release Crouch if it's toggled on.").Value;

            AutoReleaseShiftAfterWASD = config.Bind("Key Interactions", "AutoReleaseRunAfterWASD", true,
                "When enabled, Run will be automatically released after WASD keys are inactive for the specified delay.").Value;

            ShiftReleaseDelay = config.Bind("Key Interactions", "RunReleaseDelay", 0.5f,
                "Delay in seconds before automatically releasing Run after WASD keys become inactive.").Value;

            // Movement Controls
            _deadZoneRadiusConfig = config.Bind("Movement Controls", "DeadZoneRadius", 0.25f,
                "Radius of the center dead zone for the left joystick (0.0 to 1.0). Higher values require more joystick movement before keys are pressed.");

            _diagonalZoneSizeConfig = config.Bind("Movement Controls", "DiagonalZoneSize", 0.7071f,
                "Size of the diagonal zones for the left joystick (0.5 to 1.0). Higher values make diagonal movement more likely.");

            // Mouse Controls
            MouseSensitivity = config.Bind("Mouse Controls", "MouseSensitivity", 10.0f,
                "Sensitivity of the right stick for mouse movement. Higher values make the mouse move faster.").Value;

            _scrollSpeedConfig = config.Bind("Mouse Controls", "ScrollSpeed", 0.0021f,
                "Adjust the scroll speed for bumper buttons (RB/LB). Lower values result in slower scrolling, higher values in faster scrolling.");

            // Debug Options
            DebugOutput = config.Bind("Debug", "EnableDebugOutput", false,
                "Enable detailed debug output in the console. Set to false to reduce console spam.").Value;

            DevMode = config.Bind("Debug", "DevMode", false,
                "Enable development mode with runtime configuration adjustment using numpad keys. Numpad+/- adjusts scroll speed, Numpad7 sets dead zone, Numpad4 sets diagonal zone.").Value;

            // Initialize properties from config entries
            ScrollSpeed = _scrollSpeedConfig.Value;
            DeadZoneRadius = _deadZoneRadiusConfig.Value;
            DiagonalZoneSize = _diagonalZoneSizeConfig.Value;

            // Now the file should exist, check if we need to add our custom comments
            if (File.Exists(configPath))
            {
                string[] existingLines = File.ReadAllLines(configPath);
                bool hasCustomComments = existingLines.Any(line => line.Contains("Nexus download:") || line.Contains("repo:"));

                if (!hasCustomComments)
                {
                    List<string> newLines = new List<string>();

                    // Add BepInEx's auto-generated comments (typically first two lines)
                    int commentLineCount = 0;
                    foreach (string line in existingLines)
                    {
                        if (line.StartsWith("##"))
                        {
                            newLines.Add(line);
                            commentLineCount++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    // Add our custom comments with character-by-character concatenation
                    newLines.Add("#" + "#" + " " + "N" + "e" + "x" + "u" + "s" + " " + "d" + "o" + "w" + "n" + "l" + "o" + "a" + "d" + ":" + " " + "h" + "t" + "t" + "p" + "s" + ":" + "/" + "/" + "w" + "w" + "w" + "." + "n" + "e" + "x" + "u" + "s" + "m" + "o" + "d" + "s" + "." + "c" + "o" + "m" + "/" + "r" + "e" + "p" + "o" + "/" + "m" + "o" + "d" + "s" + "/" + "4" + "9");
                    newLines.Add("#" + "#" + " " + "T" + "h" + "u" + "n" + "d" + "e" + "r" + "s" + "t" + "o" + "r" + "e" + " " + "d" + "o" + "w" + "n" + "l" + "o" + "a" + "d" + ":" + " " + "h" + "t" + "t" + "p" + "s" + ":" + "/" + "/" + "t" + "h" + "u" + "n" + "d" + "e" + "r" + "s" + "t" + "o" + "r" + "e" + "." + "i" + "o" + "/" + "c" + "/" + "r" + "e" + "p" + "o" + "/" + "p" + "/" + "P" + "a" + "c" + "m" + "a" + "n" + "n" + "i" + "n" + "j" + "a" + "9" + "9" + "8" + "/" + "C" + "o" + "n" + "t" + "r" + "o" + "l" + "l" + "e" + "r" + "_" + "S" + "u" + "p" + "p" + "o" + "r" + "t" + "/");
                    newLines.Add("#" + "#" + " " + "r" + "e" + "p" + "o" + ":" + " " + "h" + "t" + "t" + "p" + "s" + ":" + "/" + "/" + "g" + "i" + "t" + "h" + "u" + "b" + "." + "c" + "o" + "m" + "/" + "P" + "a" + "c" + "m" + "a" + "n" + "n" + "i" + "n" + "j" + "a" + "/" + "R" + "." + "E" + "." + "P" + "." + "O" + "." + "C" + "o" + "n" + "t" + "r" + "o" + "l" + "l" + "e" + "r" + "S" + "u" + "p" + "p" + "o" + "r" + "t");

                    // Add the rest of the config file
                    for (int i = commentLineCount; i < existingLines.Length; i++)
                    {
                        newLines.Add(existingLines[i]);
                    }

                    File.WriteAllLines(configPath, newLines.ToArray());
                }
            }
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
