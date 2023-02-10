using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using SteamController.Helpers;

namespace SteamController.Managers
{
    public sealed class SteamManager : Manager
    {
        private static readonly Classifier[] Classifiers = new Classifier[]
        {
            new Classifier() { Type = "gamepadui", ClassName = "SDL_app", WindowText = "SP", ProcessName = "steamwebhelper" },
            new Classifier() { Type = "possiblegamepadui", ClassName = "SDL_app", WindowTextPrefix = "SP", ProcessName = "steamwebhelper" },

            // Support Steam client released around 2023-01-20, version: 1674182294
            new Classifier() { Type = "gamepadui_2023_01_20", ClassName = "SDL_app", ProcessName = "steamwebhelper" },
            new Classifier() { Type = "controllerui_2023_01_20", ClassName = "CUIEngineWin32", ProcessName = "steam" },
            // new Classifier() { Type = "possiblegamepadui_2023_01_20", ClassName = "SDL_app", WindowTextSuffix = "Controller Layout", ProcessName = "steamwebhelper" },
        };

        public const int DebounceStates = 1;

        private string? lastState;
        private int stateChanged;

        public override void Tick(Context context)
        {
            if (Settings.Default.EnableSteamDetection != true)
            {
                context.State.SteamUsesSteamInput = false;
                context.State.SteamUsesX360Controller = false;
                context.State.SteamUsesDS4Controller = false;
                lastState = null;
                stateChanged = 0;
                return;
            }

            var usesController = UsesController();
            if (lastState == usesController)
            {
                stateChanged = 0;
                return;
            }
            else if (stateChanged < DebounceStates)
            {
                stateChanged++;
                return;
            }

            if (usesController is not null)
            {
                context.State.SteamUsesSteamInput = Helpers.SteamConfiguration.IsControllerBlacklisted(
                    Devices.SteamController.VendorID,
                    Devices.SteamController.ProductID
                ) != true;

                context.State.SteamUsesX360Controller = Helpers.SteamConfiguration.IsControllerBlacklisted(
                    Devices.Xbox360Controller.VendorID,
                    Devices.Xbox360Controller.ProductID
                ) != true;

                context.State.SteamUsesDS4Controller = Helpers.SteamConfiguration.IsControllerBlacklisted(
                    Devices.DS4Controller.VendorID,
                    Devices.DS4Controller.ProductID
                ) != true;
            }
            else
            {
                context.State.SteamUsesSteamInput = false;
                context.State.SteamUsesX360Controller = false;
                context.State.SteamUsesDS4Controller = false;
            }

            lastState = usesController;
            stateChanged = 0;

#if DEBUG
            CommonHelpers.Log.TraceLine(
                "SteamManager: uses={0}, isRunning={1}, usesSteamInput={2}, usesX360={3}, usesDS4={4}",
                usesController,
                SteamConfiguration.IsRunning,
                context.State.SteamUsesSteamInput,
                context.State.SteamUsesX360Controller,
                context.State.SteamUsesDS4Controller
            );
#endif
        }

        private string? UsesController()
        {
            if (!SteamConfiguration.IsRunning)
                return null;
            if (SteamConfiguration.IsBigPictureMode.GetValueOrDefault(false))
                return "bigpicture";
            if (SteamConfiguration.IsRunningGame.GetValueOrDefault(false))
                return "game";
            return ClassifyForegroundProcess();
        }

        private string? ClassifyForegroundProcess()
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero)
                return null;

            StringBuilder classNameBuilder = new StringBuilder(256);
            if (GetClassName(hWnd, classNameBuilder, classNameBuilder.Capacity) == 0)
                return null;
            var className = classNameBuilder.ToString();

            StringBuilder windowTextBuilder = new StringBuilder(256);
            if (GetWindowText(hWnd, windowTextBuilder, windowTextBuilder.Capacity) == 0)
                return null;
            var windowText = windowTextBuilder.ToString();

            var processName = ForegroundProcess.Find(hWnd)?.ProcessName;
            if (processName is null)
                return null;

            foreach (var classifier in Classifiers)
            {
                if (classifier.Match(className, windowText, processName))
                    return classifier.Type;
            }

            return null;
        }

        private struct Classifier
        {
            public String Type { get; set; }
            public String? ClassName { get; set; }
            public String? WindowText { get; set; }
            public String? WindowTextPrefix { get; set; }
            public String? WindowTextSuffix { get; set; }
            public Regex? WindowTextRegex { get; set; }
            public String? ProcessName { get; set; }

            public bool Match(string className, string windowText, string processName)
            {
                if (ClassName is not null && className != ClassName)
                    return false;
                if (WindowText is not null && windowText != WindowText)
                    return false;
                if (WindowTextRegex is not null && WindowTextRegex.IsMatch(windowText))
                    return false;
                if (WindowTextPrefix is not null && !windowText.StartsWith(WindowTextPrefix))
                    return false;
                if (WindowTextSuffix is not null && !windowText.EndsWith(WindowTextSuffix))
                    return false;
                if (ProcessName is not null && !processName.EndsWith(ProcessName))
                    return false;
                return true;
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
    }
}
