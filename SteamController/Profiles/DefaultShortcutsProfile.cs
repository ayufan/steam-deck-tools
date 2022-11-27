using System.Diagnostics;
using System.Runtime.InteropServices;
using ExternalHelpers;
using WindowsInput;

namespace SteamController.Profiles
{
    public abstract class DefaultShortcutsProfile : Profile
    {
        public const String ShortcutConsumed = "ShortcutsProfile";
        public readonly TimeSpan HoldForShorcuts = TimeSpan.FromMilliseconds(200);
        private readonly TimeSpan HoldToSwitchProfile = TimeSpan.FromSeconds(1);
        private readonly TimeSpan HoldToSwitchDesktop = TimeSpan.FromSeconds(3);

        public override Status Run(Context c)
        {
            // Steam + 3 dots simulate CTRL+ALT+DELETE
            if (c.Steam.BtnSteam.Hold(HoldForShorcuts, ShortcutConsumed) && c.Steam.BtnQuickAccess.HoldOnce(HoldForShorcuts, ShortcutConsumed))
            {
                // TODO: Not working due to missing `uiAccess=true`
                c.Keyboard.KeyPress(new VirtualKeyCode[] { VirtualKeyCode.LCONTROL, VirtualKeyCode.LMENU }, VirtualKeyCode.DELETE);
                SendSAS(true);
                return Status.Done;
            }

            // Hold options for 1s to use next profile, or 3 seconds to switch between desktop-mode
            if (c.Steam.BtnOptions.HoldOnce(HoldToSwitchProfile, ShortcutConsumed))
            {
                if (!c.SelectNext())
                {
                    c.ToggleDesktopMode();
                }
                return Status.Done;
            }
            else if (c.Steam.BtnOptions.HoldChain(HoldToSwitchDesktop, ShortcutConsumed, "SwitchToDesktop"))
            {
                c.ToggleDesktopMode();
                return Status.Done;
            }

            // Always consume 3 dots
            if (c.Steam.BtnQuickAccess.Hold(HoldForShorcuts, ShortcutConsumed))
            {
                return Status.Done;
            }

            if (c.Steam.BtnSteam.Hold(HoldForShorcuts, ShortcutConsumed))
            {
                if (AdditionalShortcuts(c))
                {
                    return Status.Done;
                }
            }

            return Status.Continue;
        }

        protected virtual bool AdditionalShortcuts(Context c)
        {
            if (c.Steam.BtnMenu.Pressed())
            {
                c.Keyboard.KeyPress(VirtualKeyCode.LWIN, VirtualKeyCode.TAB);
                return true;
            }

            if (c.Steam.BtnOptions.Pressed())
            {
                c.Keyboard.KeyPress(VirtualKeyCode.F11);
                return true;
            }

            return false;
        }

        [DllImport("sas.dll")]
        private static extern void SendSAS(bool asUser);
    }
}
