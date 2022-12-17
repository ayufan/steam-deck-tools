using CommonHelpers;
using Sentry.Protocol;
using System.IO;
using System.Text.Json;

namespace PowerControl.Helpers
{
    public enum GameOptions
    {
        None,
        Fps,
        RefreshRate,
    }

    public class GameProfile
    {
        public static string DefaultName = "Default";
        public string name { get; set; }
        public int fps { get; set; } = 1;
        public int refreshRate { get; set; } = 0;
        // This option will be set automatically if the game is on a "naughty" list
        // or has to be changed in the json file by the user
        public bool isTroubled { get; set; } = false;

        public GameProfile(string name, int fps, int refreshRate)
        {
            this.name = name;
            this.fps = fps;
            this.refreshRate = refreshRate;
        }

        public static GameProfile Copy(GameProfile profile)
        {
            return new GameProfile(profile.name, profile.fps, profile.refreshRate);
        }

        public int? GetByKey(GameOptions key)
        {
            switch (key)
            {
                case GameOptions.Fps: return fps;
                case GameOptions.RefreshRate: return refreshRate;
            }

            return null;
        }

        public void SetByKey(GameOptions key, int value)
        {
            switch (key)
            {
                case GameOptions.Fps:
                    fps = value;
                    break;
                case GameOptions.RefreshRate:
                    refreshRate = value;
                    break;
            }
        }
    }

    public static class GameProfilesController
    {
        public static string CurrentGame { get; private set; } = string.Empty;
        public static GameProfile CurrentProfile { get; private set; }
        public static bool IsSingleDisplay { get; private set; } = false;
        public static bool HaveDisplaysChanged { get => IsSingleDisplay ^ (DeviceManager.GetActiveDisplays() == 1); }

        private static string profilesPath = Path.Combine(Directory.GetCurrentDirectory(), "Profiles");
        private static DirectoryInfo profilesDirectory;
        private static string[] troubledGames = { "dragonageinquisition" };
        private static Object syncUpdate = new();
        private static Object syncWrite = new();
        private static List<Action<GameProfile>> subscribers = new List<Action<GameProfile>>();

        static GameProfilesController()
        {
            profilesDirectory = Directory.CreateDirectory(profilesPath);
            CurrentProfile = GetDefaultProfile();
        }

        public static void UpdateGameProfile()
        {
            int displays = DeviceManager.GetActiveDisplays();

            lock (syncUpdate)
            {
                if (!IsSingleDisplay && displays != 1)
                {
                    return;
                }

                if (IsSingleDisplay && displays != 1)
                {
                    CurrentGame = string.Empty;
                    CurrentProfile = new GameProfile(string.Empty, 3, 0);
                    IsSingleDisplay = false;

                    // Let windows handle monitor plugin
                    Thread.Sleep(1000);

                    notify(CurrentProfile);
                    return;
                }

                if (!IsSingleDisplay && displays == 1)
                {
                    IsSingleDisplay = true;

                    // Let windows handle monitor unplug
                    Thread.Sleep(2500);

                    return;
                }

                string? runningGame = RTSS.GetCurrentGameName();

                if (runningGame == null && CurrentGame != GameProfile.DefaultName)
                {
                    CurrentGame = GameProfile.DefaultName;
                    CurrentProfile = GetDefaultProfile();

                    notify(CurrentProfile);
                    return;
                }

                if (runningGame != null && CurrentGame != runningGame)
                {
                    CurrentGame = runningGame;
                    GameProfile? gameProfile;

                    if (CheckIfProfileExists(CurrentGame) &&
                        (gameProfile = GetProfile(runningGame)) != null)
                    {
                        CurrentProfile = gameProfile;
                    }
                    else
                    {
                        CurrentProfile = GetDefaultProfile();
                    }

                    var profileCopy = GameProfile.Copy(CurrentProfile);
                    notify(profileCopy);
                    if (profileCopy.isTroubled)
                    {
                        // Fixes refresh rate reset for games tagged as troubled eg. Dragon Age Inquisition
                        Thread.Sleep(6500);

                        notify(profileCopy);
                    }

                    return;
                }
            }
        }

        public static void SetValueByKey(GameOptions key, int value)
        {
            CurrentProfile.SetByKey(key, value);

            if (CurrentGame != string.Empty)
            {
                WriteProfile(CurrentProfile);
            }
        }

        public static GameProfile GetDefaultProfile()
        {
            if (CheckIfProfileExists(GameProfile.DefaultName))
            {
                var profile = GetProfile(GameProfile.DefaultName);

                if (profile != null)
                {
                    return profile;
                }
            }

            var defaultProfile = new GameProfile(GameProfile.DefaultName, 3, getCurrentRefreshRateIndex());

            WriteProfile(defaultProfile);

            return defaultProfile;
        }

        public static bool CheckIfProfileExists(string name)
        {
            foreach (FileInfo fi in profilesDirectory.GetFiles())
            {
                if (fi.Name[^5..].Equals(".json") && fi.Name[..^5].Equals(name))
                {
                    return true;
                }
            }

            return false;
        }

        public static GameProfile? GetProfile(string name)
        {
            try
            {
                using (StreamReader r = new StreamReader(string.Format("{0}\\{1}.json", profilesPath, name)))
                {
                    string json = r.ReadToEnd();
                    
                    try
                    {
                        return JsonSerializer.Deserialize<GameProfile>(json);
                    }
                    catch
                    {
                        Log.TraceError("ERROR: Couldn't read {0} json profile", name);
                        return null;
                    }
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
        }

        public static GameProfile CreateProfile(string name)
        {
            var profile = GameProfile.Copy(GetDefaultProfile());
            profile.name = name;
            profile.isTroubled = troubledGames.Contains(name.ToLower());

            WriteProfile(profile);

            return profile;
        }

        public static void WriteProfile(GameProfile profile)
        {
            string fileName = string.Format("{0}\\{1}.json", profilesPath, profile.name);
            string jsonString = JsonSerializer.Serialize<GameProfile>(profile, new JsonSerializerOptions() { WriteIndented = true });
            
            lock (syncWrite)
            {
                File.WriteAllText(fileName, jsonString);
            }
        }

        public static void Subscribe(Action<GameProfile> action)
        {
            subscribers.Add(action);
        }

        private static int getCurrentRefreshRateIndex()
        {
            var refreshRates = DisplayResolutionController.GetRefreshRates();
            
            if (refreshRates != null)
            {
                return Array.IndexOf(refreshRates, DisplayResolutionController.GetRefreshRate());
            }

            return 0;
        }

        private static void notify(GameProfile profile)
        {
            

            foreach (var action in subscribers)
            {
                action.Invoke(profile);
            }
        }
    }
}
