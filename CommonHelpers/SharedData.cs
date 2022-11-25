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
    public class SharedData<T> : IDisposable where T : struct
    {
        const int MMF_MAX_SIZE = 16384;
        const int MMF_ALIGN_SIZE = 256;

        private MemoryMappedFile mmf;
        private int size;

        private SharedData(int size)
        {
            this.size = size;
        }

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

                byte[] buffer = new byte[size];
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

                byte[] buffer = new byte[size];
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

        private static int AlignedSize()
        {
            int size = Marshal.SizeOf<T>();
            size = (size + MMF_ALIGN_SIZE - 1) / MMF_ALIGN_SIZE * MMF_ALIGN_SIZE;
            if (size > MMF_MAX_SIZE)
                throw new ArgumentException();
            return size;
        }

        public static SharedData<T> CreateNew(String? name = null)
        {
            int size = AlignedSize();

            return new SharedData<T>(size)
            {
                mmf = MemoryMappedFile.CreateOrOpen(name ?? GetUniqueName(), size)
            };
        }

        public static SharedData<T> OpenExisting(String? name = null)
        {
            int size = AlignedSize();

            return new SharedData<T>(size)
            {
                mmf = MemoryMappedFile.OpenExisting(name ?? GetUniqueName())
            };
        }
    }
}
