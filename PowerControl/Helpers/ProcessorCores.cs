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
    internal class ProcessorCores
    {
        public static int GetProcessorCoreCount()
        {
            return GetProcessorCores().Count();
        }

        public static IntPtr GetProcessorMask(bool firstThreadOnly = false)
        {
            Int64 mask = 0; 

            foreach (var process in GetProcessorCores())
            {
                // This works only up-to 63 CPUs
                Int64 processorMask = (Int64)process.ProcessorMask.ToUInt64();

                if (firstThreadOnly)
                    processorMask = LSB(processorMask);

                mask |= processorMask;
            }

            return new IntPtr(mask);
        }

        public static bool HasSMTThreads()
        {
            foreach (var processorMask in GetProcessorMasks())
            {
                if (processorMask != LSB(processorMask))
                    return true;
            }

            return false;
        }

        public static bool IsUsingSMT(int processId)
        {
            try
            {
                var p = Process.GetProcessById(processId);
                UInt64 mask = (UInt64)p.ProcessorAffinity.ToInt64();

                foreach (var processorMask in GetProcessorMasks())
                {
                    // look for CPU that has more than 1 thread
                    // and has both assigned to process
                    var filtered = mask & processorMask;
                    if (filtered != LSB(filtered))
                        return true;
                }

                return false;
            }
            catch(ArgumentException)
            {
                return false;
            }
        }

        public static bool SetProcessSMT(int processId, bool allThreads)
        {
            try
            {
                var p = Process.GetProcessById(processId);
                UInt64 mask = (UInt64)p.ProcessorAffinity.ToInt64();

                foreach (var processorMask in GetProcessorMasks())
                {
                    var selectedMask = mask & processorMask;

                    if (selectedMask == 0)
                        continue; // ignore not assigned processors
                    else if (allThreads)
                        mask |= processorMask; // assign all threads
                    else
                        mask = LSB(selectedMask) | (mask & ~processorMask); // assign only first thread
                }

                p.ProcessorAffinity = new IntPtr((Int64)mask);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static UInt64 LSB(UInt64 value)
        {
            return (UInt64)LSB((Int64)value);
        }

        private static Int64 LSB(Int64 value)
        {
            return (value & -value);
        }

        static IEnumerable<UInt64> GetProcessorMasks()
        {
            return GetProcessorCores().Select((p) => p.ProcessorMask.ToUInt64());
        }

        static IEnumerable<SYSTEM_LOGICAL_PROCESSOR_INFORMATION> GetProcessorCores()
        {
            return GetLogicalProcessorInformation().Where((p) => p.Relationship == LOGICAL_PROCESSOR_RELATIONSHIP.RelationProcessorCore);
        }

        static SYSTEM_LOGICAL_PROCESSOR_INFORMATION[] GetLogicalProcessorInformation()
        {
            int bufferSize = 0;
            GetLogicalProcessorInformation(IntPtr.Zero, ref bufferSize);
            if (bufferSize == 0)
                return new SYSTEM_LOGICAL_PROCESSOR_INFORMATION[0];

            int sizeOfEntry = Marshal.SizeOf<SYSTEM_LOGICAL_PROCESSOR_INFORMATION>();
            int numEntries = bufferSize / sizeOfEntry;
            var processors = new SYSTEM_LOGICAL_PROCESSOR_INFORMATION[numEntries];

            var handle = Marshal.AllocHGlobal(bufferSize);
            try
            {
                if (!GetLogicalProcessorInformation(handle, ref bufferSize))
                    return new SYSTEM_LOGICAL_PROCESSOR_INFORMATION[0];

                for (int i = 0; i < processors.Length; i++)
                    processors[i] = Marshal.PtrToStructure<SYSTEM_LOGICAL_PROCESSOR_INFORMATION>(IntPtr.Add(handle, sizeOfEntry * i));

                return processors;
            }
            finally
            {
                Marshal.FreeHGlobal(handle);
            }
        }

        // Taken from: https://stackoverflow.com/a/63744912
        [StructLayout(LayoutKind.Sequential)]
        struct CACHE_DESCRIPTOR
        {
            public byte Level;
            public byte Associativity;
            public ushort LineSize;
            public uint Size;
            public uint Type;
        };

        [StructLayout(LayoutKind.Explicit)]
        struct SYSTEM_LOGICAL_PROCESSOR_INFORMATION_UNION
        {
            [FieldOffset(0)] public byte ProcessorCore;
            [FieldOffset(0)] public uint NumaNode;
            [FieldOffset(0)] public CACHE_DESCRIPTOR Cache;
            [FieldOffset(0)] private UInt64 Reserved1;
            [FieldOffset(8)] private UInt64 Reserved2;
        };

        public enum LOGICAL_PROCESSOR_RELATIONSHIP
        {
            RelationProcessorCore,
            RelationNumaNode,
            RelationCache,
            RelationProcessorPackage,
            RelationGroup,
            RelationAll = 0xffff
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SYSTEM_LOGICAL_PROCESSOR_INFORMATION
        {
            public UIntPtr ProcessorMask;
            public LOGICAL_PROCESSOR_RELATIONSHIP Relationship;
            public SYSTEM_LOGICAL_PROCESSOR_INFORMATION_UNION ProcessorInformation;
        }

        [DllImport("kernel32.dll")]
        static extern bool GetLogicalProcessorInformation(IntPtr buffer, ref int bufferSize);
    }
}
