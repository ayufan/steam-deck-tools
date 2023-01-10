using CommonHelpers;

namespace PowerControl.Options
{
    public static class FanControl
    {
        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "FAN",
            PersistentKey = "FANMode",
            PersistOnCreate = false,
            ApplyDelay = 500,
            OptionsValues = delegate ()
            {
                return Enum.GetNames<FanMode>();
            },
            CurrentValue = delegate ()
            {
                if (SharedData<FanModeSetting>.GetExistingValue(out var value))
                    return value.Current.ToString();
                return null;
            },
            ApplyValue = (selected) =>
            {
                if (!SharedData<FanModeSetting>.GetExistingValue(out var value))
                    return null;
                value.Desired = Enum.Parse<FanMode>(selected);
                if (!SharedData<FanModeSetting>.SetExistingValue(value))
                    return null;
                return selected;
            }
        };
    }
}
