using PowerControl.Helpers;
using PowerControl.Helpers.AMD;

namespace PowerControl.Options
{
    public static class Resolution
    {
        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "Resolution",
            PersistentKey = "Resolution",
            ApplyDelay = 1000,
            ResetValue = () =>
            {
                if (!GPUScaling.SafeResolutionChange && !Settings.Default.EnableExperimentalFeatures)
                    return null;
                return DisplayResolutionController.GetAllResolutions().Last().ToString();
            },
            OptionsValues = delegate ()
            {
                var resolutions = DisplayResolutionController.GetAllResolutions().ToList();

                var currentResolution = DisplayResolutionController.GetResolution();
                if (currentResolution is not null)
                    resolutions.Add(currentResolution.Value);

                if (ExternalHelpers.DisplayConfig.IsInternalConnected == true)
                {
                    foreach (var displayModeInfo in ModeTiming.AllDetailedTimings())
                    {
                        if (currentResolution?.Rotated == true)
                        {
                            resolutions.Add(new DisplayResolutionController.DisplayResolution(
                                displayModeInfo.iPelsHeight, displayModeInfo.iPelsWidth));
                        }
                        else
                        {
                            resolutions.Add(new DisplayResolutionController.DisplayResolution(
                                displayModeInfo.iPelsWidth, displayModeInfo.iPelsHeight));
                        }
                    }
                }

                resolutions = resolutions.Distinct().ToList();
                resolutions.Sort();

                if (resolutions.Count() > 1)
                    return resolutions.Select(item => item.ToString()).ToArray();

                return null;
            },
            CurrentValue = delegate ()
            {
                if (!GPUScaling.SafeResolutionChange && !Settings.Default.EnableExperimentalFeatures)
                    return null;
                return DisplayResolutionController.GetResolution().ToString();
            },
            ApplyValue = (selected) =>
            {
                var selectedResolution = new DisplayResolutionController.DisplayResolution(selected);
                selectedResolution.Rotated = DisplayResolutionController.GetResolution()?.Rotated;

                if (ExternalHelpers.DisplayConfig.IsInternalConnected == true)
                {
                    var normalizedResolution = selectedResolution.Normalize();

                    bool result = ModeTiming.AddTiming(new Helpers.AMD.ADLDisplayModeX2()
                    {
                        PelsWidth = normalizedResolution.Width,
                        PelsHeight = normalizedResolution.Height,
                        RefreshRate = DisplayResolutionController.GetRefreshRate(),
                        TimingStandard = Helpers.AMD.ADL.ADL_DL_MODETIMING_STANDARD_CUSTOM,
                    });

                    if (result)
                        Thread.Sleep(500);
                }

                DisplayResolutionController.SetResolution(selectedResolution);
                return DisplayResolutionController.GetResolution().ToString();
            },
            Impacts =
            {
                RefreshRate.Instance,
                FPSLimit.Instance
            }
        };
    }
}
