using CommonHelpers;

namespace PowerControl.Options
{
    public static class FanControl
    {
        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "FAN",
            ApplyDelay = 500,
            OptionsValues = delegate ()
            {
                return Enum.GetValues<FanMode>().Select(item => (object)item).ToArray();
            },
            CurrentValue = delegate ()
            {
                if (SharedData<FanModeSetting>.GetExistingValue(out var value))
                    return value.Current;
                return null;
            },
            ApplyValue = delegate (object selected)
            {
                if (!SharedData<FanModeSetting>.GetExistingValue(out var value))
                    return null;
                value.Desired = (FanMode)selected;
                if (!SharedData<FanModeSetting>.SetExistingValue(value))
                    return null;
                return selected;
            }
        };
    }
}
