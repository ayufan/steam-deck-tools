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
                if (currentRefreshRate > 0)
                    refreshRates.Add(currentRefreshRate);

                var normalizedResolution = DisplayResolutionController.GetResolution()?.Normalize();
                if (normalizedResolution is not null && ExternalHelpers.DisplayConfig.IsInternalConnected == true)
                {
                    foreach (var displayModeInfo in ModeTiming.AllDetailedTimings())
                    {
                        if (displayModeInfo.iPelsWidth == normalizedResolution.Value.Width &&
                            displayModeInfo.iPelsHeight == normalizedResolution.Value.Height)
                        {
                            refreshRates.Add(displayModeInfo.iRefreshRate);
                        }
                    }
                }

                refreshRates = refreshRates.Distinct().ToList();
                refreshRates.Sort();

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

                if (ExternalHelpers.DisplayConfig.IsInternalConnected == true)
                {
                    var currentResolution = DisplayResolutionController.GetResolution()?.Normalize();
                    if (currentResolution == null)
                        return null;

                    bool result = ModeTiming.AddTiming(new Helpers.AMD.ADLDisplayModeX2()
                    {
                        PelsWidth = currentResolution.Value.Width,
                        PelsHeight = currentResolution.Value.Height,
                        RefreshRate = selectedRefreshRate,
                        TimingStandard = Helpers.AMD.ADL.ADL_DL_MODETIMING_STANDARD_CUSTOM,
                    });

                    if (result)
                        Thread.Sleep(500);
                }

                DisplayResolutionController.SetRefreshRate(selectedRefreshRate);
                return DisplayResolutionController.GetRefreshRate().ToString();
            },
            Impacts =
            {
                FPSLimit.Instance
            }
        };
    }
}
