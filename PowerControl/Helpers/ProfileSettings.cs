using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommonHelpers;

namespace PowerControl.Helper
{
    public class ProfileSettings : BaseSettings
    {
        private static string profilesPath = Path.Combine(Directory.GetCurrentDirectory(), "Profiles");

        static ProfileSettings()
        {
            Directory.CreateDirectory(profilesPath);
        }

        public ProfileSettings(string profileName) : base("Profile")
        {
            this.TouchSettings = true;
            this.ConfigFile = Path.Combine(profilesPath, profileName + ".ini");

            this.SettingChanging += delegate { };
            this.SettingChanged += delegate { };
        }

        public T Get<T>(string key, T defaultValue)
        {
            return base.Get(key, defaultValue);
        }

        public new bool Set<T>(string key, T value)
        {
            return base.Set(key, value);
        }

        public static bool CheckIfExists(string profileName)
        {
            foreach (FileInfo fi in Directory.CreateDirectory(profilesPath).GetFiles())
            {
                if (fi.Name[^4..].Equals(".ini") && fi.Name[..^4].Equals(profileName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
