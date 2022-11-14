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
                    Options = { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 },

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
                        return Helpers.PhysicalMonitorBrightnessController.GetRefreshRate();
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

                        Process.Start("Resources/RyzenAdj/ryzenadj.exe", new string[] {
                            "--stapm-limit=" + stampLimit.ToString(),
                            "--slow-limit=" + mW.ToString(),
                            "--fast-limit=" + mW.ToString(),
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
                }
            }
        };
    }
}
