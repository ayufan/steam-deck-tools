using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Sentry;

namespace CommonHelpers
{
    public static class Log
    {
#if PRODUCTION_BUILD
        internal static String SENTRY_DSN = "https://3c93e3c3b47b40ffba72d9cb333fc6d7@o4504334913830912.ingest.sentry.io/4504334914879488";
#else
        internal static String SENTRY_DSN = "https://d9204614b2cd47468bfa1ea2ab55da4e@o4504334914355200.ingest.sentry.io/4504334915469312";
#endif

#if DEBUG
        public static bool LogToTrace = true;
#else
        public static bool LogToTrace = false;
#endif
        public static bool LogToConsole = Environment.UserInteractive;
        public static bool LogToFile = false;
        public static bool LogToFileDebug = false;

        internal static void SentryOptions(Sentry.SentryOptions o)
        {
            var env = Instance.IsProductionBuild ? "prod" : "dev";
            var build = Instance.IsDEBUG ? "debug" : "release";
            var deploy = File.Exists("Uninstall.exe") ? "setup" : "zip";

            o.BeforeSend += Sentry_BeforeSend;
            o.Dsn = Log.SENTRY_DSN;
            o.TracesSampleRate = 1.0;
            o.IsGlobalModeEnabled = true;
            o.Environment = String.Format("{0}:{1}_{2}", Instance.ApplicationName, build, deploy);
            o.DefaultTags.Add("App", Instance.ApplicationName);
            o.DefaultTags.Add("Build", build);
            o.DefaultTags.Add("Deploy", deploy);

            var releaseVersion = typeof(Log).Assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>().FirstOrDefault();
            if (releaseVersion is not null)
            {
                o.Release = releaseVersion.InformationalVersion;
            }
        }

        private static String? LogFileFolder;

        private static void EnsureLogFileFolder()
        {
            if (LogFileFolder is not null)
                return;

            var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var steamControllerDocumentsFolder = Path.Combine(documentsFolder, "SteamDeckTools", "Logs");
            Directory.CreateDirectory(steamControllerDocumentsFolder);
            LogFileFolder = steamControllerDocumentsFolder;
        }

        public static void CleanupLogFiles(DateTime beforeTime)
        {
            EnsureLogFileFolder();

            if (LogFileFolder is null)
                return;

            var searchPattern = String.Format("{0}_*.log", Instance.ApplicationName);
            string[] files = Directory.GetFiles(LogFileFolder, searchPattern);

            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.LastAccessTime >= beforeTime)
                    continue;

                try
                {
                    fi.Delete();
                }
                catch (Exception ex)
                {
                    TraceException("CleanupLog", fi.Name, ex);
                }
            }
        }

        private static SentryEvent? Sentry_BeforeSend(SentryEvent arg)
        {
            if (Instance.HasFile("DisableCheckForUpdates.txt") || Instance.HasFile("DisableSentryTracking.txt"))
                return null;

            EnsureLogFileFolder();

            if (LogFileFolder is null)
                return null;

            String logFile = Path.Combine(LogFileFolder, String.Format("SentryLog_{0}.json", arg.Timestamp.ToString("yyyy-MM-dd")));

            using (var stream = File.Open(logFile, FileMode.OpenOrCreate | FileMode.Append))
            {
                using (var writer = new System.Text.Json.Utf8JsonWriter(stream))
                    arg.WriteTo(writer, null);
                stream.Write(new byte[] { (byte)'\r', (byte)'\n' });
            }
            return arg;
        }

        private static void WriteToLogFile(String line)
        {
            EnsureLogFileFolder();

            if (LogFileFolder is null)
                return;

            String logFile = Path.Combine(LogFileFolder, String.Format("{0}_{1}.json",
                Instance.ApplicationName, DateTime.UtcNow.ToString("yyyy-MM-dd")));

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    File.AppendAllText(logFile, String.Format("{0}: {1}: {2}\r\n",
                        DateTime.UtcNow, Process.GetCurrentProcess().Id, line));
                    return;
                }
                catch (IOException)
                {
                    Thread.Sleep(0);
                }
            }
        }

        public static void TraceObject(string name, object subject)
        {
            var serialized = JsonSerializer.Serialize(
                subject,
                new JsonSerializerOptions { IncludeFields = true }
            );
            TraceLine("Object: {0}: {1}", name, serialized);
        }

        public static void TraceLine(string format, params object?[] arg)
        {
            if (!LogToTrace && !LogToConsole && !LogToFile)
                return;

            String line = string.Format(format, arg);
            if (LogToTrace)
                Trace.WriteLine(line);
            if (LogToConsole)
                Console.WriteLine(line);
            if (LogToFile)
                WriteToLogFile(line);
        }

        public static void TraceDebug(string format, params object?[] arg)
        {
            if (!LogToTrace && !LogToConsole && !LogToFileDebug)
                return;

            String line = string.Format(format, arg);
            if (LogToTrace)
                Trace.WriteLine(line);
            if (LogToConsole)
                Console.WriteLine(line);
            if (LogToFileDebug)
                WriteToLogFile(line);
        }

        public static void TraceError(string format, params object?[] arg)
        {
            String line = string.Format(format, arg);
            Sentry.SentrySdk.CaptureMessage(line, Sentry.SentryLevel.Error);
            if (LogToTrace)
                Trace.WriteLine(line);
            if (LogToConsole)
                Console.WriteLine(line);
            if (LogToFile)
                WriteToLogFile(line);
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

        public static void DebugException(String type, Exception e)
        {
        }

        public static void DebugException(String type, Object? name, Exception e)
        {
        }
    }
}
