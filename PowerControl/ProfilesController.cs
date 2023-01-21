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
        public const int ResetProfileDelayMs = 500;

        private Dictionary<int, PowerControl.Helper.ProfileSettings> watchedProcesses = new Dictionary<int, PowerControl.Helper.ProfileSettings>();
        private Dictionary<MenuItemWithOptions, String>? changedSettings;
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

        public ProfileSettings? CurrentProfileSettings { get; private set; }

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

            RTSS.Applications.Instance.Refresh();

            if (RTSS.Applications.Instance.FindForeground(out var processId, out var processName))
            {
                if (!BringUpProcess(processId))
                    AddProcess(processId, processName);
            }

            foreach (var process in watchedProcesses)
            {
                if (RTSS.Applications.Instance.IsRunning(process.Key))
                    continue;
                RemoveProcess(process.Key);
            }
        }

        private bool BringUpProcess(int processId)
        {
            if (!watchedProcesses.TryGetValue(processId, out var profileSettings))
                return false;

            if (CurrentProfileSettings != profileSettings)
            {
                Log.TraceLine("ProfilesController: Foreground changed: {0} => {1}",
                    CurrentProfileSettings?.ProfileName, profileSettings.ProfileName);
                CurrentProfileSettings = profileSettings;
                ProfileChanged();
            }
            return true;
        }

        private void AddProcess(int processId, string processName)
        {
            Log.TraceLine("ProfilesController: New Process: {0}/{1}", processId, processName);

            if (changedSettings == null)
                changedSettings = new Dictionary<MenuItemWithOptions, string>();

            var profileSettings = new ProfileSettings(processName);
            watchedProcesses.Add(processId, profileSettings);

            ApplyProfile(profileSettings);
        }

        private void RemoveProcess(int processId)
        {
            if (!watchedProcesses.Remove(processId, out var profileSettings))
                return;

            if (CurrentProfileSettings == profileSettings)
                CurrentProfileSettings = null;

            Log.TraceLine("ProfilesController: Removed Process: {0}", processId);

            if (watchedProcesses.Any())
                return;

            ResetProfile();
        }

        private void Root_OnOptionValueChange(MenuItemWithOptions options, string? oldValue, string newValue)
        {
            if (options.PersistentKey is null)
                return;

            if (oldValue is not null)
            {
                if (changedSettings?.TryAdd(options, oldValue) == true)
                {
                    Log.TraceLine("ProfilesController: Saved change: {0} from {1}", options.PersistentKey, oldValue);
                }
            }

            // If profile exists persist value
            if (CurrentProfileSettings != null && (CurrentProfileSettings.Exists || AutoCreateProfiles))
            {
                CurrentProfileSettings.SetValue(options.PersistentKey, newValue);
                options.ProfileOption = newValue;

                Log.TraceLine("ProfilesController: Stored: {0} {1} = {2}",
                    CurrentProfileSettings.ProfileName, options.PersistentKey, newValue);
            }
        }

        private void ProfileChanged()
        {
            foreach (var menuItem in PersistableOptions())
            {
                menuItem.ProfileOption = CurrentProfileSettings?.GetValue(menuItem.PersistentKey ?? "");
            }
        }

        public void CreateProfile(bool saveAll = true)
        {
            var profileSettings = CurrentProfileSettings;

            profileSettings?.TouchFile();

            Log.TraceLine("ProfilesController: Created Profile: {0}, SaveAll={1}",
                profileSettings?.ProfileName, saveAll);

            if (!saveAll)
                return;

            foreach (var menuItem in PersistableOptions())
            {
                if (menuItem.ActiveOption is null || !menuItem.PersistOnCreate)
                    continue;
                profileSettings?.SetValue(menuItem.PersistentKey ?? "", menuItem.ActiveOption);
            }

            ProfileChanged();
        }

        public void DeleteProfile()
        {
            CurrentProfileSettings?.DeleteFile();
            ProfileChanged();

            Log.TraceLine("ProfilesController: Deleted Profile: {0}", CurrentProfileSettings?.ProfileName);
        }

        private void ApplyProfile(ProfileSettings profileSettings)
        {
            CurrentProfileSettings = profileSettings;
            ProfileChanged();

            if (CurrentProfileSettings is null || CurrentProfileSettings?.Exists != true)
                return;

            int delay = CurrentProfileSettings.GetInt("ApplyDelay", ApplyProfileDelayMs);

            changeTask?.Cancel();
            changeTask = Dispatcher.RunWithDelay(delay, () =>
            {
                foreach (var menuItem in PersistableOptions())
                {
                    var persistedValue = CurrentProfileSettings.GetValue(menuItem.PersistentKey ?? "");
                    if (persistedValue is null)
                        continue;

                    try
                    {
                        menuItem.Set(persistedValue, true, false);

                        Log.TraceLine("ProfilesController: Applied from Profile: {0}: {1} = {2}",
                            CurrentProfileSettings.ProfileName, menuItem.PersistentKey, persistedValue);
                    }
                    catch (Exception e)
                    {
                        Log.TraceLine("ProfilesController: Exception Profile: {0}: {1} = {2} => {3}",
                            CurrentProfileSettings.ProfileName, menuItem.PersistentKey, persistedValue, e);

                        CurrentProfileSettings.DeleteKey(menuItem.PersistentKey ?? "");
                        menuItem.ProfileOption = null;
                    }
                }
            });
        }

        private void ResetProfile()
        {
            CurrentProfileSettings = null;
            ProfileChanged();

            if (changedSettings is null)
                return;

            // Revert all changes made to original value
            var appliedSettings = changedSettings;
            changedSettings = null;

            changeTask?.Cancel();
            changeTask = Dispatcher.RunWithDelay(ResetProfileDelayMs, () =>
            {
                foreach (var menuItem in PersistableOptions())
                {
                    if (!appliedSettings.TryGetValue(menuItem, out var setting))
                        continue;

                    try
                    {
                        menuItem.Set(setting, true, true);

                        Log.TraceLine("ProfilesController: Reset: {0} = {1}",
                            menuItem.PersistentKey, setting);
                    }
                    catch (Exception e)
                    {
                        Log.TraceLine("ProfilesController: Reset Exception: {0} = {1} => {2}",
                            menuItem.PersistentKey, setting, e);
                    }
                }
            });
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
