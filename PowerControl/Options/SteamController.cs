using CommonHelpers;

namespace PowerControl.Options
{
    public static class SteamController
    {
        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "Controller",
            PersistentKey = "SteamController",
            PersistOnCreate = false,
            ApplyDelay = 500,
            OptionsValues = delegate ()
            {
                if (SharedData<SteamControllerSetting>.GetExistingValue(out var value))
                    return value.SelectableProfiles.SplitWithN();
                return null;
            },
            CurrentValue = delegate ()
            {
                if (SharedData<SteamControllerSetting>.GetExistingValue(out var value))
                    return value.CurrentProfile.Length > 0 ? value.CurrentProfile : null;
                return null;
            },
            ApplyValue = (selected) =>
            {
                if (!SharedData<SteamControllerSetting>.GetExistingValue(out var value))
                    return null;
                value.DesiredProfile = selected;
                if (!SharedData<SteamControllerSetting>.SetExistingValue(value))
                    return null;
                return selected;
            }
        };
    }
}
