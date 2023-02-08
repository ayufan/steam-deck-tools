using PowerControl.Helpers.AMD;

namespace PowerControl.Options
{
    public static class GPUScalingItem
    {
        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "GPU Scaling",
            PersistentKey = "GPUScaling",
            ApplyDelay = 1000,
            Options = Enum.GetNames<GPUScaling.ScalingMode>().Prepend("Off").ToArray(),
            CurrentValue = delegate ()
            {
                if (!GPUScaling.IsSupported)
                    return null;
                if (!GPUScaling.Enabled)
                    return "Off";
                return GPUScaling.Mode.ToString();
            },
            ApplyValue = (selected) =>
            {
                if (!GPUScaling.IsSupported)
                    return null;

                if (selected == "Off")
                    GPUScaling.Enabled = false;
                else
                    GPUScaling.Mode = Enum.Parse<GPUScaling.ScalingMode>(selected);

                // Since the RadeonSoftware will try to revert values
                RadeonSoftware.Kill();

                if (!GPUScaling.Enabled)
                    return "Off";
                return GPUScaling.Mode.ToString();
            },
            Impacts =
            {
                Resolution.Instance,
                RefreshRate.Instance,
                FPSLimit.Instance
            }
        };
    }
}
