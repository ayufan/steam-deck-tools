using System.Diagnostics;
using static CommonHelpers.Log;

namespace SteamController
{
    public partial class Context : IDisposable
    {
        public readonly TimeSpan UpdateResetInterval = TimeSpan.FromSeconds(1);

        Thread? thread;
        private bool threadRunning;

        public int UpdatesPerSec { get; private set; }

        public bool Start()
        {
            if (thread is not null)
                return false;

            threadRunning = true;
            thread = new Thread(ThreadLoop);
            thread.Start();
            return true;
        }

        private void ThreadLoop(object? obj)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            int updates = 0;
            var nextReset = stopwatch.Elapsed.Add(UpdateResetInterval);

            while (threadRunning)
            {
                if (nextReset < stopwatch.Elapsed)
                {
                    UpdatesPerSec = updates;
                    nextReset = stopwatch.Elapsed.Add(UpdateResetInterval);
                    updates = 0;
                }

                updates++;
                Update();
                Debug();

                if (!Enabled || !Steam.Updated)
                {
                    Thread.Sleep(100);
                }
            }
        }

        public void Stop()
        {
            threadRunning = false;

            if (thread != null)
            {
                thread.Interrupt();
                thread.Join();
                thread = null;
            }
        }
    }
}