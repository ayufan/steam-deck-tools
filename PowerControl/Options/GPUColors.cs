using PowerControl.Helpers.AMD;

namespace PowerControl.Options
{
    public static class GPUColors
    {
        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "Colors",
            PersistentKey = "GPUColors",
            ApplyDelay = 1000,
            Options = Enum.GetNames<DCE.Mode>(),
            CurrentValue = delegate ()
            {
                return DCE.Current.ToString();
            },
            ApplyValue = (selected) =>
            {
                if (DCE.Current is null)
                    return null;

                DCE.Current = Enum.Parse<DCE.Mode>(selected);
                RadeonSoftware.Kill();
                return DCE.Current.ToString();
            }
        };
    }
}
