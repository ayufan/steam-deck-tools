using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerControl.Helpers.GPU
{
    internal class AMDAdrenaline
    {
        // TODO: This CLSID is likely to change over time and be broken
        // pnputil /enum-devices /class Display
        const string GPUDriverKey = "SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}\\0000";
        const string DriverDesc = "DriverDesc";
        const string ExpectedDriverDesc = "AMD Custom GPU 0405";
        const string GPUScaling = "GPUScaling00";

        internal static bool IsGPUScalingEnabled()
        {
            try
            {
                var registry = Registry.LocalMachine.OpenSubKey(GPUDriverKey);
                if (registry == null)
                    return false;

                var driverDesc = registry.GetValue(DriverDesc);
                if (driverDesc is string && (string)driverDesc != ExpectedDriverDesc)
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
