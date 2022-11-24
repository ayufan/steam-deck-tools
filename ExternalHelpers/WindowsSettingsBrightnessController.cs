using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace PowerControl.Helpers
{
    /// <summary>
    /// Taken from: https://gist.github.com/maxkoshevoi/b8a1ad91f4d2a9fd3931168c14080694
    /// </summary>
    public static class WindowsSettingsBrightnessController
    {
        public static void Increase(int brightness)
        {
            var current = Get();
            current += brightness;
            current = Math.Clamp(current, 0, 100);
            Set(current);
        }

        public static int Get(double roundValue = 10.0)
        {
            return (int)(Math.Round(Get() / roundValue) * roundValue);
        }

        public static int Get()
        {
            try
            {
                using var mclass = new ManagementClass("WmiMonitorBrightness")
                {
                    Scope = new ManagementScope(@"\\.\root\wmi")
                };
                using var instances = mclass.GetInstances();
                foreach (ManagementObject instance in instances)
                {
                    return (byte)instance.GetPropertyValue("CurrentBrightness");
                }
                return -1;
            }
            catch
            {
                return -1;
            }
        }

        public static void Set(int brightness)
        {
            using var mclass = new ManagementClass("WmiMonitorBrightnessMethods")
            {
                Scope = new ManagementScope(@"\\.\root\wmi")
            };
            using var instances = mclass.GetInstances();
            var args = new object[] { 1, brightness };
            foreach (ManagementObject instance in instances)
            {
                instance.InvokeMethod("WmiSetBrightness", args);
            }
        }
    }
}
