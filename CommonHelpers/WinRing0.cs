using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers
{
    public class WinRing0
    {
        public const Int32 OLS_DLL_NO_ERROR = 0;
        public const Int32 OLS_DLL_UNSUPPORTED_PLATFORM = 1;
        public const Int32 OLS_DLL_DRIVER_NOT_LOADED = 2;
        public const Int32 OLS_DLL_DRIVER_NOT_FOUND = 3;
        public const Int32 OLS_DLL_DRIVER_UNLOADED = 4;
        public const Int32 OLS_DLL_DRIVER_NOT_LOADED_ON_NETWORK = 5;
        public const Int32 OLS_DLL_UNKNOWN_ERROR = 9;

        public const Int32 OLS_DRIVER_TYPE_UNKNOWN = 0;
        public const Int32 OLS_DRIVER_TYPE_WIN_9X = 1;
        public const Int32 OLS_DRIVER_TYPE_WIN_NT = 2;
        public const Int32 OLS_DRIVER_TYPE_WIN_NT4 = 3;
        public const Int32 OLS_DRIVER_TYPE_WIN_NT_X64 = 4;
        public const Int32 OLS_DRIVER_TYPE_WIN_NT_IA64 = 5;// Reseved

        public const UInt32 OLS_ERROR_PCI_BUS_NOT_EXIST = (0xE0000001);
        public const UInt32 OLS_ERROR_PCI_NO_DEVICE = (0xE0000002);
        public const UInt32 OLS_ERROR_PCI_WRITE_CONFIG = (0xE0000003);
        public const UInt32 OLS_ERROR_PCI_READ_CONFIG = (0xE0000004);

        public const UInt32 NO_DEVICE = 0xFFFFFFFF;

        public static UInt32 PciBusDevFunc(UInt32 Bus, UInt32 Dev, UInt32 Func)
        {
            return ((Bus & 0xFF) << 8) | ((Dev & 0x1F) << 3) | (Func & 7);
        }

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern UInt32 GetDllStatus();

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern UInt32 GetDllVersion(out byte major, out byte minor, out byte revision, out byte release);

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern UInt32 GetDriverVersion(out byte major, out byte minor, out byte revision, out byte release);

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern UInt32 GetDriverType();

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern bool InitializeOls();

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern void DeinitializeOls();

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern bool IsCpuid();

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern bool IsMsr();

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern bool IsTsc();

        #region PCI Bus
        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern void SetPciMaxBusIndex(byte max);

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern byte ReadPciConfigByte(UInt32 pci, byte address);

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern UInt16 ReadPciConfigWord(UInt32 pci, byte address);

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern UInt32 ReadPciConfigDword(UInt32 pci, byte address);

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern bool ReadPciConfigByteEx(UInt32 pci, byte address, out byte value);

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern bool ReadPciConfigWordEx(UInt32 pci, byte address, out UInt16 value);

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern bool ReadPciConfigDwordEx(UInt32 pci, byte address, out UInt32 value);

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern void WritePciConfigByte(UInt32 pci, byte address, byte value);

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern void WritePciConfigWord(UInt32 pci, byte address, UInt16 value);

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern void WritePciConfigDword(UInt32 pci, byte address, UInt32 value);

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern bool WritePciConfigByteEx(UInt32 pci, byte address, byte value);

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern bool WritePciConfigWordEx(UInt32 pci, byte address, UInt16 value);

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern bool WritePciConfigDwordEx(UInt32 pci, byte address, UInt32 value);

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern UInt32 FindPciDeviceById(UInt16 vendor, UInt16 device, byte index);

        [DllImport("WinRing0x64.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern UInt32 FindPciDeviceByClass(byte baseClass, byte subClass, byte programIf, byte index);
        #endregion
    }
}
