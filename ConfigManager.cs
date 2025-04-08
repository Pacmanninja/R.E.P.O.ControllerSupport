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

        // Current joystick state for dev tools
        public float CurrentJoystickX { get; set; }
        public float CurrentJoystickY { get; set; }
        public float CurrentJoystickAngle { get; set; }

        public ConfigManager(ConfigFile config)
        {
            _config = config;
            string configPath = config.ConfigFilePath;

            // First bind the configuration entries so the file is created
            EnableTabToggle = config.Bind("Controls", "EnableTabToggle", true, "Enable toggle functionality for the Tab key (Back button). When true, pressing Back will toggle Tab on/off.").Value;
            EnableCtrlToggle = config.Bind("Controls", "EnableCtrlToggle", true, "Enable toggle functionality for the Ctrl key (B button). When true, pressing B will toggle Ctrl on/off.").Value;
            EnableShiftToggle = config.Bind("Controls", "EnableShiftToggle", false, "Enable toggle functionality for the Shift key (L3 button). When true, pressing L3 will toggle Shift on/off.").Value;
            _scrollSpeedConfig = config.Bind("Controls", "ScrollSpeed", 0.0021f, "Adjust the scroll speed for bumper buttons (RB/LB). Lower values result in slower scrolling, higher values in faster scrolling.");
            _deadZoneRadiusConfig = config.Bind("Controls", "DeadZoneRadius", 0.25f, "Radius of the center dead zone for the left joystick (0.0 to 1.0). Higher values require more joystick movement before keys are pressed.");
            _diagonalZoneSizeConfig = config.Bind("Controls", "DiagonalZoneSize", 0.7071f, "Size of the diagonal zones for the left joystick (0.5 to 1.0). Higher values make diagonal movement more likely.");

            MouseSensitivity = config.Bind("Controls", "MouseSensitivity", 10.0f, "Sensitivity of the right stick for mouse movement. Higher values make the mouse move faster.").Value;
            DebugOutput = config.Bind("Debug", "EnableDebugOutput", false, "Enable detailed debug output in the console. Set to false to reduce console spam.").Value;
            DevMode = config.Bind("Debug", "DevMode", false, "Enable development mode with runtime configuration adjustment using numpad keys.").Value;

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

                    // Add our custom comments
                    newLines.Add("## Nexus download: nexus.com");
                    newLines.Add("## repo: github");

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
