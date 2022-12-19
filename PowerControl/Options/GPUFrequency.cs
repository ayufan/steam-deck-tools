using PowerControl.Helpers.AMD;

namespace PowerControl.Options
{
    public static class GPUFrequency
    {
        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "GPU",
            Options = { "Default", "400MHz", "800MHz", "1200MHz", "1600MHz" },
            ApplyDelay = 1000,
            Visible = VangoghGPU.IsSupported,
            ActiveOption = "?",
            ResetValue = () => { return "Default"; },
            ApplyValue = (selected) =>
            {
                if (!Settings.Default.AckAntiCheat(
                    Controller.TitleWithVersion,
                    "GPU",
                    "Changing GPU frequency requires kernel access for a short period. Leave the game if it uses anti-cheat protection.")
                )
                    return null;

                return CommonHelpers.Instance.WithGlobalMutex<string>(200, () =>
                {
                    using (var sd = VangoghGPU.Open())
                    {
                        if (sd is null)
                            return null;

                        if (selected == "Default")
                        {
                            sd.HardMinGfxClock = 200;
                            return selected;
                        }

                        sd.HardMinGfxClock = uint.Parse(selected.Replace("MHz", ""));
                        return selected;
                    }
                });
            }
        };
    }
}
