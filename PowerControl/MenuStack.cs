namespace PowerControl
{
    internal class MenuStack
    {
        public static Menu.MenuRoot Root = new Menu.MenuRoot()
        {
            Name = String.Format("\r\n\r\nPower Control v{0}\r\n", Application.ProductVersion.ToString()),
            Items =
            {
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
    }
}
