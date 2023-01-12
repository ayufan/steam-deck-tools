using CommonHelpers;

namespace PerformanceOverlay
{
    internal sealed class Settings : BaseSettings
    {
        public static readonly Settings Default = new Settings();

        public Settings() : base("Settings")
        {
            TouchSettings = true;
        }

        public OverlayMode OSDMode
        {
            get { return Get<OverlayMode>("OSDMode", OverlayMode.FPS); }
            set { Set("OSDMode", value); }
        }

        public string ShowOSDShortcut
        {
            get { return Get<string>("ShowOSDShortcut", "Shift+F11"); }
            set { Set("ShowOSDShortcut", value); }
        }

        public string CycleOSDShortcut
        {
            get { return Get<string>("CycleOSDShortcut", "Alt+Shift+F11"); }
            set { Set("CycleOSDShortcut", value); }
        }

        public bool ShowOSD
        {
            get { return Get<bool>("ShowOSD", true); }
            set { Set("ShowOSD", value); }
        }

        public bool EnableFullOnPowerControl
        {
            get { return Get<bool>("EnableFullOnPowerControl", false); }
            set { Set("EnableFullOnPowerControl", value); }
        }

        public bool EnableKernelDrivers
        {
            get { return Get<bool>("EnableKernelDrivers", false); }
            set { Set("EnableKernelDrivers", value); }
        }

        public bool EnableExperimentalFeatures
        {
            get { return Instance.IsDEBUG; }
        }
    }
}
