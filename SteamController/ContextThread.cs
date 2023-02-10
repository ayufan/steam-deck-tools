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

        public bool Start(int? startDelayMs = null)
        {
            if (thread is not null)
                return false;

            UpdatesPerSec = 0;
            threadRunning = true;
            thread = new Thread(ThreadLoop);
            thread.Start(startDelayMs);
            return true;
        }

        private void ThreadLoop(object? startDelayMs)
        {
            if (startDelayMs is int)
            {
                ThreadSleep((int)startDelayMs);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            int updates = 0;
            var nextReset = stopwatch.Elapsed.Add(UpdateResetInterval);

            X360.Start();
            DS4.Start();

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
                    ThreadSleep(100);
                }
            }

            X360.Stop();
            DS4.Stop();
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

        private bool ThreadSleep(int delayMs)
        {
            try
            {
                Thread.Sleep(delayMs);
                return true;
            }
            catch (ThreadInterruptedException)
            {
                return false;
            }
        }
    }
}