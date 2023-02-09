#region Copyright

/*******************************************************************************
 Copyright(c) 2008 - 2022 Advanced Micro Devices, Inc. All Rights Reserved.
 Copyright (c) 2002 - 2006  ATI Technologies Inc. All Rights Reserved.
 
 THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
 ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDED BUT NOT LIMITED TO
 THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
 PARTICULAR PURPOSE.
 
 File:        ADL.cs
 
 Purpose:     Implements ADL interface 
 
 Description: Implements some of the methods defined in ADL interface.
              
 ********************************************************************************/

#endregion Copyright

#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using FARPROC = System.IntPtr;
using HMODULE = System.IntPtr;

#endregion Using

#region ATI.ADL

namespace PowerControl.Helpers.AMD
{
    #region Export Struct

    #region ADLAdapterInfo
    /// <summary> ADLAdapterInfo Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLAdapterInfo
    {
        /// <summary>The size of the structure</summary>
        int Size;
        /// <summary> Adapter Index</summary>
        internal int AdapterIndex;
        /// <summary> Adapter UDID</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        internal string UDID;
        /// <summary> Adapter Bus Number</summary>
        internal int BusNumber;
        /// <summary> Adapter Driver Number</summary>
        internal int DriverNumber;
        /// <summary> Adapter Function Number</summary>
        internal int FunctionNumber;
        /// <summary> Adapter Vendor ID</summary>
        internal int VendorID;
        /// <summary> Adapter Adapter name</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        internal string AdapterName;
        /// <summary> Adapter Display name</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        internal string DisplayName;
        /// <summary> Adapter Present status</summary>
        internal int Present;
        /// <summary> Adapter Exist status</summary>
        internal int Exist;
        /// <summary> Adapter Driver Path</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        internal string DriverPath;
        /// <summary> Adapter Driver Ext Path</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        internal string DriverPathExt;
        /// <summary> Adapter PNP String</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        internal string PNPString;
        /// <summary> OS Display Index</summary>
        internal int OSDisplayIndex;
    }


    /// <summary> ADLAdapterInfo Array</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLAdapterInfoArray
    {
        /// <summary> ADLAdapterInfo Array </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)ADL.ADL_MAX_ADAPTERS)]
        internal ADLAdapterInfo[] ADLAdapterInfo;
    }
    #endregion ADLAdapterInfo

    #region ADLDisplayInfo
    /// <summary> ADLDisplayID Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLDisplayID
    {
        /// <summary> Display Logical Index </summary>
        internal int DisplayLogicalIndex;
        /// <summary> Display Physical Index </summary>
        internal int DisplayPhysicalIndex;
        /// <summary> Adapter Logical Index </summary>
        internal int DisplayLogicalAdapterIndex;
        /// <summary> Adapter Physical Index </summary>
        internal int DisplayPhysicalAdapterIndex;
    }

    /// <summary> ADLDisplayInfo Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLDisplayInfo
    {
        /// <summary> Display Index </summary>
        internal ADLDisplayID DisplayID;
        /// <summary> Display Controller Index </summary>
        internal int DisplayControllerIndex;
        /// <summary> Display Name </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        internal string DisplayName;
        /// <summary> Display Manufacturer Name </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        internal string DisplayManufacturerName;
        /// <summary> Display Type : < The Display type. CRT, TV,CV,DFP are some of display types,</summary>
        internal int DisplayType;
        /// <summary> Display output type </summary>
        internal int DisplayOutputType;
        /// <summary> Connector type</summary>
        internal int DisplayConnector;
        ///<summary> Indicating the display info bits' mask.<summary>
        internal int DisplayInfoMask;
        ///<summary> Indicating the display info value.<summary>
        internal int DisplayInfoValue;
    }

