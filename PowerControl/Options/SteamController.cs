using CommonHelpers;

namespace PowerControl.Options
{
    public static class SteamController
    {
        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "Controller",
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
            ApplyValue = delegate (object selected)
            {
                if (!SharedData<SteamControllerSetting>.GetExistingValue(out var value))
                    return null;
                value.DesiredProfile = (String)selected;
                if (!SharedData<SteamControllerSetting>.SetExistingValue(value))
                    return null;
                return selected;
            }
        };
    }
}
