using CommonHelpers;
using PowerControl.Helpers.AMD;

namespace PowerControl.Options
{
    public static class GPUFrequency
    {
        public const string HardMin = "HardMin";
        public const string SoftMax = "SoftMax";

        public const int DefaultMin = 200;
        public const int DefaultMax = 1600;

        public static PersistedOptions UserOptions()
        {
            var options = new PersistedOptions("GPUFrequency");

            if (options.GetOptions().Count() == 0)
            {
                options.SetOptions(new PersistedOptions.Option[]
                {
                    options.ForOption("Default").Set(HardMin, DefaultMin).Set(SoftMax, DefaultMax),
                    options.ForOption("400MHz").Set(HardMin, 400).Set(SoftMax, DefaultMax),
                    options.ForOption("800MHz").Set(HardMin, 800).Set(SoftMax, DefaultMax),
                    options.ForOption("1200MHz").Set(HardMin, 1200).Set(SoftMax, DefaultMax),
                    options.ForOption("1600MHz").Set(HardMin, 1600).Set(SoftMax, DefaultMax),
                });
            }

            return options;
        }

        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "GPU",
            PersistentKey = "GPUFrequency",
            PersistOnCreate = false,
            OptionsValues = () => { return UserOptions().GetOptions(); },
            ApplyDelay = 1000,
            Visible = VangoghGPU.IsSupported,
            ActiveOption = "?",
            ResetValue = () => { return UserOptions().GetOptions().FirstOrDefault("Default"); },
            ApplyValue = (selected) =>
            {
                if (!AntiCheatSettings.Default.AckAntiCheat(
                    Controller.TitleWithVersion,
                    "Changing GPU frequency requires kernel access for a short period.",
                    "Leave the game if it uses anti-cheat protection."))
                    return null;

                var selectedOption = UserOptions().ForOption(selected);
                if (!selectedOption.Exist)
                    return null;

                var hardMin = selectedOption.Get(HardMin, DefaultMin);
                var softMax = selectedOption.Get(SoftMax, DefaultMax);

                return CommonHelpers.Instance.WithGlobalMutex<string>(200, () =>
                {
                    using (var sd = VangoghGPU.Open())
                    {
                        if (sd is null)
                            return null;

                        sd.HardMinGfxClock = (uint)hardMin;
                        sd.SoftMaxGfxClock = (uint)softMax;
                        return selected;
                    }
                });
            }
        };
    }
}
