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
                return new string[]
                {
                    (refreshRate / 4).ToString(),
                    (refreshRate / 2).ToString(),
                    refreshRate.ToString(),
                    "Off"
                };
            },
            CurrentValue = delegate ()
            {
                try
                {
                    RTSS.LoadProfile();
                    if (RTSS.GetProfileProperty("FramerateLimit", out int framerate))
                        return (framerate == 0) ? "Off" : framerate.ToString();
                }
                catch { }
                return null;
            },
            ApplyValue = (selected) =>
            {
                try
                {
                    int framerate = 0;
                    if (selected != "Off")
                        framerate = int.Parse(selected);

                    RTSS.LoadProfile();
                    if (!RTSS.SetProfileProperty("FramerateLimit", framerate))
                        return null;
                    if (!RTSS.GetProfileProperty("FramerateLimit", out framerate))
                        return null;
                    RTSS.SaveProfile();
                    RTSS.UpdateProfiles();
                    return (framerate == 0) ? "Off" : framerate.ToString();
                }
                catch { }
                return null;
            }
        };
    }
}
