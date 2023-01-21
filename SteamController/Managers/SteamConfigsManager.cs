using System.Diagnostics;
using CommonHelpers;
using SteamController.Helpers;

namespace SteamController.Managers
{
    public sealed class SteamConfigsManager : Manager
    {
        static readonly Dictionary<String, byte[]> lockedSteamControllerFiles = new Dictionary<string, byte[]>
        {
            // Use existing defaults in BasicUI and BigPicture
            // { "controller_base/basicui_neptune.vdf", Resources.basicui_neptune },
            // { "controller_base/bigpicture_neptune.vdf", Resources.bigpicture_neptune },
            { "controller_base/desktop_neptune.vdf", Resources.empty_neptune },
            { "controller_base/chord_neptune.vdf", Resources.chord_neptune }
        };
        static readonly Dictionary<String, byte[]> installedSteamControllerFiles = new Dictionary<string, byte[]>
        {
            { "controller_base/templates/controller_neptune_steamcontroller.vdf", Resources.empty_neptune },
        };

        private bool? filesLocked;

        public SteamConfigsManager()
        {
            // always unlock configs when changed
            Settings.Default.SettingChanging += UnlockControllerFiles;
            SetSteamControllerFilesLock(false);
        }

        private bool IsActive
        {
            get
            {
                return Settings.Default.SteamControllerConfigs == Settings.SteamControllerConfigsMode.Overwrite &&
                Settings.Default.EnableSteamDetection == true;
            }
        }

        public override void Dispose()
        {
            SetSteamControllerFilesLock(false);
            Settings.Default.SettingChanging -= UnlockControllerFiles;
        }

        private void UnlockControllerFiles(string key)
        {
            SetSteamControllerFilesLock(false);
        }

        public override void Tick(Context context)
        {
            if (!IsActive)
                return;

            bool running = Helpers.SteamConfiguration.IsRunning;
            if (running == filesLocked)
                return;

            SetSteamControllerFilesLock(running);
        }

        private void SetSteamControllerFilesLock(bool lockConfigs)
        {
            if (!IsActive)
                return;

            Log.TraceLine("SetSteamControllerFilesLock: {0}", lockConfigs);

            if (lockConfigs)
            {
                foreach (var config in lockedSteamControllerFiles)
                    Helpers.SteamConfiguration.OverwriteConfigFile(config.Key, config.Value, true);
                foreach (var config in installedSteamControllerFiles)
                    Helpers.SteamConfiguration.OverwriteConfigFile(config.Key, config.Value, false);
            }
            else
            {
                foreach (var config in lockedSteamControllerFiles)
                    Helpers.SteamConfiguration.ResetConfigFile(config.Key);
            }
            filesLocked = lockConfigs;
        }
    }
}
