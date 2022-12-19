using PowerControl.Helpers;
using PowerControl.Helpers.AMD;

namespace PowerControl.Options
{
    public static class RefreshRate
    {
        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "Refresh Rate",
            ApplyDelay = 1000,
            ResetValue = () => { return DisplayResolutionController.GetRefreshRates().Max(); },
            OptionsValues = delegate ()
            {
                var refreshRates = DisplayResolutionController.GetRefreshRates();
                if (refreshRates.Count() > 1)
                    return refreshRates.Select(item => (object)item).ToArray();
                return null;
            },
            CurrentValue = delegate ()
            {
                return DisplayResolutionController.GetRefreshRate();
            },
            ApplyValue = delegate (object selected)
            {
                DisplayResolutionController.SetRefreshRate((int)selected);
                // force reset and refresh of FPS limit
                FPSLimit.Instance.Reset();
                FPSLimit.Instance.Update();
                return DisplayResolutionController.GetRefreshRate();
            }
        };
    }
}
