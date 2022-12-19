namespace PowerControl.Options
{
    public static class Volume
    {
        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "Volume",
            Options = { "0", "5", "10", "15", "20", "25", "30", "35", "40", "45", "50", "55", "60", "65", "70", "75", "80", "85", "90", "95", "100" },
            CycleOptions = false,
            CurrentValue = delegate ()
            {
                try { return Helpers.AudioManager.GetMasterVolume(5.0).ToString(); }
                catch (Exception) { return null; }
            },
            ApplyValue = (selected) =>
            {
                try
                {
                    Helpers.AudioManager.SetMasterVolumeMute(false);
                    Helpers.AudioManager.SetMasterVolume(int.Parse(selected));

                    return Helpers.AudioManager.GetMasterVolume(5.0).ToString();
                }
                catch (Exception)
                {
                    // In some cases MasterVolume device is missing
                    return null;
                }
            }
        };
    }
}
