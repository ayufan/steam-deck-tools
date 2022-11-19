using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerControl.Helpers
{
    internal class AMD
    {
        // TODO: This CLSID is likely to change over time and be broken
        // pnputil /enum-devices /class Display
        const String GPUDriverKey = "SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}\\0000";
        const String DriverDesc = "DriverDesc";
        const String ExpectedDriverDesc = "AMD Custom GPU 0405";
        const String GPUScaling = "GPUScaling00";

        internal static bool IsGPUScalingEnabled()
        {
            try
            {
                var registry = Registry.LocalMachine.OpenSubKey(GPUDriverKey);
                if (registry == null)
                    return false;

                var driverDesc = registry.GetValue(DriverDesc);
                if (driverDesc is String && ((string)driverDesc) != ExpectedDriverDesc)
                    return false;

                var scalingBytes = registry.GetValue(GPUScaling);
                if (scalingBytes is not byte[])
                    return false;

                var scaling = BitConverter.ToUInt32((byte[])scalingBytes);
                return scaling == 1;
            }
            catch
            {
                return false;
            }
        }
    }
}
