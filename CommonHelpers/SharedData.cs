using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers
{
    public class SharedData<T> : IDisposable where T : unmanaged
    {
        const int MMF_MAX_SIZE = 256;

        private MemoryMappedFile mmf;

        private SharedData()
        { }

        public T NewValue()
        {
            return default(T);
        }

        public bool GetValue(out T value)
        {
            using (MemoryMappedViewStream mmvStream = mmf.CreateViewStream())
            {
                value = default(T);

                if (!mmvStream.CanRead)
                    return false;

                byte[] buffer = new byte[MMF_MAX_SIZE];
                mmvStream.Read(buffer, 0, buffer.Length);

                var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    var output = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
                    if (output is null)
                        return false;

                    value = (T)output;
                    return true;
                }
                catch
                {
                    return false;
                }
                finally
                {
                    handle.Free();
                }
            }
        }

        public bool SetValue(T value)
        {
            using (MemoryMappedViewStream mmvStream = mmf.CreateViewStream())
            {
                if (!mmvStream.CanWrite)
                    return false;

                byte[] buffer = new byte[MMF_MAX_SIZE];
                var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
                }
                catch
                {
                    return false;
                }
                finally
                {
                    handle.Free();
                }

                mmvStream.Write(buffer, 0, buffer.Length);
                return true;
            }
        }

        public static bool GetExistingValue(out T value)
        {
            try
            {
                using (var shared = OpenExisting())
                {
                    if (shared.GetValue(out value))
                        return true;
                }
            }
            catch
            {
                value = default(T);
            }
            return false;
        }

        public static bool SetExistingValue(T value)
        {
            try
            {
                using (var shared = OpenExisting())
                {
                    if (shared.SetValue(value))
                        return true;
                }
            }
            catch
            {
            }
            return false;
        }

        public void Dispose()
        {
            mmf.Dispose();
        }

        public static String GetUniqueName()
        {
            return String.Format("Global_{0}_Setting", typeof(T).Name);
        }

        public static SharedData<T> CreateNew(String? name = null)
        {
            return new SharedData<T>()
            {
                mmf = MemoryMappedFile.CreateOrOpen(name ?? GetUniqueName(), MMF_MAX_SIZE)
            };
        }

        public static SharedData<T> OpenExisting(String? name = null)
        {
            return new SharedData<T>()
            {
                mmf = MemoryMappedFile.OpenExisting(name ?? GetUniqueName())
            };
        }
    }
}
