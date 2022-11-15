using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers
{
    public class InpOut
    {
        [DllImport("inpoutx64.dll", EntryPoint = "MapPhysToLin", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr MapPhysToLin(IntPtr pbPhysAddr, uint dwPhysSize, out IntPtr pPhysicalMemoryHandle);

        [DllImport("inpoutx64.dll", EntryPoint = "UnmapPhysicalMemory", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnmapPhysicalMemory(IntPtr PhysicalMemoryHandle, IntPtr pbLinAddr);

        [DllImport("inpoutx64.dll", EntryPoint = "DlPortReadPortUchar", CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static extern byte DlPortReadPortUchar(ushort port);

        [DllImport("inpoutx64.dll", EntryPoint = "DlPortWritePortUchar", CallingConvention = CallingConvention.StdCall)]
        public static extern byte DlPortWritePortUchar(ushort port, byte vlaue);

        [DllImport("inpoutx64.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool GetPhysLong(IntPtr pbPhysAddr, out uint physValue);

        [DllImport("inpoutx64.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool SetPhysLong(IntPtr pbPhysAddr, uint physValue);

        public static byte[] ReadMemory(IntPtr baseAddress, uint size)
        {
            IntPtr pdwLinAddr = MapPhysToLin(baseAddress, size, out IntPtr pPhysicalMemoryHandle);
            if (pdwLinAddr != IntPtr.Zero)
            {
                byte[] bytes = new byte[size];
                Marshal.Copy(pdwLinAddr, bytes, 0, bytes.Length);
                UnmapPhysicalMemory(pPhysicalMemoryHandle, pdwLinAddr);

                return bytes;
            }

            return null;
        }

        public static bool WriteMemory(IntPtr baseAddress, byte[] data)
        {
            IntPtr pdwLinAddr = MapPhysToLin(baseAddress, (uint)data.Length, out IntPtr pPhysicalMemoryHandle);
            if (pdwLinAddr != IntPtr.Zero)
            {
                Marshal.Copy(data, 0, pdwLinAddr, data.Length);
                UnmapPhysicalMemory(pPhysicalMemoryHandle, pdwLinAddr);

                return true;
            }

            return false;
        }
    }
}
