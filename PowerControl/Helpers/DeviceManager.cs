using PowerControl.External;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PowerControl.Helpers
{
    internal class DeviceManager
    {
        public static string[]? GetDevices(Guid? classGuid)
        {
            string? filter = null;
            int flags = CM_GETIDLIST_FILTER_PRESENT;

            if (classGuid is not null)
            {
                filter = classGuid?.ToString("B").ToUpper();
                flags |= CM_GETIDLIST_FILTER_CLASS;
            }

            var res = CM_Get_Device_ID_List_Size(out var size, filter, flags);
            if (res != CR_SUCCESS)
                return null;

            char[] data = new char[size];
            res = CM_Get_Device_ID_List(filter, data, size, flags);
            if (res != CR_SUCCESS)
                return null;

            var result = new string(data);
            var devices = result.Split('\0', StringSplitOptions.RemoveEmptyEntries);
            return devices.ToArray();
        }

        public static string? GetDeviceDesc(String PNPString)
        {
            if (CM_Locate_DevNode(out var devInst, PNPString, 0) != 0)
                return null;

            if (!CM_Get_DevNode_Property(devInst, DEVPKEY_Device_DeviceDesc, out var deviceDesc, 0))
                return null;

            return deviceDesc;
        }

        public static IList<Tuple<UIntPtr, UIntPtr>>? GetDeviceMemResources(string PNPString)
        {
            int res = CM_Locate_DevNode(out var devInst, PNPString, 0);
            if (res != CR_SUCCESS)
                return null;

            res = CM_Get_First_Log_Conf(out var logConf, devInst, ALLOC_LOG_CONF);
            if (res != CR_SUCCESS)
                res = CM_Get_First_Log_Conf(out logConf, devInst, BOOT_LOG_CONF);
            if (res != CR_SUCCESS)
                return null;

            var ranges = new List<Tuple<UIntPtr, UIntPtr>>();

            while (CM_Get_Next_Res_Des(out var newResDes, logConf, ResType_Mem, out _, 0) == 0)
            {
                CM_Free_Res_Des_Handle(logConf);
                logConf = newResDes;

                if (!CM_Get_Res_Des_Data<MEM_RESOURCE>(logConf, out var memResource, 0))
                    continue;

                ranges.Add(new Tuple<UIntPtr, UIntPtr>(
                    memResource.MEM_Header.MD_Alloc_Base, memResource.MEM_Header.MD_Alloc_End));
            }

            CM_Free_Res_Des_Handle(logConf);
            return ranges;
        }

        static bool CM_Get_DevNode_Property(IntPtr devInst, DEVPROPKEY propertyKey, out string result, int flags)
        {
            result = default;

            // int length = 0;
            // int res = CM_Get_DevNode_Property(devInst, ref propertyKey, out var propertyType, null, ref length, flags);
            // if (res != CR_SUCCESS && res != CR_BUFFER_TOO_SMALL)
            //     return false;

            char[] buffer = new char[2048];
            int length = buffer.Length;
            int res = CM_Get_DevNode_Property(devInst, ref propertyKey, out var propertyType, buffer, ref length, flags);
            if (res != CR_SUCCESS)
                return false;
            if (propertyType != DEVPROP_TYPE_STRING)
                return false;

            result = new String(buffer, 0, length).Split('\0').First();
            return true;
        }

        static bool CM_Get_Res_Des_Data<T>(IntPtr rdResDes, out T buffer, int ulFlags) where T : struct
        {
            buffer = default;

            int res = CM_Get_Res_Des_Data_Size(out var size, rdResDes, ulFlags);
            if (res != CR_SUCCESS)
                return false;

            int sizeOf = Marshal.SizeOf<T>();
            if (sizeOf < size)
                return false;

            var addr = Marshal.AllocHGlobal(sizeOf);
            try
            {
                res = CM_Get_Res_Des_Data(rdResDes, addr, size, 0);
                if (res != CR_SUCCESS)
                    return false;

                buffer = Marshal.PtrToStructure<T>(addr);
                return true;
            }
            finally
            {
                Marshal.FreeHGlobal(addr);
            }
        }

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        static extern int CM_Locate_DevNode(out IntPtr pdnDevInst, string pDeviceID, int ulFlags);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode)]
        static extern int CM_Get_Device_ID_List_Size(out int idListlen, string? filter, int ulFlags);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode)]
        static extern int CM_Get_Device_ID_List(string? filter, char[] bffr, int bffrLen, int ulFlags);

        [DllImport("CfgMgr32.dll", CharSet = CharSet.Unicode)]
        static extern int CM_Get_DevNode_Property(IntPtr devInst, ref DEVPROPKEY propertyKey, out int propertyType, char[]? bffr, ref int bffrLen, int flags);

        [DllImport("setupapi.dll")]
        static extern int CM_Free_Res_Des_Handle(IntPtr rdResDes);

        [DllImport("setupapi.dll")]
        static extern int CM_Get_First_Log_Conf(out IntPtr rdResDes, IntPtr pdnDevInst, int ulFlags);

        [DllImport("setupapi.dll")]
        static extern int CM_Get_Next_Res_Des(out IntPtr newResDes, IntPtr rdResDes, int resType, out int resourceID, int ulFlags);

        [DllImport("setupapi.dll")]
        static extern int CM_Get_Res_Des_Data_Size(out int size, IntPtr rdResDes, int ulFlags);

        [DllImport("setupapi.dll")]
        static extern int CM_Get_Res_Des_Data(IntPtr rdResDes, IntPtr buffer, int size, int ulFlags);

        [StructLayout(LayoutKind.Sequential)]
        struct MEM_DES
        {
            internal uint MD_Count;
            internal uint MD_Type;
            internal UIntPtr MD_Alloc_Base;
            internal UIntPtr MD_Alloc_End;
            internal uint MD_Flags;
            internal uint MD_Reserved;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct MEM_RANGE
        {
            internal UIntPtr MR_Align;     // specifies mask for base alignment
            internal uint MR_nBytes;    // specifies number of bytes required
            internal UIntPtr MR_Min;       // specifies minimum address of the range
            internal UIntPtr MR_Max;       // specifies maximum address of the range
            internal uint MR_Flags;     // specifies flags describing range (fMD flags)
            internal uint MR_Reserved;
        };

        [StructLayout(LayoutKind.Sequential)]
        struct MEM_RESOURCE
        {
            internal MEM_DES MEM_Header;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            internal MEM_RANGE[] MEM_Data;
        };

        [StructLayout(LayoutKind.Sequential)]
        struct DEVPROPKEY
        {
            public Guid Guid;
            public uint Pid;

            public DEVPROPKEY(String guid, uint pid)
            {
                this.Guid = new Guid(guid);
                this.Pid = pid;
            }
        };

        const int ALLOC_LOG_CONF = 0x00000002;  // Specifies the Alloc Element.
        const int BOOT_LOG_CONF = 0x00000003;  // Specifies the RM Alloc Element.
        const int ResType_Mem = (0x00000001);  // Physical address resource

        const int CM_GETIDLIST_FILTER_PRESENT = 0x00000100;
        const int CM_GETIDLIST_FILTER_CLASS = 0x00000200;
        const int CR_SUCCESS = 0x0;
        const int CR_BUFFER_TOO_SMALL = 0x1A;

        const int DEVPROP_TYPE_STRING = 0x00000012;

        static readonly DEVPROPKEY DEVPKEY_Device_DeviceDesc = new DEVPROPKEY("a45c254e-df1c-4efd-8020-67d146a850e0", 2);

        internal static readonly Guid GUID_DISPLAY = new Guid("{4d36e968-e325-11ce-bfc1-08002be10318}");
    }
}
