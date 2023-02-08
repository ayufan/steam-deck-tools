using CommonHelpers;
using PowerControl.Helpers;

namespace PowerControl.Options
{
    public static class FPSLimit
    {
        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "FPS Limit",
            PersistentKey = "FPSLimit",
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
                    if (!Dependencies.EnsureRTSS(null))
                        return "?";

                    RTSS.LoadProfile();
                    if (RTSS.GetProfileProperty("FramerateLimit", out int framerate))
                        return (framerate == 0) ? "Off" : framerate.ToString();
                    return null;
                }
                catch (Exception e)
                {
#if DEBUG
                    CommonHelpers.Log.TraceException("RTSS", e);
#endif
                    return "?";
                }
            },
            ApplyValue = (selected) =>
            {
                try
                {
                    if (!Dependencies.EnsureRTSS(Controller.TitleWithVersion))
                        return null;

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
                catch (Exception e)
                {
                    CommonHelpers.Log.TraceException("RTSS", e);
                }
                return null;
            },
            ImpactedBy = (option, was, isNow) =>
            {
                if (Instance is null)
                    return;

                try
                {
                    if (!Dependencies.EnsureRTSS(null))
                        return;

                    var refreshRate = DisplayResolutionController.GetRefreshRate();
                    if (refreshRate <= 0)
                        return;

                    RTSS.LoadProfile();
                    RTSS.GetProfileProperty("FramerateLimit", out int fpsLimit);
                    if (fpsLimit == 0)
                        return;

                    // FPSLimit, RR => outcome
                    // 50 + 60 => 60 (div 1)
                    // 25 + 60 => 30 (div 2)
                    // 10 + 60 => 15 (div 6)
                    // 60 + 50 => 50 (div 0)
                    // 50 + 40 => 40 (div 0)
                    // 60 + 30 => 30 (div 0)
                    int div = refreshRate / fpsLimit;
                    if (div >= 4)
                        fpsLimit = refreshRate / 4;
                    else if (div >= 2)
                        fpsLimit = refreshRate / 2;
                    else
                        fpsLimit = refreshRate;
                    RTSS.SetProfileProperty("FramerateLimit", fpsLimit);
                    RTSS.SaveProfile();
                    RTSS.UpdateProfiles();
                }
                catch (Exception e)
                {
#if DEBUG
                    CommonHelpers.Log.TraceException("RTSS", e);
#endif
                }
            }
        };
    }
}
