using CommonHelpers;
using PowerControl.Helpers;

namespace PowerControl.Options
{
    public static class FPSLimit
    {
        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "FPS Limit",
            ApplyDelay = 500,
            ResetValue = () => { return "Off"; },
            OptionsValues = delegate ()
            {
                var refreshRate = DisplayResolutionController.GetRefreshRate();
                return new object[]
                {
                    refreshRate / 4, refreshRate / 2, refreshRate, "Off"
                };
            },
            CurrentValue = delegate ()
            {
                try
                {
                    RTSS.LoadProfile();
                    if (RTSS.GetProfileProperty("FramerateLimit", out int framerate))
                        return (framerate == 0) ? "Off" : framerate;
                }
                catch { }
                return null;
            },
            ApplyValue = delegate (object selected)
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
                catch { }
                return null;
            }
        };
    }
}
