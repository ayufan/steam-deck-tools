using System.Globalization;

namespace PowerControl
{
    internal class MenuStack
    {
        public static Menu.MenuRoot Root = new Menu.MenuRoot()
        {
            Name = String.Format(
                "\r\n\r\nPower Control v{0} <C4>-<C> <TIME={1}>\r\n",
                Application.ProductVersion.ToString(),
                Is24hClock ? "%H:%M:%S" : "%I:%M:%S %p"
            ),
            Items =
            {
                Options.Profiles.Instance,
                new Menu.MenuItemSeparator(),
                Options.Brightness.Instance,
                Options.Volume.Instance,
                new Menu.MenuItemSeparator(),
                Options.Resolution.Instance,
                Options.RefreshRate.Instance,
                Options.FPSLimit.Instance,
                Options.GPUScalingItem.Instance,
                #if DEBUG
                Options.Sharpening.Instance,
                #endif
                Options.GPUColors.Instance,
                new Menu.MenuItemSeparator(),
                Options.TDP.Instance,
                Options.GPUFrequency.Instance,
                Options.CPUFrequency.Instance,
                Options.SMT.Instance,
                new Menu.MenuItemSeparator(),
                Options.PerformanceOverlay.EnabledInstance,
                Options.PerformanceOverlay.ModeInstance,
                Options.PerformanceOverlay.KernelDriversInstance,
                Options.FanControl.Instance,
                Options.SteamController.Instance
            }
        };

        private static bool Is24hClock
        {
            get => DateTimeFormatInfo.CurrentInfo.ShortTimePattern.Contains("HH");
        }
    }
}
