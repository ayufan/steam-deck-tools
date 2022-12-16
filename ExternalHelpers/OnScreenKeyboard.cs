using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace ExternalHelpers
{
    public static class OnScreenKeyboard
    {
        public const String TabTipPath = @"C:\Program Files\Common Files\Microsoft Shared\ink\TabTip.exe";

        public static bool Toggle()
        {
            try
            {
                StartTabTip();

                var type = Type.GetTypeFromCLSID(Guid.Parse("4ce576fa-83dc-4F88-951c-9d0782b4e376"));
                if (type is null)
                    return false;
                var instance = (ITipInvocation?)Activator.CreateInstance(type);
                if (instance is null)
                    return false;
                instance?.Toggle(GetDesktopWindow());
                Marshal.ReleaseComObject(instance);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        static void StartTabTip()
        {
            if (FindWindow("IPTIP_Main_Window", "") != IntPtr.Zero)
                return;

            Process.Start(TabTipPath);

            for (int i = 0; i < 10 && FindWindow("IPTIP_Main_Window", "") == IntPtr.Zero; i++)
                Thread.Sleep(100);
        }

        [ComImport, Guid("4ce576fa-83dc-4F88-951c-9d0782b4e376")]
        class UIHostNoLaunch
        {
        }

        [ComImport, Guid("37c994e7-432b-4834-a2f7-dce1f13b834b")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface ITipInvocation
        {
            void Toggle(IntPtr hwnd);
        }

        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }
}
