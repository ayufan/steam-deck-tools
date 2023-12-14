using CommonHelpers;

namespace PowerControl.Options
{
    public static class BatteryChargeLimit
    {
        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "Charge Limit",
            ApplyDelay = 1000,
            Options = { "70%", "80%", "90%", "100%" },
            ActiveOption = "?",
            ApplyValue = (selected) =>
            {
                var value = int.Parse(selected.ToString().TrimEnd('%'));

                using (var vlv0100 = new Vlv0100())
                {
                    if (!vlv0100.Open())
                        return null;

                    vlv0100.SetMaxBatteryCharge(value);

                    var newValue = vlv0100.GetMaxBatteryCharge();
                    if (newValue is null)
                        return null;
                    return newValue.ToString() + "%";
                }
            }
        };
    }
}
