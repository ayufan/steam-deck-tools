using Microsoft.Win32;

namespace CommonHelpers
{
    public class WindowsDarkMode
    {
        private static System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer()
        {
            Interval = 1500
        };

        public static bool IsDarkModeEnabled { get; private set; }

        static WindowsDarkMode()
        {

            IsDarkModeEnabled = FetchIsDarkModeEnabled();

            timer.Tick += delegate{ IsDarkModeEnabled = FetchIsDarkModeEnabled(); };
            timer.Start();
        }

        private static bool FetchIsDarkModeEnabled()
        {
            try
            {
                string RegistryKey = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
                int theme = (int?)Registry.GetValue(RegistryKey, "SystemUsesLightTheme", 1) ?? 1;
                return theme == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
