using System;
using System.IO;
using BepInEx.Logging;

namespace ControllerSupport
{
    public class DllChecker
    {
        public static void CheckDlls()
        {
            string[] dllsToCheck = { "SharpDX.dll", "SharpDX.XInput.dll" };
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Print current working directory (only REPO and everything to the right)
            string shortPath = ShortenPath(currentDirectory);
            Console.WriteLine($"Current working directory: {shortPath}");

            bool allDllsFound = true;

            // Check in the current directory
            foreach (string dll in dllsToCheck)
            {
                string dllPath = Path.Combine(currentDirectory, dll);
                bool dllExists = File.Exists(dllPath);
                Console.WriteLine($"{dll}: {(dllExists ? "Found" : "Not Found")}");

                if (!dllExists)
                    allDllsFound = false;
            }

            // If any DLLs are missing, show a popup notification
            if (!allDllsFound)
            {
                string missingDlls = string.Join(", ", dllsToCheck);
                Console.WriteLine($"Required DLLs not found for Controller Support: {missingDlls}\n\nPlease ensure all required files are in the BepInEx/plugins folder.",
                    "Controller Support - Missing Files");
                Console.WriteLine("Please ensure all required files are in the BepInEx/plugins folder.");

            }
        }

        private static string ShortenPath(string path)
        {
            int repoIndex = path.IndexOf("REPO", StringComparison.OrdinalIgnoreCase);
            if (repoIndex >= 0)
                return path.Substring(repoIndex);
            return path;
        }
    }
}
