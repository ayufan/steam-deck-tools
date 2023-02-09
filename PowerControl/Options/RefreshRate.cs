using PowerControl.Helpers;
using PowerControl.Helpers.AMD;

namespace PowerControl.Options
{
    public static class RefreshRate
    {
        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "Refresh Rate",
            PersistentKey = "RefreshRate",
            ApplyDelay = 1000,
            ResetValue = () => { return DisplayResolutionController.GetRefreshRates().Max().ToString(); },
            OptionsValues = delegate ()
            {
                var refreshRates = DisplayResolutionController.GetRefreshRates().ToList();

                var currentRefreshRate = DisplayResolutionController.GetRefreshRate();
                if (currentRefreshRate > 0 && !refreshRates.Contains(currentRefreshRate))
                {
                    refreshRates.Add(currentRefreshRate);
                    refreshRates.Sort();
                }

                if (refreshRates.Count() > 1)
                    return refreshRates.Select(item => item.ToString()).ToArray();
                return null;
            },
            CurrentValue = delegate ()
            {
                return DisplayResolutionController.GetRefreshRate().ToString();
            },
            ApplyValue = (selected) =>
            {
                var selectedRefreshRate = int.Parse(selected);

#if USE_ADL2
                if (ExternalHelpers.DisplayConfig.IsInternalConnected == true)
                {
                    var currentResolution = DisplayResolutionController.GetResolution();
                    if (currentResolution == null)
                        return null;

                    var modes = ModeTiming.GetAllModes();

                    // ModeTiming.ReplaceTiming(new Helpers.AMD.ADLDisplayModeX2()
                    // {
                    //     PelsWidth = currentResolution.Value.Width,
                    //     PelsHeight = currentResolution.Value.Height,
                    //     RefreshRate = selectedRefreshRate,
                    //     TimingStandard = Helpers.AMD.ADL.ADL_DL_MODETIMING_STANDARD_CVT,
                    // });

                    ModeTiming.AddTiming(ModeTiming.Mode1280x800p40);

                    ModeTiming.SetTiming(new Helpers.AMD.ADLDisplayModeX2()
                    {
                        PelsWidth = currentResolution.Value.Width,
                        PelsHeight = currentResolution.Value.Height,
                        RefreshRate = selectedRefreshRate,
                        TimingStandard = Helpers.AMD.ADL.ADL_DL_MODETIMING_STANDARD_CVT,
                    });
                }
                else
#endif
                {
                    DisplayResolutionController.SetRefreshRate(selectedRefreshRate);
                }

                return DisplayResolutionController.GetRefreshRate().ToString();
            },
            Impacts =
            {
                FPSLimit.Instance
            }
        };
    }
}
