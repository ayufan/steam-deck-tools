using PowerControl.Helpers;
using PowerControl.Helpers.AMD;

namespace PowerControl.Options
{
    public static class Sharpening
    {
        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "Sharpening",
            ApplyDelay = 500,
            Options = { "Off", "On" },
            CurrentValue = delegate ()
            {
                var value = ImageSharpening.Enabled;
                if (value is null)
                    return null;
                return value.Value ? "On" : "Off";
            },
            ApplyValue = (selected) =>
            {
                ImageSharpening.Enabled = (string)selected == "On";

                var value = ImageSharpening.Enabled;
                if (value is null)
                    return null;
                return value.Value ? "On" : "Off";
            }
        };
    }
}