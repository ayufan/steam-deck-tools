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
    }

    public static class GameProfilesController
    {
        public static string CurrentGame { get; private set; }
        public static GameProfile CurrentProfile { get; private set; }

        private static string profilesPath = Path.Combine(Directory.GetCurrentDirectory(), "Profiles");
        private static DirectoryInfo profilesDirectory;
        
        static GameProfilesController()
        {
            profilesDirectory = Directory.CreateDirectory(profilesPath);
            CurrentGame = GameProfile.DefaultName;
            CurrentProfile = GetDefaultProfile();
        }

        public static bool UpdateGameProfile()
        {
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

            WriteProfile(CurrentProfile);
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

            return new GameProfile(GameProfile.DefaultName, 3, getCurrentRefreshRateIndex());
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
