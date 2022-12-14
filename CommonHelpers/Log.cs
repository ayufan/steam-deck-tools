using System.Diagnostics;
using System.Reflection;

namespace CommonHelpers
{
    public static class Log
    {
        internal static String SENTRY_DSN = "https://a6f1925b30fe43529aa7cefd0af7b8a4@o37791.ingest.sentry.io/4504316313993216";

#if DEBUG
        private static bool LogToTrace = true;
#else
        private static bool LogToTrace = false;
#endif
        private static bool LogToConsole = Environment.UserInteractive;

        internal static void SentryOptions(Sentry.SentryOptions o)
        {
            var build = Instance.IsDEBUG ? "debug" : "release";
            var type = File.Exists("Uninstaller.exe") ? "setup" : "zip";

            o.Dsn = Log.SENTRY_DSN;
            o.TracesSampleRate = 1.0;
            o.IsGlobalModeEnabled = true;
            o.Environment = String.Format("{0}:{1}_{2}", Instance.ApplicationName, build, type);
            o.DefaultTags.Add("App", Instance.ApplicationName);
            o.DefaultTags.Add("MachineID", Instance.MachineID);
            o.DefaultTags.Add("Build", type);
            o.DefaultTags.Add("Configuration", build);

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

        public static void TraceError(string format, params object?[] arg)
        {
            String line = string.Format(format, arg);
            Sentry.SentrySdk.CaptureMessage(line, Sentry.SentryLevel.Error);
            if (LogToTrace)
                Trace.WriteLine(line);
            if (LogToConsole)
                Console.WriteLine(line);
        }

        public static void TraceException(String type, Object? name, Exception e)
        {
            TraceLine("{0}: {1}: Exception: {2}", type, name, e);
            Sentry.SentrySdk.CaptureException(e, scope =>
            {
                scope.SetTag("type", type);
                scope.SetTag("name", name?.ToString() ?? "null");
            });
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
