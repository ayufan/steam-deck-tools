using PowerControl.Helpers.AMD;

namespace PowerControl.Options
{
    public static class CPUFrequency
    {
        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "CPU",
            PersistentKey = "CPUFrequency",
            PersistOnCreate = false,
            Options = { "Default", "Power-Save", "Balanced", "Max" },
            ApplyDelay = 1000,
            ActiveOption = "?",
            Visible = VangoghGPU.IsSupported,
            ResetValue = () => { return "Default"; },
            ApplyValue = (selected) =>
            {
                if (!Settings.Default.AckAntiCheat(
                    Controller.TitleWithVersion,
                    "CPU",
                    "Changing GPU frequency requires kernel access for a short period. Leave the game if it uses anti-cheat protection.")
                )
                    return null;

                return CommonHelpers.Instance.WithGlobalMutex<string>(200, () =>
                {
                    using (var sd = VangoghGPU.Open())
                    {
                        if (sd is null)
                            return null;

                        switch (selected.ToString())
                        {
                            case "Default":
                                sd.MinCPUClock = 1400;
                                sd.MaxCPUClock = 3500;
                                break;

                            case "Power-Save":
                                sd.MinCPUClock = 1400;
                                sd.MaxCPUClock = 1800;
                                break;

                            case "Balanced":
                                sd.MinCPUClock = 2200;
                                sd.MaxCPUClock = 2800;
                                break;

                            case "Max":
                                sd.MinCPUClock = 3000;
                                sd.MaxCPUClock = 3500;
                                break;

                            default:
                                return null;
                        }
                        return selected;
                    }
                });
            }
        };
    }
}
