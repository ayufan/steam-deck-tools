using System.Diagnostics;
using System.Reflection;

namespace CommonHelpers
{
    public static class Log
    {
        internal const String SENTRY_DSN = "https://a6f1925b30fe43529aa7cefd0af7b8a4@o37791.ingest.sentry.io/4504316313993216";

#if DEBUG
        private static bool LogToTrace = true;
#else
        private static bool LogToTrace = false;
#endif
        private static bool LogToConsole = Environment.UserInteractive;

        internal static void SentryOptions(Sentry.SentryOptions o)
        {
            o.Dsn = Log.SENTRY_DSN;
            o.Environment = File.Exists("Uninstaller.exe") ? "setup_" : "zip_";
            o.Environment += Instance.IsDEBUG ? "debug" : "release";
            o.TracesSampleRate = 1.0;
            o.IsGlobalModeEnabled = true;
            o.DefaultTags.Add("MachineID", Instance.MachineID);

            var releaseVersion = typeof(Log).Assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>().FirstOrDefault();
            if (releaseVersion is not null)
            {
                o.Release = releaseVersion.InformationalVersion;
            }
        }

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

        public static void TraceException(String type, Exception e)
        {
            TraceLine("{0}: Exception: {1}", type, e);
            Sentry.SentrySdk.CaptureException(e, scope =>
            {
                scope.SetTag("type", type);
            });
        }
    }
}
