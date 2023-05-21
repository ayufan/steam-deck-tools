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
            new Classifier() { Type = "gamepadui", ClassName = "SDL_app", WindowText = "SP", ProcessName = "steamwebhelper", MaxVersion = 1674182294-1 },
            new Classifier() { Type = "possiblegamepadui", ClassName = "SDL_app", WindowTextPrefix = "SP", ProcessName = "steamwebhelper", MaxVersion = 1674182294-1 },

            // Support Steam client released around 2023-05-21, version: 1684535786
            new Classifier() { Type = "bigpicturemode_2023_05_21", ClassName = "SDL_app", ProcessName = "steamwebhelper", WindowTextSuffix = "Steam Big Picture Mode", MinVersion = 1684535786 },
            new Classifier() { Type = "controller_layout_2023_05_21", ClassName = "SDL_app", ProcessName = "steamwebhelper", WindowText = "Controller Layout", MinVersion = 1684535786 }, // Controller Calibration
            new Classifier() { Type = "desktop_guide_layout_2023_05_21", ClassName = "SDL_app", ProcessName = "steamwebhelper", WindowTextPrefix = "Steam Controller Configs -", MinVersion = 1684535786 }, // Desktop and Guide
            new Classifier() { Type = "game_layout_2023_05_21", ClassName = "SDL_app", ProcessName = "steamwebhelper", WindowTextSuffix = "- Controller Layout", MinVersion = 1684535786 }, // for Game

            // Support Steam client released around 2023-01-20, version: 1674182294
            new Classifier() { Type = "gamepadui_2023_05_21", ClassName = "SDL_app", ProcessName = "steamwebhelper", WindowText = "Steam", Forbid = true, MaxVersion = 1684535786-1 },
            new Classifier() { Type = "gamepadui_2023_05_21", ClassName = "SDL_app", ProcessName = "steamwebhelper", WindowText = "Steam Settings", Forbid = true, MaxVersion = 1684535786-1 },
            new Classifier() { Type = "gamepadui_2023_05_21", ClassName = "SDL_app", ProcessName = "steamwebhelper", WindowText = "Special Offers", Forbid = true, MaxVersion = 1684535786-1 },
            new Classifier() { Type = "gamepadui_2023_01_20", ClassName = "SDL_app", ProcessName = "steamwebhelper", MaxVersion = 1684535786-1 },
            new Classifier() { Type = "controllerui_2023_01_20", ClassName = "CUIEngineWin32", ProcessName = "steam", MaxVersion = 1684535786-1 },
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

            var steamVersion = SteamConfiguration.SteamVersion;

            foreach (var classifier in Classifiers)
            {
                if (classifier.Match(className, windowText, processName, steamVersion))
                {
                    if (classifier.Forbid)
                        return null;

                    return String.Format("{0} (className={1}, windowText={2}, processName={3}, steamVersion={4})",
                        classifier.Type, className, windowText, processName, steamVersion);
                }
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
            public uint MinVersion { get; set; }
            public uint MaxVersion { get; set; }
            public bool Forbid { get; set; }

            public bool Match(string className, string windowText, string processName, uint steamVersion)
            {
                if (ClassName is not null && className != ClassName)
                    return false;
                if (WindowText is not null && windowText != WindowText)
                    return false;
                if (WindowTextRegex is not null && !WindowTextRegex.IsMatch(windowText))
                    return false;
                if (WindowTextPrefix is not null && !windowText.StartsWith(WindowTextPrefix))
                    return false;
                if (WindowTextSuffix is not null && !windowText.EndsWith(WindowTextSuffix))
                    return false;
                if (ProcessName is not null && !processName.EndsWith(ProcessName))
                    return false;
                if (MinVersion > 0 && steamVersion < MinVersion)
                    return false;
                if (MaxVersion > 0 && steamVersion > MaxVersion)
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
