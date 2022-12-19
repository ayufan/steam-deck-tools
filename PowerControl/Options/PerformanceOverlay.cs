using CommonHelpers;

namespace PowerControl.Options
{
    public static class PerformanceOverlay
    {
        public static Menu.MenuItemWithOptions EnabledInstance = new Menu.MenuItemWithOptions()
        {
            Name = "OSD",
            ApplyDelay = 500,
            OptionsValues = delegate ()
            {
                return Enum.GetValues<OverlayEnabled>().Select(item => (object)item).ToArray();
            },
            CurrentValue = delegate ()
            {
                if (SharedData<OverlayModeSetting>.GetExistingValue(out var value))
                    return value.CurrentEnabled;
                return null;
            },
            ApplyValue = delegate (object selected)
            {
                if (!SharedData<OverlayModeSetting>.GetExistingValue(out var value))
                    return null;
                value.DesiredEnabled = (OverlayEnabled)selected;
                if (!SharedData<OverlayModeSetting>.SetExistingValue(value))
                    return null;
                return selected;
            }
        };

        public static Menu.MenuItemWithOptions ModeInstance = new Menu.MenuItemWithOptions()
        {
            Name = "OSD Mode",
            ApplyDelay = 500,
            OptionsValues = delegate ()
            {
                return Enum.GetValues<OverlayMode>().Select(item => (object)item).ToArray();
            },
            CurrentValue = delegate ()
            {
                if (SharedData<OverlayModeSetting>.GetExistingValue(out var value))
                    return value.Current;
                return null;
            },
            ApplyValue = delegate (object selected)
            {
                if (!SharedData<OverlayModeSetting>.GetExistingValue(out var value))
                    return null;
                value.Desired = (OverlayMode)selected;
                if (!SharedData<OverlayModeSetting>.SetExistingValue(value))
                    return null;
                return selected;
            }
        };

        public static Menu.MenuItemWithOptions KernelDriversInstance = new Menu.MenuItemWithOptions()
        {
            Name = "OSD Kernel Drivers",
            ApplyDelay = 500,
            OptionsValues = delegate ()
            {
                return Enum.GetValues<KernelDriversLoaded>().Select(item => (object)item).ToArray();
            },
            CurrentValue = delegate ()
            {
                if (SharedData<OverlayModeSetting>.GetExistingValue(out var value))
                    return value.KernelDriversLoaded;
                return null;
            },
            ApplyValue = delegate (object selected)
            {
                if (!SharedData<OverlayModeSetting>.GetExistingValue(out var value))
                    return null;
                value.DesiredKernelDriversLoaded = (KernelDriversLoaded)selected;
                if (!SharedData<OverlayModeSetting>.SetExistingValue(value))
                    return null;
                return selected;
            }
        };
    }
}
