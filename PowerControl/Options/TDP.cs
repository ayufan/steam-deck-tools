using System.Diagnostics;
using CommonHelpers;
using PowerControl.Helpers.AMD;

namespace PowerControl.Options
{
    public static class TDP
    {
        public const string SlowTDP = "SlowTDP";
        public const string FastTDP = "FastTDP";

        public const string ResetTDP = "15W";

        public const int DefaultSlowTDP = 15000;
        public const int DefaultFastTDP = 15000;

        public static PersistedOptions UserOptions()
        {
            var options = new PersistedOptions("TDP");

            if (options.GetOptions().Count() == 0)
            {
                options.SetOptions(new PersistedOptions.Option[]
                {
                    options.ForOption("3W").Set(SlowTDP, 3000).Set(FastTDP, 3000),
                    options.ForOption("4W").Set(SlowTDP, 4000).Set(FastTDP, 4000),
                    options.ForOption("5W").Set(SlowTDP, 5000).Set(FastTDP, 5000),
                    options.ForOption("6W").Set(SlowTDP, 6000).Set(FastTDP, 6000),
                    options.ForOption("7W").Set(SlowTDP, 7000).Set(FastTDP, 7000),
                    options.ForOption("8W").Set(SlowTDP, 8000).Set(FastTDP, 8000),
                    options.ForOption("9W").Set(SlowTDP, 9000).Set(FastTDP, 9000),
                    options.ForOption(GlobalConstants.DefaultSilentTDP).Set(SlowTDP, 10000).Set(FastTDP, 10000),
                    options.ForOption("11W").Set(SlowTDP, 11000).Set(FastTDP, 11000),
                    options.ForOption("12W").Set(SlowTDP, 12000).Set(FastTDP, 12000),
                    options.ForOption("13W").Set(SlowTDP, 13000).Set(FastTDP, 13000),
                    options.ForOption("14W").Set(SlowTDP, 14000).Set(FastTDP, 14000),
                    options.ForOption(ResetTDP).Set(SlowTDP, 15000).Set(FastTDP, 15000),
                });
            }

            return options;
        }

        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "TDP",
            PersistentKey = "TDP",
            PersistOnCreate = false,
            OptionsValues = () => { return UserOptions().GetOptions(); },
            ApplyDelay = 1000,
            ResetValue = () => { return ResetTDP; },
            CurrentValue = delegate ()
            {
                if (SharedData<FanModeSetting>.GetExistingValue(out var value) && value.Current == FanMode.Silent)
                    return GlobalConstants.DefaultSilentTDP;
                else if (Instance != null)
                    return Instance.ActiveOption;
                else
                    return ResetTDP;
            },
            ApplyValue = (selected) =>
            {
                if (!AntiCheatSettings.Default.AckAntiCheat(
                    Controller.TitleWithVersion,
                    "Changing TDP requires kernel access for a short period.",
                    "Leave the game if it uses anti-cheat protection."))
                    return null;

                var selectedOption = UserOptions().ForOption(selected);
                if (!selectedOption.Exist)
                    return null;

                var slowTDP = selectedOption.Get(SlowTDP, DefaultSlowTDP);
                var fastTDP = selectedOption.Get(FastTDP, DefaultFastTDP);

                if (SharedData<FanModeSetting>.GetExistingValue(out var fanMode) 
                    && fanMode.Current == FanMode.Silent
                    && selected != GlobalConstants.DefaultSilentTDP)
                {
                    fanMode.Desired = FanMode.SemiSilent;
                    SharedData<FanModeSetting>.SetExistingValue(fanMode);
                }

                if (VangoghGPU.IsSupported)
                {
                    return CommonHelpers.Instance.WithGlobalMutex<string>(200, () =>
                    {
                        using (var sd = VangoghGPU.Open())
                        {
                            if (sd is null)
                                return null;

                            sd.SlowTDP = (uint)slowTDP;
                            sd.FastTDP = (uint)fastTDP;
                        }

                        return selected;
                    });
                }
                else
                {
                    int stampLimit = slowTDP / 10;

                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = "Resources/RyzenAdj/ryzenadj.exe",
                        ArgumentList = {
                            "--stapm-limit=" + stampLimit.ToString(),
                            "--slow-limit=" + slowTDP.ToString(),
                            "--fast-limit=" + fastTDP.ToString(),
                        },
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });

                    return selected;
                }
            }
        };
    }
}
