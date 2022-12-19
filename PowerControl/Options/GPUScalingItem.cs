using PowerControl.Helpers.AMD;

namespace PowerControl.Options
{
    public static class GPUScalingItem
    {
        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "GPU Scaling",
            ApplyDelay = 1000,
            Options = Enum.GetValues<GPUScaling.ScalingMode>().Cast<object>().Prepend("Off").ToArray(),
            CurrentValue = delegate ()
            {
                if (!GPUScaling.IsSupported)
                    return null;
                if (!GPUScaling.Enabled)
                    return "Off";
                return GPUScaling.Mode;
            },
            ApplyValue = delegate (object selected)
            {
                if (!GPUScaling.IsSupported)
                    return null;

                if (selected is GPUScaling.ScalingMode)
                    GPUScaling.Mode = (GPUScaling.ScalingMode)selected;
                else
                    GPUScaling.Enabled = false;

                // Since the RadeonSoftware will try to revert values
                RadeonSoftware.Kill();

                Resolution.Instance.Update();
                RefreshRate.Instance.Update();
                FPSLimit.Instance.Reset();
                FPSLimit.Instance.Update();

                if (!GPUScaling.Enabled)
                    return "Off";
                return GPUScaling.Mode;
            }
        };
    }
}
