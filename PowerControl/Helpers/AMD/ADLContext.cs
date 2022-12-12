using System.Runtime.InteropServices;
using CommonHelpers;

namespace PowerControl.Helpers.AMD
{
    internal class ADLContext : IDisposable
    {
        public IntPtr Context { get; private set; }

        public ADLContext()
        {
            IntPtr context = IntPtr.Zero;
            WithSafe(() => ADL.ADL2_Main_Control_Create(ADL.ADL_Main_Memory_Alloc_, 1, out context), true);
            Context = context;
        }

        public bool IsValid
        {
            get { return Context != IntPtr.Zero; }
        }

        public ADLAdapterInfo[]? AdapterInfos
        {
            get
            {
                int res = ADL.ADL2_Adapter_NumberOfAdapters_Get(Context, out var numAdapters);
                if (res != 0)
                    return null;

                res = ADL.ADL2_Adapter_AdapterInfo_Get(Context,
                    out var adapters,
                    Marshal.SizeOf<ADLAdapterInfoArray>());
                if (res != 0)
                    return null;

                return adapters.ADLAdapterInfo.Take(numAdapters).ToArray();
            }
        }

        public IEnumerable<ADLDisplayInfo> DisplayInfos
        {
            get
            {
                foreach (var adapter in AdapterInfos)
                {
                    if (adapter.Present == 0)
                        continue;

                    foreach (var display in GetDisplayInfos(adapter.AdapterIndex))
                    {
                        if ((display.DisplayInfoValue & ADL.ADL_DISPLAY_DISPLAYINFO_DISPLAYCONNECTED) != ADL.ADL_DISPLAY_DISPLAYINFO_DISPLAYCONNECTED)
                            continue;

                        yield return display;
                    }
                }
            }
        }

        public IEnumerable<ADLDisplayInfo> GetDisplayInfos(int adapterIndex)
        {
            int res = ADL.ADL2_Display_DisplayInfo_Get(Context, adapterIndex, out var numDisplays, out var displays, 0);
            if (res != 0)
                yield break;

            try
            {
                int sizeOf = Marshal.SizeOf<ADLDisplayInfo>();

                for (int i = 0; i < numDisplays; i++)
                {
                    var display = Marshal.PtrToStructure<ADLDisplayInfo>(IntPtr.Add(displays, i * sizeOf));
                    // TODO: why even though we pass adapterIndex?
                    if (display.DisplayID.DisplayLogicalAdapterIndex != adapterIndex)
                        continue;
                    yield return display;
                }
            }
            finally
            {
                ADL.ADL_Main_Memory_Free(displays);
            }
        }

        ~ADLContext()
        {
            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            if (Context != IntPtr.Zero)
            {
                WithSafe(() => ADL.ADL2_Main_Control_Destroy(Context));
                Context = IntPtr.Zero;
            }
        }

        private static ADLContext? instance;
        private static readonly Mutex mutex = new Mutex();

        public static T? WithSafe<T>(Func<ADLContext, T?> func, bool useInstance = false)
        {
            if (useInstance)
            {
                if (!mutex.WaitOne(200))
                    return default;

                try
                {
                    if (instance == null)
                        instance = new ADLContext();
                    return instance.WithSafe(() => func(instance));
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            else
            {
                using (var context = new ADLContext())
                {
                    return context.WithSafe(() => func(context));
                }
            }
        }

        public T? WithSafe<T>(Func<T?> func, bool force = false)
        {
            if (!IsValid && !force)
                return default;

            try
            {
                return func();
            }
            catch (DllNotFoundException e) { Log.TraceException("ADL: Method not found", e); }
            catch (EntryPointNotFoundException e) { Log.TraceException("ADL: Entry point not found", e); }
            catch (Exception e) { Log.TraceException("ADL: Generic Exception", e); }

            return default;
        }
    }
}
