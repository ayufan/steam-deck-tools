using System.Reflection;
using CommonHelpers;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace SteamController.Profiles.Dynamic
{
    public sealed partial class RoslynDynamicProfile : Profile
    {
        private const int ScriptTimeout = 10; // max 10ms

        private String fileName;
        private Script? compiledScript;
        private Profile? inherited;
        private DateTime? lastModifiedTime;
        private System.Windows.Forms.Timer? watchTimer;

        public RoslynDynamicProfile(string name, string fileName, Profile? inherited = null)
        {
            this.fileName = fileName;
            this.inherited = inherited;
            this.Name = name;
            if (inherited is not null)
                this.Name = inherited.Name + ": " + name;
            this.Visible = inherited?.Visible ?? true;
            this.IsDesktop = inherited?.IsDesktop ?? false;
        }

        private static ScriptOptions CompilationOptions
        {
            get
            {
                var options = ScriptOptions.Default;

                // Add Keyboard controls
                options = options.AddReferences(typeof(KeyModifiers).Assembly);
                options = options.AddImports(typeof(KeyModifiers).FullName);
                options = options.AddReferences(typeof(ProfilesSettings.VirtualKeyCode).Assembly);
                options = options.AddImports(typeof(ProfilesSettings.VirtualKeyCode).FullName);
                return options;
            }
        }

        public bool Compile()
        {
            this.compiledScript = null;
            this.lastModifiedTime = null;
            this.Errors = null;

            if (!File.Exists(fileName))
            {
                this.Errors = new string[] { String.Format("File '{0}' does not exist.", fileName) };
                return false;
            }

            try
            {
                this.lastModifiedTime = File.GetLastWriteTimeUtc(fileName);

                using (var file = System.IO.File.OpenRead(fileName))
                {
                    var options = CompilationOptions.WithFilePath(Path.GetFileName(fileName));
                    var script = CSharpScript.Create(file, options, typeof(Globals));
                    var compileResult = script.Compile();
                    var errors = compileResult.Where((result) => result.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);

                    if (!errors.Any())
                    {
                        this.compiledScript = script;
                        return true;
                    }

                    this.Errors = errors.Select((result) => result.ToString()).ToArray();
                    OnErrorsChanged();
                }
            }
            catch (Exception e)
            {
                this.Errors = new string[] { e.Message };
                OnErrorsChanged();
            }

            Log.TraceLine("UserProfile: {0}: Compilation Error", fileName);
            foreach (var error in this.Errors)
                Log.TraceLine("\t{0}", error);
            return false;
        }

        public void Watch()
        {
            if (this.lastModifiedTime is null)
                return;
            if (this.watchTimer is not null)
                return;

            watchTimer = new System.Windows.Forms.Timer();
            watchTimer.Interval = 1000;
            watchTimer.Tick += delegate
            {
                try
                {
                    if (this.lastModifiedTime is null)
                        return;

                    var latest = File.GetLastWriteTimeUtc(fileName);
                    if (this.lastModifiedTime >= latest)
                        return;

                    Log.TraceLine("UserProfile: {0}. Detected modification: '{1}' vs '{2}'", fileName, this.lastModifiedTime, latest);
                }
                catch (Exception) { return; }

                Compile();
            };
            watchTimer.Start();
        }

        public override System.Drawing.Icon Icon
        {
            get
            {
                if (inherited is not null)
                    return inherited.Icon;
                else if (CommonHelpers.WindowsDarkMode.IsDarkModeEnabled)
                    return Resources.microsoft_xbox_controller_white;
                else
                    return Resources.microsoft_xbox_controller;
            }
        }

        public override bool Selected(Context context)
        {
            return (this.compiledScript is not null) && (inherited?.Selected(context) ?? true);
        }

        public override Status Run(Context context)
        {
            if (inherited?.Run(context).IsDone ?? false)
                return Status.Done;

            if (this.compiledScript is null)
                return Status.Continue;

            try
            {
                var cancelToken = new CancellationTokenSource();
                var task = this.compiledScript.RunAsync(new Globals(this, context), cancelToken.Token);
                if (!task.Wait(ScriptTimeout))
                {
                    cancelToken.Cancel();
                    task.Wait();
                    Log.TraceLine("UserProfile: {0}: Timedout. Canceled.");
                }
            }
            catch (Exception e)
            {
                Log.TraceLine("UserProfile: {0}: {1}", fileName, e);
            }

            return Status.Continue;
        }

        public static IEnumerable<RoslynDynamicProfile> GetUserProfiles(Dictionary<String, Profile> preconfiguredProfiles)
        {
            var files = new Dictionary<String, String>();

            foreach (var directory in GetUserProfilesPaths())
            {
                foreach (var profile in preconfiguredProfiles)
                {
                    foreach (string file in Directory.GetFiles(directory, profile.Key))
                    {
                        String name = Path.GetFileNameWithoutExtension(file);
                        name = Path.GetFileNameWithoutExtension(name);

                        yield return new RoslynDynamicProfile(name, file, profile.Value);
                    }
                }
            }
        }

        private static IEnumerable<String> GetUserProfilesPaths()
        {
            var exePath = Assembly.GetExecutingAssembly().Location;
            var exeFolder = Path.GetDirectoryName(exePath);
            if (exeFolder is not null)
            {
                var exeControllerProfiles = Path.Combine(exeFolder, "ControllerProfiles");
                if (Directory.Exists(exeControllerProfiles))
                    yield return exeControllerProfiles;
            }

            var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var controllerProfilesDocumentsFolder = Path.Combine(documentsFolder, "SteamController", "ControllerProfiles");
            if (Directory.Exists(controllerProfilesDocumentsFolder))
                yield return controllerProfilesDocumentsFolder;
        }
    }
}
