using System.Diagnostics;

namespace CommonHelpers
{
    public static class Log
    {
        public static void TraceLine(string format, params object?[] arg)
        {
            String line = string.Format(format, arg);

            Trace.WriteLine(line);
            if (Environment.UserInteractive)
                Console.WriteLine(line);
        }
    }
}
