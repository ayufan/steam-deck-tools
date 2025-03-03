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
                int refreshRate = DisplayResolutionController.GetRefreshRate();
        		string[] availableLimits = new string[refreshRate/5];
        		for (int i = 0; i < refreshRate/5; i++) {
        			availableLimits[i] = string.Format("{0}", (i + 1) * 5);
        		}

                // dissalow to use fps limits lower than 15
                string[] allowedLimits = Array.FindAll(availableLimits, val => val != null && Int32.Parse(val) >= 15)
                
                return allowedLimits;
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
                    if (fpsLimit <= 0)
                        return;

                    if (fpsLimit > refreshRate) {
                        fpsLimit = refreshRate;
                    }

                    if (fpsLimit < 15) {
                        fpsLimit = 15;
                    }
                    
                    int convertFpsLimitDividedByFive(int limit) {
                        var leftOver = limit % 5;
                        if (leftOver == 0) {
                            return limit;
                        }
                        
                        if (leftOver >= 3) {
                            return limit + (5 - leftOver);
                        }
                        
                        return limit - leftOver;
                    }
                    
                    RTSS.SetProfileProperty("FramerateLimit", convertFpsLimitDividedByFive(fpsLimit));
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
