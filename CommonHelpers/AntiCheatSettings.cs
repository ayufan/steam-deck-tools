using Microsoft.Win32;

namespace CommonHelpers
{
    public class AntiCheatSettings : BaseSettings
    {
        public const String HelpURL = "https://steam-deck-tools.ayufan.dev/#anti-cheat-and-antivirus-software";

        public static readonly AntiCheatSettings Default = new AntiCheatSettings();

        public enum AckState
        {
            NotYet,
            Always,
            Session,
            Never
        }

        public bool SessionAcked { get; set; }

        public AckState Acked
        {
            get => Get<AckState>("Acked", AckState.NotYet);
            set => Set<AckState>("Acked", value);
        }

        public bool NotYet
        {
            get { return Acked == AckState.NotYet; }
        }

        public bool Dismissed
        {
            get { return Acked == AckState.Never || Acked == AckState.Session && SessionAcked; }
        }

        public AntiCheatSettings() : base("AntiCheat")
        {
            Microsoft.Win32.SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        ~AntiCheatSettings()
        {
            Microsoft.Win32.SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
            {
                SessionAcked = false;
            }
        }

        public bool AckAntiCheat(String title, String? message, String? footnote = null, bool allowDismiss = true)
        {
            if (Dismissed)
                return true;

            Application.DoEvents();

            var page = new TaskDialogPage();

            page.Caption = title;
            page.AllowCancel = true;

            var alwaysShow = page.RadioButtons.Add("Always show Anti-Cheat warnings");
            var dismissSession = page.RadioButtons.Add("Hide Anti-Cheat warnings for current session");
            var neverShow = page.RadioButtons.Add("Never show Anti-Cheat warnings");

            alwaysShow.Checked = (Acked == AckState.Always || Acked == AckState.NotYet);
            dismissSession.Checked = (Acked == AckState.Session);
            neverShow.Checked = (Acked == AckState.Never);

            var continueButton = new TaskDialogButton("Continue") { ShowShieldIcon = true };
            var abortButton = new TaskDialogButton("Abort");

            page.Buttons.Add(continueButton);
            page.Buttons.Add(abortButton);
            page.Buttons.Add(TaskDialogButton.Help);
            page.Icon = TaskDialogIcon.ShieldWarningYellowBar;
            page.Heading = "Anti-Cheat Protection";
            page.Text = message;
            page.Footnote = new TaskDialogFootnote("This might result in kicking from the game or even be banned.");
            page.Footnote.Icon = TaskDialogIcon.Information;
            if (footnote is not null)
                page.Footnote.Text += "\n" + footnote;

            page.HelpRequest += delegate { Dependencies.OpenLink(HelpURL); };

            var result = TaskDialog.ShowDialog(new Form { TopMost = true }, page, TaskDialogStartupLocation.CenterScreen);
            if (result != continueButton)
                return false;

            if (alwaysShow.Checked)
            {
                Acked = AckState.Always;
            }
            else if (dismissSession.Checked)
            {
                Acked = AckState.Session;
                if (allowDismiss)
                    SessionAcked = true;
            }
            else if (neverShow.Checked)
            {
                Acked = AckState.Never;
            }
            return true;
        }
    }
}