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
                return new string[] { "30", "35", "40", "45", "48", "50", "55", "60" };

                var refreshRates = DisplayResolutionController.GetRefreshRates();
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
                    var currentResolution = DisplayResolutionController.GetResolution();
                    if (currentResolution == null)
                        return null;

                    var modes = ModeTiming.GetAllModes();

                    ModeTiming.ReplaceTiming(new Helpers.AMD.ADLDisplayModeX2()
                    {
                        PelsWidth = currentResolution.Value.Width,
                        PelsHeight = currentResolution.Value.Height,
                        RefreshRate = selectedRefreshRate,
                        TimingStandard = Helpers.AMD.ADL.ADL_DL_MODETIMING_STANDARD_CVT,
                    });
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
