using CommonHelpers;
using PowerControl.External;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace PowerControl
{
    internal class MenuStack
    {
        public static Menu.MenuRoot Root = new Menu.MenuRoot()
        {
            Name = String.Format("\r\n\r\nPower Control v{0}\r\n", Application.ProductVersion.ToString()),
            Items =
            {
                new Menu.MenuItemWithOptions()
                {
                    Name = "Brightness",
                    Options = { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 },

                    CurrentValue = delegate()
                    {
                        return Helpers.WindowsSettingsBrightnessController.Get10();
                    },
                    ApplyValue = delegate(object selected)
                    {
                        Helpers.WindowsSettingsBrightnessController.Set((int)selected);

                        return Helpers.WindowsSettingsBrightnessController.Get10();
                    }
                },
                new Menu.MenuItemWithOptions()
                {
                    Name = "Volume",
                    Options = { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 },
                    CurrentValue = delegate()
                    {
                        return Helpers.AudioManager.GetMasterVolume10();
                    },
                    ApplyValue = delegate(object selected)
                    {
                        Helpers.AudioManager.SetMasterVolumeMute(false);
                        Helpers.AudioManager.SetMasterVolume((int)selected);

                        return Helpers.AudioManager.GetMasterVolume10();
                    }
                },
                new Menu.MenuItemWithOptions()
                {
                    Name = "Refresh Rate",
                    ApplyDelay = 1000,
                    OptionsValues = delegate()
                    {
                        return Helpers.PhysicalMonitorBrightnessController.GetRefreshRates().Select(item => (object)item).ToArray();
                    },
                    CurrentValue = delegate()
                    {
                        return Helpers.PhysicalMonitorBrightnessController.GetRefreshRate();
                    },
                    ApplyValue = delegate(object selected)
                    {
                        Helpers.PhysicalMonitorBrightnessController.SetRefreshRate((int)selected);
                        Root["FPS Limit"].Update(); // force refresh FPS limit
                        return Helpers.PhysicalMonitorBrightnessController.GetRefreshRate();
                    }
                },
                new Menu.MenuItemWithOptions()
                {
                    Name = "FPS Limit",
                    ApplyDelay = 500,
                    OptionsValues = delegate()
                    {
                        var refreshRate = Helpers.PhysicalMonitorBrightnessController.GetRefreshRate();
                        return new object[]
                        {
                            "Off", refreshRate, refreshRate / 2, refreshRate / 4
                        };
                    },
                    CurrentValue = delegate()
                    {
                        try
                        {
                            RTSS.LoadProfile();
                            if (RTSS.GetProfileProperty("FramerateLimit", out int framerate))
                                return (framerate == 0) ? "Off" : framerate;
                        }
                        catch
                        {
                        }
                        return null;
                    },
                    ApplyValue = delegate(object selected)
                    {
                        try
                        {
                            int framerate = 0;
                            if (selected != null && selected.ToString() != "Off")
                                framerate = (int)selected;

                            RTSS.LoadProfile();
                            if (!RTSS.SetProfileProperty("FramerateLimit", framerate))
                                return null;
                            if (!RTSS.GetProfileProperty("FramerateLimit", out framerate))
                                return null;
                            RTSS.SaveProfile();
                            RTSS.UpdateProfiles();
                            return (framerate == 0) ? "Off" : framerate;
                        }
                        catch
                        {
                        }
                        return null;
                    }
                },
                new Menu.MenuItemWithOptions()
                {
                    Name = "TDP",
                    Options = { "Auto", "3W", "4W", "5W", "6W", "7W", "8W", "10W", "12W", "15W" },
                    ApplyDelay = 1000,
                    ApplyValue = delegate(object selected)
                    {
                        int mW = 15000;

                        if (selected.ToString() != "Auto")
                        {
                            mW = int.Parse(selected.ToString().Replace("W", "")) * 1000;
                        }

                        int stampLimit = mW/10;

                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = "Resources/RyzenAdj/ryzenadj.exe",
                            ArgumentList = {
                                "--stapm-limit=" + stampLimit.ToString(),
                                "--slow-limit=" + mW.ToString(),
                                "--fast-limit=" + mW.ToString(),
                            },
                            WindowStyle = ProcessWindowStyle.Hidden,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        });

                        return selected;
                    }
                },
                new Menu.MenuItemWithOptions()
                {
                    Name = "GPU Clock",
                    Options = { "Auto", 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400, 1500, 1600 },
                    ApplyDelay = 1000,
                    Visible = false
                },
                new Menu.MenuItemSeparator(),
                new Menu.MenuItemWithOptions()
                {
                    Name = "OSD",
                    ApplyDelay = 500,
                    OptionsValues = delegate()
                    {
                        return Enum.GetValues<OverlayEnabled>().Select(item => (object)item).ToArray();
                    },
                    CurrentValue = delegate()
                    {
                        if (SharedData<OverlayModeSetting>.GetExistingValue(out var value))
                           return value.CurrentEnabled;
                        return null;
                    },
                    ApplyValue = delegate(object selected)
                    {
                        if (!SharedData<OverlayModeSetting>.GetExistingValue(out var value))
                            return null;
                        value.DesiredEnabled =  (OverlayEnabled)selected;
                        if (!SharedData<OverlayModeSetting>.SetExistingValue(value))
                            return null;
                        return selected;
                    }
                },
                new Menu.MenuItemWithOptions()
                {
                    Name = "OSD Mode",
                    ApplyDelay = 500,
                    OptionsValues = delegate()
                    {
                        return Enum.GetValues<OverlayMode>().Select(item => (object)item).ToArray();
                    },
                    CurrentValue = delegate()
                    {
                        if (SharedData<OverlayModeSetting>.GetExistingValue(out var value))
                           return value.Current;
                        return null;
                    },
                    ApplyValue = delegate(object selected)
                    {
                        if (!SharedData<OverlayModeSetting>.GetExistingValue(out var value))
                            return null;
                        value.Desired = (OverlayMode)selected;
                        if (!SharedData<OverlayModeSetting>.SetExistingValue(value))
                            return null;
                        return selected;
                    }
                },
                new Menu.MenuItemWithOptions()
                {
                    Name = "FAN",
                    ApplyDelay = 500,
                    OptionsValues = delegate()
                    {
                        return Enum.GetValues<FanMode>().Select(item => (object)item).ToArray();
                    },
                    CurrentValue = delegate()
                    {
                        if (SharedData<FanModeSetting>.GetExistingValue(out var value))
                           return value.Current;
                        return null;
                    },
                    ApplyValue = delegate(object selected)
                    {
                        if (!SharedData<FanModeSetting>.GetExistingValue(out var value))
                            return null;
                        value.Desired = (FanMode)selected;
                        if (!SharedData<FanModeSetting>.SetExistingValue(value))
                            return null;
                        return selected;
                    }
                }
            }
        };
    }
}
