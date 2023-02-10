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
            "RTSSHooks64.dll"
        };

        public static string SDTURL = "https://steam-deck-tools.ayufan.dev";
        public static string VCRuntimeURL = "https://aka.ms/vs/17/release/vc_redist.x64.exe";
        public static string RTSSURL = "https://www.guru3d.com/files-details/rtss-rivatuner-statistics-server-download.html";

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

        public static bool EnsureRTSS(string? title = null)
        {
            string? libraryPath = null;

            try
            {
                libraryPath = Microsoft.Win32.Registry.GetValue(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Unwinder\RTSS",
                    "InstallDir",
                    null
                ) as string;
            }
            catch
            {
            }

            return EnsureDependency(title, "Rivatuner Statistics Server", RTSS, libraryPath, RTSSURL, false);
        }

        private static void ValidateVCRuntime(string title)
        {
            InstallDependency(title, "Microsoft Visual C++ Runtime", VCRuntime, null, VCRuntimeURL, true, false);
        }

        private static void ValidateDependency(string title, string name, string[] dllNames, bool unload = true)
        {
            if (TryToLoad(dllNames, null, unload))
                return;

            Log.TraceError("Cannot load: {0}", dllNames);

            var result = ShowDialog(
                title,
                name,
                "Failure in loading the " + string.Join(", ", dllNames) + ".\n\n" +
                "This is required dependency. Application will exit.\n",
                null,
                TaskDialogButton.Close
            );

            Environment.Exit(1);
        }

        private static bool EnsureDependency(string? title, string name, string[] dllNames, string? libraryPath, string url, bool required = true)
        {
            if (TryToLoad(dllNames, libraryPath, false))
                return true;

            if (title == null)
                return false;

            Log.TraceError("Cannot load: {0}", dllNames);

            var downloadButton = new TaskDialogButton("Download");

            var result = ShowDialog(
                title,
                name,
                "Failure in loading the " + string.Join(", ", dllNames) + ".\n\n" +
                "The '" + name + "' is likely not installed.\n" +
                "Click Download to download it.\n",
                url,
                downloadButton,
                TaskDialogButton.Ignore
            );

            if (result == downloadButton)
                OpenLink(url);

            return false;
        }

        private static void InstallDependency(string title, string name, string[] dllNames, string? libraryPath, string url, bool required = true, bool unload = true)
        {
            if (TryToLoad(dllNames, libraryPath, unload))
                return;

            Log.TraceError("Cannot load: {0}", dllNames);

            var downloadButton = new TaskDialogButton("Download");
            var exitButton = new TaskDialogButton("Exit");

            var result = ShowDialog(
                title,
                name,
                "Failure in loading the " + string.Join(", ", dllNames) + ".\n\n" +
                "The '" + name + "' is likely not installed.\n" +
                "Click Download to download it.\n" +
                (required ? "Once installed start application again.\n" : ""),
                url,
                downloadButton,
                required ? exitButton : TaskDialogButton.Ignore
            );

            if (result == downloadButton)
            {
                OpenLink(url);
                Environment.Exit(1);
            }
            else if (result == exitButton)
            {
                Environment.Exit(1);
            }
        }

        private static bool TryToLoad(string[] dllNames, string? libraryPath, bool unload = true)
        {
            foreach (var dllName in dllNames)
            {
                if (!TryToLoad(dllName, libraryPath, unload))
                    return false;
            }
            return true;
        }

        private static bool IsLoaded(string dllName)
        {
            return GetModuleHandle(dllName) != IntPtr.Zero;
        }

        private static bool TryToLoad(string dllName, String? libraryPath = null, bool unload = true)
        {
            if (IsLoaded(dllName))
                return true;

            var handle = LoadLibrary(dllName);
            if (handle == IntPtr.Zero && libraryPath is not null)
                handle = LoadLibrary(Path.Join(libraryPath, dllName));
            if (unload)
                FreeLibrary(handle);
            return handle != IntPtr.Zero;
        }

        private static TaskDialogButton ShowDialog(string caption, string heading, string text, string? url, params TaskDialogButton[] buttons)
        {
            var page = new TaskDialogPage();
            page.Caption = caption;
            foreach (var button in buttons)
                page.Buttons.Add(button);
            page.Icon = TaskDialogIcon.ShieldWarningYellowBar;
            page.Heading = heading;
            page.Text = text;
            if (page.Buttons.Contains(TaskDialogButton.Help) && url is not null)
            {
                page.Footnote = new TaskDialogFootnote("Click help to download it.");
                page.Footnote.Icon = TaskDialogIcon.Information;

                page.HelpRequest += delegate
                {
                    try { OpenLink(url); }
                    catch { }
                };
            }

            return TaskDialog.ShowDialog(new Form { TopMost = true }, page, TaskDialogStartupLocation.CenterScreen);
        }

        public static void OpenLink(string link)
        {
            try { Process.Start("explorer.exe", link); }
            catch { }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr module);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string moduleName);
    }
}
