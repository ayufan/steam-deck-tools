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
        private static string profilesPath = "Profiles";

        static ProfileSettings()
        {
            Directory.CreateDirectory(profilesPath);
        }

        public ProfileSettings(string profileName) : base("Profile")
        {
            this.TouchSettings = true;
            SetRelativeConfigFile(Path.Combine(profilesPath, profileName + ".ini"));
        }

        public T Get<T>(string key, T defaultValue)
        {
            return base.Get(key, defaultValue);
        }

        public new bool Set<T>(string key, T value)
        {
            return base.Set(key, value);
        }

        public bool Exist
        {
            get { return File.Exists(this.ConfigFile); }
        }
    }
}
