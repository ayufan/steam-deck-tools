using CommonHelpers;
using PowerControl.Helper;
using PowerControl.Menu;

namespace PowerControl.Helpers
{
    public class ProfilesController
    {
        private const string IsTroubledKey = "IsTroubled";
        private const string DefaultName = "Default";

        private string CurrentGame = string.Empty;
        private ProfileSettings DefaultSettings = new ProfileSettings(DefaultName);
        private ProfileSettings? CurrentSettings;
        private static string[] troubledGames = { "dragonageinquisition" };

        private System.Windows.Forms.Timer? timer; 

        public ProfilesController()
        {
            timer = new System.Windows.Forms.Timer();
        }

        public void Initialize()
        {
            MenuStack.Root.ValueChanged += OnOptionValueChange;

            timer.Interval = 1000;
            timer.Tick += (_, _) =>
            {
                timer.Stop();

                RefreshProfiles();

                timer.Start();
            };
            timer.Start();
        }

        private void RefreshProfiles()
        {
            if (!DeviceManager.IsDeckOnlyDisplay)
            {
                CurrentGame = string.Empty;
                return;
            }

            string? gameName;

            RTSS.IsOSDForeground(out _, out gameName);

            // If there's no foreground games keep current profile if possible
            if (gameName == null && RTSS.GetCurrentApps().Contains(CurrentGame))
            {
                gameName = CurrentGame;
            }

            if (gameName == null && CurrentGame != DefaultName)
            {
                CurrentGame = DefaultName;
                CurrentSettings = null;

                ApplyProfile();
            }

            if (gameName != null && CurrentGame != gameName)
            {
                CurrentGame = gameName;
                CurrentSettings = new ProfileSettings(CurrentGame);

                if (!CurrentSettings.Exist)
                {
                    CurrentSettings = null;
                }

                ApplyProfile();
            }
        }

        private void ApplyProfile()
        {
            int delay = GetBoolValue(IsTroubledKey) ? 5200 : 0;
            var options = MenuStack.Root.Items.Where(o => o is MenuItemWithOptions).Select(o => (MenuItemWithOptions)o).ToList();

            foreach (var option in options)
            {
                string? key = option.PersistentKey;

                if (key != null)
                {
                    option.Set(GetValue(option), delay, true);
                }
            }
        }

        private void OnOptionValueChange(MenuItemWithOptions options, string? oldValue, string newValue)
        {
            string? key = options.PersistentKey;

            if (key != null)
            {
                SetValue(key, newValue);
            }
        }

        private void SetBoolValue(string key, bool value)
        {
            var settings = CurrentSettings ?? DefaultSettings;

            settings.Set(key, value);
        }

        private bool GetBoolValue(string key)
        {
            var settings = CurrentSettings ?? DefaultSettings;

            return settings.Get(key, false);
        }

        private void SetValue(string key, string value)
        {
            var settings = CurrentSettings ?? DefaultSettings;
            settings.Set(key, value);
        }

        private string GetValue(MenuItemWithOptions option)
        {
            if (CurrentSettings == null)
            {
                return GetDefaultValue(option);
            }

            return CurrentSettings.Get(option.PersistentKey, GetDefaultValue(option));
        }

        private string GetDefaultValue(MenuItemWithOptions option)
        {
            return DefaultSettings.Get(option.PersistentKey, option.ResetValue?.Invoke() ?? string.Empty);
        }
    }
}
