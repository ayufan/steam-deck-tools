using System.Diagnostics;

namespace CommonHelpers
{
    public static class Log
    {
#if DEBUG
        private static bool LogToTrace = true;
#else
        private static bool LogToTrace = false;
#endif
        private static bool LogToConsole = Environment.UserInteractive;

        public static void TraceLine(string format, params object?[] arg)
        {
            if (!LogToTrace && !LogToConsole)
                return;

            String line = string.Format(format, arg);
            if (LogToTrace)
                Trace.WriteLine(line);
            if (LogToConsole)
                Console.WriteLine(line);
        }
    }
}
