using CommonHelpers;
using PowerControl.Helpers.AMD;

namespace PowerControl.Options
{
    public static class CPUFrequency
    {
        public const string SoftMin = "SoftMin";
        public const string SoftMax = "SoftMax";

        public const int DefaultMin = 1400;
        public const int DefaultMax = 3500;

        public static PersistedOptions UserOptions()
        {
            var options = new PersistedOptions("CPUFrequency");

            if (options.GetOptions().Count() == 0)
            {
                options.SetOptions(new PersistedOptions.Option[]
                {
                    options.ForOption("Default").Set(SoftMin, DefaultMin).Set(SoftMax, DefaultMax),
                    options.ForOption("Power-Save").Set(SoftMin, 1400).Set(SoftMax, 1800),
                    options.ForOption("Balanced").Set(SoftMin, 2200).Set(SoftMax, 2800),
                    options.ForOption("Max").Set(SoftMin, 3000).Set(SoftMax, 3500),
                });
            }

            return options;
        }

        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "CPU",
            PersistentKey = "CPUFrequency",
            PersistOnCreate = false,
            OptionsValues = () => { return UserOptions().GetOptions(); },
            ApplyDelay = 1000,
            ActiveOption = "?",
            Visible = VangoghGPU.IsSupported,
            ResetValue = () => { return UserOptions().GetOptions().FirstOrDefault("Default"); },
            ApplyValue = (selected) =>
            {
                if (!AntiCheatSettings.Default.AckAntiCheat(
                    Controller.TitleWithVersion,
                    "Changing CPU frequency requires kernel access for a short period.",
                    "Leave the game if it uses anti-cheat protection."))
                    return null;

                var selectedOption = UserOptions().ForOption(selected);
                if (!selectedOption.Exist)
                    return null;

                var softMin = selectedOption.Get(SoftMin, DefaultMin);
                var softMax = selectedOption.Get(SoftMax, DefaultMax);

                return CommonHelpers.Instance.WithGlobalMutex<string>(200, () =>
                {
                    using (var sd = VangoghGPU.Open())
                    {
                        if (sd is null)
                            return null;

                        sd.MinCPUClock = (uint)softMin;
                        sd.MaxCPUClock = (uint)softMax;
                        return selected;
                    }
                });
            }
        };
    }
}
