namespace PowerControl.Options
{
    public static class Profiles
    {
        public static ProfilesController? Controller;

        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "Profiles",
            OptionsValues = delegate ()
            {
                var currentProfileSettings = Controller?.CurrentProfileSettings;
                if (currentProfileSettings == null)
                    return null;

                if (currentProfileSettings.Exists)
                {
                    return new string[] {
                        currentProfileSettings.ProfileName,
                        "Save All",
                        "Delete"
                    };
                }
                else
                {
                    return new string[] {
                        "None",
                        "Create New",
                        "Save All"
                    };
                }
            },
            CycleOptions = false,
            CurrentValue = delegate ()
            {
                var currentProfileSettings = Controller?.CurrentProfileSettings;
                if (currentProfileSettings == null)
                    return null;

                if (currentProfileSettings.Exists)
                    return currentProfileSettings.ProfileName;
                else
                    return "None";
            },
            ApplyValue = (selected) =>
            {
                switch (selected)
                {
                    case "Delete":
                        Controller?.DeleteProfile();
                        return "None";

                    case "Create New":
                        Controller?.CreateProfile(false);
                        return Controller?.CurrentProfileSettings?.ProfileName;

                    case "Save All":
                        Controller?.CreateProfile(true);
                        return Controller?.CurrentProfileSettings?.ProfileName;

                    default:
                        return selected;
                }
            },
            AfterApply = () =>
            {
                Instance?.Update();
            }
        };
    }
}