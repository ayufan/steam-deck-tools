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
        public static String UserProfilesPath
        {
            get
            {
                var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var exeFolder = Path.GetDirectoryName(exePath) ?? Directory.GetCurrentDirectory();
                var exeGameProfiles = Path.Combine(exeFolder, "GameProfiles");
                if (!Directory.Exists(exeGameProfiles))
                    Directory.CreateDirectory(exeGameProfiles);
                return exeGameProfiles;
            }
        }

        public String ProfileName { get; }

        public ProfileSettings(string prefix, string profileName) : base("PersistentSettings")
        {
            this.ProfileName = profileName;
            this.ConfigFile = Path.Combine(UserProfilesPath, String.Format("{0}.{1}.ini", prefix, profileName));

            this.SettingChanging += delegate { };
            this.SettingChanged += delegate { };
        }

        public String? GetValue(string key)
        {
            var result = base.Get(key, String.Empty);
            if (result == String.Empty)
                return null;
            return result;
        }

        public int GetInt(string key, int defaultValue)
        {
            return base.Get(key, defaultValue);
        }

        public void SetValue(string key, string value)
        {
            base.Set(key, value);
        }
    }
}
