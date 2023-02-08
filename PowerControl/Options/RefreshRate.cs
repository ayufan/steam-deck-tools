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
                DisplayResolutionController.SetRefreshRate(int.Parse(selected));

                return DisplayResolutionController.GetRefreshRate().ToString();
            },
            Impacts =
            {
                FPSLimit.Instance
            }
        };
    }
}
