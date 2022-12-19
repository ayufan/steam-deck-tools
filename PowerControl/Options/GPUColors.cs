using PowerControl.Helpers.AMD;

namespace PowerControl.Options
{
    public static class GPUColors
    {
        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "Colors",
            ApplyDelay = 1000,
            Options = Enum.GetValues<DCE.Mode>().Cast<object>().ToList(),
            CurrentValue = delegate ()
            {
                return DCE.Current;
            },
            ApplyValue = delegate (object selected)
            {
                if (DCE.Current is null)
                    return null;

                DCE.Current = (DCE.Mode)selected;
                RadeonSoftware.Kill();
                return DCE.Current;
            }
        };
    }
}
