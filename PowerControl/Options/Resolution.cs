using PowerControl.Helpers;
using PowerControl.Helpers.AMD;

namespace PowerControl.Options
{
    public static class Resolution
    {
        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "Resolution",
            ApplyDelay = 1000,
            ResetValue = () =>
            {
                if (!GPUScaling.SafeResolutionChange && !Settings.Default.EnableExperimentalFeatures)
                    return null;
                return DisplayResolutionController.GetAllResolutions().Last();
            },
            OptionsValues = delegate ()
            {
                var resolutions = DisplayResolutionController.GetAllResolutions();
                if (resolutions.Count() > 1)
                    return resolutions.Select(item => (object)item).ToArray();
                return null;
            },
            CurrentValue = delegate ()
            {
                if (!GPUScaling.SafeResolutionChange && !Settings.Default.EnableExperimentalFeatures)
                    return null;
                return DisplayResolutionController.GetResolution();
            },
            ApplyValue = delegate (object selected)
            {
                DisplayResolutionController.SetResolution((DisplayResolutionController.DisplayResolution)selected);
                // force refresh Refresh Rate
                RefreshRate.Instance.Update();
                // force reset and refresh of FPS limit
                FPSLimit.Instance.Reset();
                FPSLimit.Instance.Update();
                return DisplayResolutionController.GetResolution();
            }
        };
    }
}
