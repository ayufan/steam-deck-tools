using System.Diagnostics;
using CommonHelpers;
using ExternalHelpers;
using PowerControl.Helper;
using PowerControl.Menu;

namespace PowerControl
{
    public class ProfilesController : IDisposable
    {
        public const bool AutoCreateProfiles = true;
        public const int ApplyProfileDelayMs = 500;

        private Dictionary<int, PowerControl.Helper.ProfileSettings> watchedProcesses = new Dictionary<int, PowerControl.Helper.ProfileSettings>();
        private CancellationTokenSource? changeTask;

        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer()
        {
            Interval = 1000
        };

        public IEnumerable<String> WatchedProfiles
        {
            get
            {
                foreach (var process in watchedProcesses)
                    yield return process.Value.ProfileName;
            }
        }

        public ProfileSettings? GameProfileSettings { get; private set; }

        public ProfileSettings? SessionProfileSettings { get; private set; }

        public ProfilesController()
        {
            PowerControl.Options.Profiles.Controller = this;
            MenuStack.Root.ValueChanged += Root_OnOptionValueChange;

            timer.Start();
            timer.Tick += Timer_Tick;
        }

        ~ProfilesController()
        {
            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            PowerControl.Options.Profiles.Controller = null;
            MenuStack.Root.ValueChanged -= Root_OnOptionValueChange;
            timer.Stop();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            timer.Enabled = false;

            try { RefreshProfiles(); }
            finally { timer.Enabled = true; }
        }

        private void RefreshProfiles()
        {
            if (DisplayConfig.IsInternalConnected != true)
            {
                foreach (var process in watchedProcesses)
                    RemoveProcess(process.Key);
                return;
            }

            OSDHelpers.Applications.Instance.Refresh();

            if (OSDHelpers.Applications.Instance.FindForeground(out var processId, out var processName))
            {
                if (!BringUpProcess(processId))
                    AddProcess(processId, processName);
            }

            foreach (var process in watchedProcesses)
            {
                if (OSDHelpers.Applications.Instance.IsRunning(process.Key))
                    continue;
                RemoveProcess(process.Key);
            }
        }

        private bool BringUpProcess(int processId)
        {
            if (!watchedProcesses.TryGetValue(processId, out var profileSettings))
                return false;

            if (GameProfileSettings != profileSettings)
            {
                Log.TraceLine("ProfilesController: Foreground changed: {0} => {1}",
                    GameProfileSettings?.ProfileName, profileSettings.ProfileName);
                GameProfileSettings = profileSettings;
                ProfileChanged(profileSettings);
            }
            return true;
        }

        private void AddProcess(int processId, string processName)
        {
            Log.TraceLine("ProfilesController: New Process: {0}/{1}", processId, processName);

            var profileSettings = new ProfileSettings(processName);
            watchedProcesses.Add(processId, profileSettings);

            // Create memory only SessionProfileSettings
            if (SessionProfileSettings is null)
            {
                SessionProfileSettings = new ProfileSettings("Session:" + processName) { UseConfigFile = false };
                SaveProfile(SessionProfileSettings);
            }

            GameProfileSettings = profileSettings;
            ProfileChanged(profileSettings);
            ApplyProfile(profileSettings);
        }

        private void RemoveProcess(int processId)
        {
            if (!watchedProcesses.Remove(processId, out var profileSettings))
                return;

            if (GameProfileSettings == profileSettings)
                GameProfileSettings = null;

            Log.TraceLine("ProfilesController: Removed Process: {0}", processId);

            if (watchedProcesses.Any())
                return;

            ResetProfile();
        }

        private void Root_OnOptionValueChange(MenuItemWithOptions options, string? oldValue, string newValue)
        {
            if (options.PersistentKey is null)
                return;

            // No active profile, cannot persist
            if (GameProfileSettings is null)
                return;

            // Do not auto-create profile unless requested
            if (!GameProfileSettings.Exists && !AutoCreateProfiles)
                return;

            GameProfileSettings.SetValue(options.PersistentKey, newValue);
            options.ProfileOption = newValue;

            Log.TraceLine("ProfilesController: Stored: {0} {1} = {2}",
                GameProfileSettings.ProfileName, options.PersistentKey, newValue);
        }

        private void ProfileChanged(ProfileSettings? profileSettings)
        {
            foreach (var menuItem in PersistableOptions())
            {
                menuItem.ProfileOption = profileSettings?.GetValue(menuItem.PersistentKey ?? "");
            }
        }

        public void CreateProfile(bool saveAll = true)
        {
            var profileSettings = GameProfileSettings;
            if (profileSettings is null)
                return;

            profileSettings.TouchFile();

            Log.TraceLine("ProfilesController: Created Profile: {0}, SaveAll={1}",
                profileSettings.ProfileName, saveAll);

            if (saveAll)
                SaveProfile(profileSettings);

            ProfileChanged(profileSettings);
        }

        public void DeleteProfile()
        {
            GameProfileSettings?.DeleteFile();
            ProfileChanged(GameProfileSettings);

            Log.TraceLine("ProfilesController: Deleted Profile: {0}", GameProfileSettings?.ProfileName);
        }

        private void SaveProfile(ProfileSettings profileSettings, bool force = false)
        {
            foreach (var menuItem in PersistableOptions())
            {
                if (menuItem.ActiveOption is null || !menuItem.PersistOnCreate && !force)
                    continue;
                profileSettings?.SetValue(menuItem.PersistentKey ?? "", menuItem.ActiveOption);
            }
        }

        private void ApplyProfile(ProfileSettings profileSettings)
        {
            if (profileSettings is null || profileSettings?.Exists != true)
                return;

            int delay = profileSettings.GetInt("ApplyDelay", ApplyProfileDelayMs);

            changeTask?.Cancel();
            changeTask = Dispatcher.RunWithDelay(delay, () =>
            {
                foreach (var menuItem in PersistableOptions())
                {
                    var persistedValue = profileSettings.GetValue(menuItem.PersistentKey ?? "");
                    if (persistedValue is null)
                        continue;

                    try
                    {
                        menuItem.Set(persistedValue, true, false);

                        Log.TraceLine("ProfilesController: Applied from Profile: {0}: {1} = {2}",
                            profileSettings.ProfileName, menuItem.PersistentKey, persistedValue);
                    }
                    catch (Exception e)
                    {
                        Log.TraceLine("ProfilesController: Exception Profile: {0}: {1} = {2} => {3}",
                            profileSettings.ProfileName, menuItem.PersistentKey, persistedValue, e);

                        profileSettings.DeleteKey(menuItem.PersistentKey ?? "");
                        menuItem.ProfileOption = null;
                    }
                }
            });
        }

        private void ResetProfile()
        {
            GameProfileSettings = null;
            ProfileChanged(null);

            if (SessionProfileSettings is not null)
            {
                ApplyProfile(SessionProfileSettings);
                SessionProfileSettings = null;
            }
        }

        private IEnumerable<MenuItemWithOptions> PersistableOptions()
        {
            return MenuItemWithOptions.
                Order(MenuStack.Root.AllMenuItemOptions()).
                Where((item) => item.PersistentKey is not null).
                Reverse();
        }
    }
}
