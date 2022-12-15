using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CommonHelpers
{
    public static class Dependencies
    {
        public static string[] Hidapi = new string[]
        {
            "hidapi.dll"
        };

        public static string[] RTSSShared = new string[]
        {
            "RTSSSharedMemoryNET.dll"
        };

        private static string[] VCRuntime = new string[]
        {
            "vcruntime140.dll"
        };

        private static string[] RTSS = new string[]
        {
            "C:\\Program Files (x86)\\RivaTuner Statistics Server\\RTSSHooks64.dll"
        };

        private static string VCRuntimeURL = "https://aka.ms/vs/17/release/vc_redist.x64.exe";
        private static string RTSSURL = "https://www.guru3d.com/files-details/rtss-rivatuner-statistics-server-download.html";

        public static void ValidateHidapi(string title)
        {
            ValidateVCRuntime(title);
            ValidateDependency(title, "HidAPI", Hidapi, false);
        }

        public static void ValidateRTSSSharedMemoryNET(string title)
        {
            ValidateVCRuntime(title);
            ValidateDependency(title, "RTSSSharedMemoryNET", RTSSShared, false);
        }

        public static void ValidateRTSS(string title)
        {
            InstallDependency(title, "Rivatuner Statistics Server", RTSS, RTSSURL, false, false);
        }

        private static void ValidateVCRuntime(string title)
        {
            InstallDependency(title, "Microsoft Visual C++ Runtime", VCRuntime, VCRuntimeURL, true, false);
        }

        private static void ValidateDependency(string title, string name, string[] dllNames, bool unload = true)
        {
            if (TryToLoad(dllNames, unload))
                return;

            Log.TraceError("Cannot load: {0}", dllNames);

            MessageBox.Show(
                "Cannot load: " + string.Join(", ", dllNames) + ".\n\n" +
                "Application will exit.\n",
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );

            Environment.Exit(1);
        }

        private static void InstallDependency(string title, string name, string[] dllNames, string url, bool required = true, bool unload = true)
        {
            if (TryToLoad(dllNames, unload))
                return;

            Log.TraceError("Cannot load: {0}", dllNames);

            var result = MessageBox.Show(
                "Missing '" + name + "' (" + string.Join(", ", dllNames) + ").\n\n" +
                "Click Yes to download and install?\n",
                title,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error
            );

            if (result == DialogResult.Yes)
            {
                ExecuteLink(url);
                Environment.Exit(1);
            }

            if (required)
            {
                MessageBox.Show(
                    "The '" + name + "' is required. " +
                    "Application will exit now. " +
                    "Once installed start application again.",
                    title,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                Environment.Exit(1);
            }
        }

        private static bool TryToLoad(string[] dllNames, bool unload = true)
        {
            foreach (var dllName in dllNames)
            {
                if (!TryToLoad(dllName, unload))
                    return false;
            }
            return true;
        }

        private static bool TryToLoad(string dllName, bool unload = true)
        {
            var handle = LoadLibrary(dllName);
            if (unload)
                FreeLibrary(handle);
            return handle != IntPtr.Zero;
        }

        private static void ExecuteLink(string link)
        {
            try { Process.Start("explorer.exe", link); }
            catch { }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr module);
    }
}
