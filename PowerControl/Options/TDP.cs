using System.Diagnostics;
using CommonHelpers;
using PowerControl.Helpers.AMD;

namespace PowerControl.Options
{
    public static class TDP
    {
        public const string SlowTDP = "SlowTDP";
        public const string FastTDP = "FastTDP";

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
                    options.ForOption("10W").Set(SlowTDP, 10000).Set(FastTDP, 10000),
                    options.ForOption("12W").Set(SlowTDP, 12000).Set(FastTDP, 12000),
                    options.ForOption("15W").Set(SlowTDP, 15000).Set(FastTDP, 15000),
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
            ResetValue = () => { return "15W"; },
            ActiveOption = "?",
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
