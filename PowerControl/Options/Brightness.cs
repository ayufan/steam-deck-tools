namespace PowerControl.Options
{
    public static class Brightness
    {
        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "Brightness",
            Options = { "0", "5", "10", "15", "20", "25", "30", "35", "40", "45", "50", "55", "60", "65", "70", "75", "80", "85", "90", "95", "100" },
            CycleOptions = false,
            CurrentValue = delegate ()
            {
                return Helpers.WindowsSettingsBrightnessController.Get(5.0).ToString();
            },
            ApplyValue = (selected) =>
            {
                Helpers.WindowsSettingsBrightnessController.Set(int.Parse(selected));
                return Helpers.WindowsSettingsBrightnessController.Get(5.0).ToString();
            }
        };
    }
}