    /// <summary> ADLAdapterInfo Array</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal class ADLDisplayInfoArray
    {
        /// <summary> ADLAdapterInfo Array </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)ADL.ADL_MAX_DISPLAYS)]
        internal ADLDisplayInfo[] ADLAdapterInfo;
    }
    #endregion ADLDisplayInfo

    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLDisplayEDIDData
    {
        internal int iSize;
        internal int iFlag;
        internal int iEDIDSize;
        internal int iBlockIndex;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)ADL.ADL_MAX_EDIDDATA_SIZE)]
        internal byte[] cEDIDData;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        internal int[] iReserved;
    };

    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLDisplayEDIDDataX2
    {
        internal int iSize;
        internal int iFlag;
        internal int iEDIDSize;
        internal int iIgnored;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)2 * ADL.ADL_MAX_EDIDDATA_SIZE)]
        internal byte[] cEDIDData;
    };

    #region Radeon Image Sharpening
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADL_RIS_SETTINGS
    {
        internal int GlobalEnable; //Global enable value
        internal int GlobalSharpeningDegree; //Global sharpening value
        internal int GlobalSharpeningDegree_MinLimit; //Gloabl sharpening slider min limit value
        internal int GlobalSharpeningDegree_MaxLimit; //Gloabl sharpening slider max limit value
        internal int GlobalSharpeningDegree_Step; //Gloabl sharpening step  value
    };

    [StructLayout(LayoutKind.Sequential)]
    internal struct ADL_RIS_NOTFICATION_REASON
    {
        internal int GlobalEnableChanged; //Set when Global enable value is changed
        internal int GlobalSharpeningDegreeChanged; //Set when Global sharpening Degree value is changed
    };
    #endregion ADLDisplayInfo

    #endregion Export Struct

    #region ADL Class
    /// <summary> ADL Class</summary>
    internal static class ADL
    {
        internal const int ADL_DEFAULT_ADAPTER = 0;
        internal const int ADL_DEFAULT_DISPLAY = 0;

        #region Internal Constant
        /// <summary> Define the maximum path</summary>
        internal const int ADL_MAX_PATH = 256;
        /// <summary> Define the maximum adapters</summary>
        internal const int ADL_MAX_ADAPTERS = 40 /* 150 */;
        /// <summary> Define the maximum displays</summary>
        internal const int ADL_MAX_DISPLAYS = 40 /* 150 */;
        /// <summary> Define the maximum device name length</summary>
        internal const int ADL_MAX_DEVICENAME = 32;
        /// <summary> Define the successful</summary>
        internal const int ADL_SUCCESS = 0;
        /// <summary> Define the failure</summary>
        internal const int ADL_FAIL = -1;
        /// <summary> Define the driver ok</summary>
        internal const int ADL_DRIVER_OK = 0;
        /// <summary> Maximum number of GL-Sync ports on the GL-Sync module </summary>
        internal const int ADL_MAX_GLSYNC_PORTS = 8;
        /// <summary> Maximum number of GL-Sync ports on the GL-Sync module </summary>
        internal const int ADL_MAX_GLSYNC_PORT_LEDS = 8;
        /// <summary> Maximum number of ADLMOdes for the adapter </summary>
        internal const int ADL_MAX_NUM_DISPLAYMODES = 1024;
        internal const int ADL_MAX_EDIDDATA_SIZE = 256;
        internal const int ADL_MAX_EDID_EXTENSION_BLOCKS = 3;

        internal const int ADL_DISPLAY_DISPLAYINFO_DISPLAYCONNECTED = 0x00000001;

        #endregion Internal Constant

        #region Internal Constant
        /// <summary> Atiadlxx_FileName </summary>
        internal const string Atiadlxx_FileName = "atiadlxx.dll";
        /// <summary> Kernel32_FileName </summary>
        internal const string Kernel32_FileName = "kernel32.dll";
        #endregion Internal Constant

        #region Export Delegates
        /// <summary> ADL Memory allocation function allows ADL to callback for memory allocation</summary>
        /// <param name="size">input size</param>
        /// <returns> retrun ADL Error Code</returns>
        internal delegate IntPtr ADL_Main_Memory_Alloc(int size);
        #endregion

        #region DLLImport
        [DllImport(Kernel32_FileName)]
        internal static extern HMODULE GetModuleHandle(string moduleName);

        [DllImport(Atiadlxx_FileName)]
        internal static extern int ADL2_Main_Control_Create(ADL_Main_Memory_Alloc callback, int enumConnectedAdapters, out IntPtr context);

        [DllImport(Atiadlxx_FileName)]
        internal static extern int ADL2_Main_Control_Destroy(IntPtr context);

        [DllImport(Atiadlxx_FileName)]
        internal static extern int ADL2_Main_Control_IsFunctionValid(IntPtr context, HMODULE module, string procName);

        [DllImport(Atiadlxx_FileName)]
        internal static extern int ADL2_Adapter_NumberOfAdapters_Get(IntPtr context, out int numAdapters);

        [DllImport(Atiadlxx_FileName)]
        internal static extern int ADL2_Adapter_AdapterInfo_Get(IntPtr context, out ADLAdapterInfoArray info, int inputSize);

        [DllImport(Atiadlxx_FileName)]
        internal static extern int ADL2_Adapter_Active_Get(IntPtr context, int adapterIndex, out int status);

        [DllImport(Atiadlxx_FileName)]
        internal static extern int ADL2_Display_DisplayInfo_Get(IntPtr context, int adapterIndex, out int numDisplays, out IntPtr displayInfoArray, int forceDetect);

        [DllImport(Atiadlxx_FileName)]
        internal static extern int ADL2_DFP_GPUScalingEnable_Get(IntPtr context, int adapterIndex, int displayIndex, out int support, out int current, out int default_);

        [DllImport(Atiadlxx_FileName)]
        internal static extern int ADL2_DFP_GPUScalingEnable_Set(IntPtr context, int adapterIndex, int displayIndex, int current);

        [DllImport(Atiadlxx_FileName)]
        internal static extern int ADL2_Display_PreservedAspectRatio_Get(IntPtr context, int adapterIndex, int displayIndex, out int support, out int current, out int default_);

        [DllImport(Atiadlxx_FileName)]
        internal static extern int ADL2_Display_PreservedAspectRatio_Set(IntPtr context, int adapterIndex, int displayIndex, int current);

        [DllImport(Atiadlxx_FileName)]
        internal static extern int ADL2_Display_ImageExpansion_Get(IntPtr context, int adapterIndex, int displayIndex, out int support, out int current, out int default_);

        [DllImport(Atiadlxx_FileName)]
        internal static extern int ADL2_Display_ImageExpansion_Set(IntPtr context, int adapterIndex, int displayIndex, int current);

        [DllImport(Atiadlxx_FileName)]
        internal static extern int ADL2_Display_SCE_State_Get(IntPtr context, int adapterIndex, int displayIndex, out int current, out int support, out int default_);

        [DllImport(Atiadlxx_FileName)]
        internal static extern int ADL2_Display_SCE_State_Set(IntPtr context, int adapterIndex, int displayIndex, int current);

        [DllImport(Atiadlxx_FileName)]
        internal static extern int ADL2_RIS_Settings_Get(IntPtr context, int adapterIndex, out ADL_RIS_SETTINGS settings);

        [DllImport(Atiadlxx_FileName)]
        internal static extern int ADL2_RIS_Settings_Set(IntPtr context, int adapterIndex, ADL_RIS_SETTINGS settings, ADL_RIS_NOTFICATION_REASON reason);

        [DllImport(Atiadlxx_FileName)]
        internal static extern int ADL2_Display_EdidData_Get(IntPtr context, int adapterIndex, int displayIndex, ref ADLDisplayEDIDData edidData);

        [DllImport(Atiadlxx_FileName)]
        internal static extern int ADL2_Display_EdidData_Set(IntPtr context, int adapterIndex, int displayIndex, ref ADLDisplayEDIDDataX2 edidData);
        #endregion DLLImport

        #region ADL_Main_Memory_Alloc
        /// <summary> Build in memory allocation function</summary>
        /// <param name="size">input size</param>
        /// <returns>return the memory buffer</returns>
        internal static IntPtr ADL_Main_Memory_Alloc_(int size)
        {
            IntPtr result = Marshal.AllocCoTaskMem(size);
            return result;
        }
        #endregion ADL_Main_Memory_Alloc

        #region ADL_Main_Memory_Free
        /// <summary> Build in memory free function</summary>
        /// <param name="buffer">input buffer</param>
        internal static void ADL_Main_Memory_Free(IntPtr buffer)
        {
            if (IntPtr.Zero != buffer)
            {
                Marshal.FreeCoTaskMem(buffer);
            }
        }
        #endregion ADL_Main_Memory_Free
    }
    #endregion ADL Class
}

#endregion ATI.ADL