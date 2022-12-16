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
    }

    public static class GameProfilesController
    {
        public static string CurrentGame { get; private set; } = string.Empty;
        public static GameProfile CurrentProfile { get; private set; }

        private static string profilesPath = Path.Combine(Directory.GetCurrentDirectory(), "Profiles");
        private static DirectoryInfo profilesDirectory;
        private static bool isSingleDisplay = false;

        static GameProfilesController()
        {
            profilesDirectory = Directory.CreateDirectory(profilesPath);
            CurrentProfile = GetDefaultProfile();
        }

        public static bool UpdateGameProfile()
        {
            int displays = DeviceManager.GetActiveDisplays();

            if (!isSingleDisplay && displays != 1)
            {
                return false;
            }

            if (isSingleDisplay && displays != 1)
            {
                CurrentGame = string.Empty;
                CurrentProfile = new GameProfile(string.Empty, 3, 0);
                isSingleDisplay = false;

                // Let windows handle monitor plugin
                Thread.Sleep(1000);

                return true;
            }

            if (!isSingleDisplay && displays == 1)
            {
                isSingleDisplay = true;

                // Let windows handle monitor unplug
                Thread.Sleep(1500);

                return false;
            }

            string? runningGame = RTSS.GetCurrentGameName();

            if (runningGame == null && CurrentGame != GameProfile.DefaultName)
            {
                CurrentGame = GameProfile.DefaultName;
                CurrentProfile = GetDefaultProfile();

                return true;
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

                return true;
            }

            return false;
        }

        public static void SetValueByKey(GameOptions key, int value)
        {
            switch (key)
            {
                case GameOptions.Fps:
                    CurrentProfile.fps = value;
                    break;
                case GameOptions.RefreshRate:
                    CurrentProfile.refreshRate = value;
                    break;
            }

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

        public static void WriteProfile(GameProfile profile)
        {
            string fileName = string.Format("{0}\\{1}.json", profilesPath, profile.name);
            string jsonString = JsonSerializer.Serialize<GameProfile>(profile, new JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText(fileName, jsonString);
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
    }
}
