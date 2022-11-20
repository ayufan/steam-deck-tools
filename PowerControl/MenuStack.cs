using CommonHelpers;
using PowerControl.Helpers;
using PowerControl.Helpers.GPU;
using System.Diagnostics;

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
                    Options = { 0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100 },
                    CycleOptions = false,
                    CurrentValue = delegate()
                    {
                        return Helpers.WindowsSettingsBrightnessController.Get(5.0);
                    },
                    ApplyValue = delegate(object selected)
                    {
                        Helpers.WindowsSettingsBrightnessController.Set((int)selected);

                        return Helpers.WindowsSettingsBrightnessController.Get(5.0);
                    }
                },
                new Menu.MenuItemWithOptions()
                {
                    Name = "Volume",
                    Options = { 0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100 },
                    CycleOptions = false,
                    CurrentValue = delegate()
                    {
                        return Helpers.AudioManager.GetMasterVolume(5.0);
                    },
                    ApplyValue = delegate(object selected)
                    {
                        Helpers.AudioManager.SetMasterVolumeMute(false);
                        Helpers.AudioManager.SetMasterVolume((int)selected);

                        return Helpers.AudioManager.GetMasterVolume(5.0);
                    }
                },
                new Menu.MenuItemWithOptions()
                {
                    Name = "Resolution",
                    ApplyDelay = 1000,
                    ResetValue = () => {
                        if (!AMDAdrenaline.IsGPUScalingEnabled() && !Settings.Default.EnableExperimentalFeatures)
                            return null;
                        return DisplayResolutionController.GetAllResolutions().Last();
                    },
                    OptionsValues = delegate()
                    {
                        var resolutions = DisplayResolutionController.GetAllResolutions();
                        if (resolutions.Count() > 1)
                            return resolutions.Select(item => (object)item).ToArray();
                        return null;
                    },
                    CurrentValue = delegate()
                    {
                        if (!AMDAdrenaline.IsGPUScalingEnabled() && !Settings.Default.EnableExperimentalFeatures)
                            return null;
                        return DisplayResolutionController.GetResolution();
                    },
                    ApplyValue = delegate(object selected)
                    {
                        DisplayResolutionController.SetResolution((DisplayResolutionController.DisplayResolution)selected);
                        Root["Refresh Rate"].Update(); // force refresh Refresh Rate
                        Root["FPS Limit"].Update(); // force refresh FPS limit
                        return DisplayResolutionController.GetResolution();
                    }
                },
                new Menu.MenuItemWithOptions()
                {
                    Name = "Refresh Rate",
                    ApplyDelay = 1000,
                    ResetValue = () => { return DisplayResolutionController.GetRefreshRates().Max(); },
                    OptionsValues = delegate()
                    {
                        var refreshRates = DisplayResolutionController.GetRefreshRates();
                        if (refreshRates.Count() > 1)
                            return refreshRates.Select(item => (object)item).ToArray();
                        return null;
                    },
                    CurrentValue = delegate()
                    {
                        return DisplayResolutionController.GetRefreshRate();
                    },
                    ApplyValue = delegate(object selected)
                    {
                        DisplayResolutionController.SetRefreshRate((int)selected);
                        Root["FPS Limit"].Update(); // force refresh FPS limit
                        return DisplayResolutionController.GetRefreshRate();
                    }
                },
                new Menu.MenuItemWithOptions()
                {
                    Name = "FPS Limit",
                    ApplyDelay = 500,
                    ResetValue = () => { return "Off"; },
                    OptionsValues = delegate()
                    {
                        var refreshRate = DisplayResolutionController.GetRefreshRate();
                        return new object[]
                        {
                            refreshRate / 4, refreshRate / 2, refreshRate, "Off"
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
                    Options = { "3W", "4W", "5W", "6W", "7W", "8W", "10W", "12W", "15W" },
                    ApplyDelay = 1000,
                    ResetValue = () => { return "15W"; },
                    ActiveOption = "?",
                    ApplyValue = delegate(object selected)
                    {
                        uint mW = uint.Parse(selected.ToString().Replace("W", "")) * 1000;

                        if (VangoghGPU.IsSupported)
                        {
                            using (var sd = VangoghGPU.Open())
                            {
                                if (sd is null)
                                    return null;

                                sd.SlowTDP = mW;
                                sd.FastTDP = mW;
                            }
                        }
                        else
                        {
                            uint stampLimit = mW/10;

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
                        }

                        return selected;
                    }
                },
                new Menu.MenuItemWithOptions()
                {
                    Name = "GPU",
                    Options = { "Default", "400MHz", "800MHz", "1200MHz", "1600MHz" },
                    ApplyDelay = 1000,
                    Visible = VangoghGPU.IsSupported,
                    ActiveOption = "?",
                    ResetValue = () => { return "Default"; },
                    ApplyValue = delegate(object selected)
                    {
                        using (var sd = VangoghGPU.Open())
                        {
                            if (sd is null)
                                return null;

                            if (selected.ToString() == "Default")
                            {
                                sd.HardMinGfxClock = 200;
                                return selected;
                            }

                            sd.HardMinGfxClock = uint.Parse(selected.ToString().Replace("MHz", ""));
                            return selected;
                        }
                    }
                },
                new Menu.MenuItemWithOptions()
                {
                    Name = "CPU",
                    Options = { "Default", "Power-Save", "Balanced", "Max" },
                    ApplyDelay = 1000,
                    ActiveOption = "?",
                    Visible = VangoghGPU.IsSupported,
                    ResetValue = () => { return "Default"; },
                    ApplyValue = delegate(object selected)
                    {
                        using (var sd = VangoghGPU.Open())
                        {
                            if (sd is null)
                                return null;

                            switch(selected.ToString())
                            {
                                case "Default":
                                    sd.MinCPUClock = 1400;
                                    sd.MaxCPUClock = 3500;
                                    break;

                                case "Power-Save":
                                    sd.MinCPUClock = 1400;
                                    sd.MaxCPUClock = 1800;
                                    break;

                                case "Balanced":
                                    sd.MinCPUClock = 2200;
                                    sd.MaxCPUClock = 2800;
                                    break;

                                case "Max":
                                    sd.MinCPUClock = 3000;
                                    sd.MaxCPUClock = 3500;
                                    break;

                                default:
                                    return null;
                            }
                            return selected;
                        }
                    }
                },
                new Menu.MenuItemWithOptions()
                {
                    Name = "SMT",
                    ApplyDelay = 500,
                    Options = { "No", "Yes" },
                    ResetValue = () => { return "Yes"; },
                    CurrentValue = delegate()
                    {
                        if (!RTSS.IsOSDForeground(out var processId))
                            return null;
                        if (!ProcessorCores.HasSMTThreads())
                            return null;

                        return ProcessorCores.IsUsingSMT(processId.Value) ? "Yes" : "No";
                    },
                    ApplyValue = delegate(object selected)
                    {
                        if (!RTSS.IsOSDForeground(out var processId))
                            return null;
                        if (!ProcessorCores.HasSMTThreads())
                            return null;

                        ProcessorCores.SetProcessSMT(processId.Value, selected.ToString() == "Yes");

                        return ProcessorCores.IsUsingSMT(processId.Value) ? "Yes" : "No";
                    }
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
