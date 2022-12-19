using System;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using Microsoft.Win32;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace ExternalHelpers
{
    // Taken from: https://stackoverflow.com/a/45548823
    public static class RegistryUtils
    {
        [DllImport("advapi32.dll", EntryPoint = "RegQueryInfoKey", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        private static extern int RegQueryInfoKey(
            UIntPtr hkey,
            out StringBuilder lpClass,
            ref uint lpcbClass,
            IntPtr lpReserved,
            out uint lpcSubKeys,
            out uint lpcbMaxSubKeyLen,
            out uint lpcbMaxClassLen,
            out uint lpcValues,
            out uint lpcbMaxValueNameLen,
            out uint lpcbMaxValueLen,
            out uint lpcbSecurityDescriptor,
            ref FILETIME lpftLastWriteTime);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegCloseKey(UIntPtr hKey);


        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern int RegOpenKeyEx(
          UIntPtr hKey,
          string subKey,
          int ulOptions,
          int samDesired,
          out UIntPtr hkResult);

        private static DateTimeOffset ToDateTime(FILETIME ft)
        {
            IntPtr buf = IntPtr.Zero;
            try
            {
                long[] longArray = new long[1];
                int cb = Marshal.SizeOf(ft);
                buf = Marshal.AllocHGlobal(cb);
                Marshal.StructureToPtr(ft, buf, false);
                Marshal.Copy(buf, longArray, 0, 1);
                return DateTimeOffset.FromFileTime(longArray[0]);
            }
            finally
            {
                if (buf != IntPtr.Zero) Marshal.FreeHGlobal(buf);
            }
        }

        public static DateTimeOffset? GetDateModified(RegistryHive registryHive, string path)
        {
            var lastModified = new FILETIME();
            var lpcbClass = new uint();
            var lpReserved = new IntPtr();
            UIntPtr key = UIntPtr.Zero;

            try
            {
                try
                {
                    var hive = new UIntPtr(unchecked((uint)registryHive));
                    if (RegOpenKeyEx(hive, path, 0, (int)RegistryRights.ReadKey, out key) != 0)
                    {
                        return null;
                    }

                    uint lpcbSubKeys;
                    uint lpcbMaxKeyLen;
                    uint lpcbMaxClassLen;
                    uint lpcValues;
                    uint maxValueName;
                    uint maxValueLen;
                    uint securityDescriptor;
                    StringBuilder sb;
                    if (RegQueryInfoKey(
                                 key,
                                 out sb,
                                 ref lpcbClass,
                                 lpReserved,
                                 out lpcbSubKeys,
                                 out lpcbMaxKeyLen,
                                 out lpcbMaxClassLen,
                                 out lpcValues,
                                 out maxValueName,
                                 out maxValueLen,
                                 out securityDescriptor,
                                 ref lastModified) != 0)
                    {
                        return null;
                    }

                    var result = ToDateTime(lastModified);
                    return result;
                }
                finally
                {
                    if (key != UIntPtr.Zero)
                        RegCloseKey(key);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
