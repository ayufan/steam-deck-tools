using Nefarius.ViGEm.Client.Targets.DualShock4;
using SteamController.Devices;

namespace SteamController.ProfilesSettings
{
    public enum VirtualDS4Code
    {
        None = 0,

        DS4_TRIANGLE,
        DS4_CIRCLE,
        DS4_CROSS,
        DS4_SQUARE,

        DS4_OPTIONS,
        DS4_SHARE,

        DS4_STICK_LEFT,
        DS4_STICK_RIGHT,

        DS4_L1,
        DS4_R1,
        DS4_L2,
        DS4_R2,
    }

    public static class VirtualDS4CodeExtensions
    {
        private static readonly Dictionary<VirtualDS4Code, DS4Controller.DualShock4Button> codeToButton = new Dictionary<VirtualDS4Code, DS4Controller.DualShock4Button>()
        {
            { VirtualDS4Code.DS4_TRIANGLE, DS4Controller.Triangle },
            { VirtualDS4Code.DS4_CIRCLE, DS4Controller.Circle },
            { VirtualDS4Code.DS4_CROSS, DS4Controller.Cross },
            { VirtualDS4Code.DS4_SQUARE, DS4Controller.Square },
            { VirtualDS4Code.DS4_OPTIONS, DS4Controller.Options },
            { VirtualDS4Code.DS4_SHARE, DS4Controller.Share },
            { VirtualDS4Code.DS4_STICK_LEFT, DS4Controller.ThumbLeft },
            { VirtualDS4Code.DS4_STICK_RIGHT, DS4Controller.ThumbRight },
            { VirtualDS4Code.DS4_L1, DS4Controller.ShoulderLeft },
            { VirtualDS4Code.DS4_R1, DS4Controller.ShoulderRight },
            { VirtualDS4Code.DS4_L2, DS4Controller.TriggerLeft },
            { VirtualDS4Code.DS4_R2, DS4Controller.TriggerRight }
        };

        public static DS4Controller.DualShock4Button? ToDS4Button(this VirtualDS4Code code)
        {
            if (codeToButton.TryGetValue(code, out var button))
                return button;
            return null;
        }
    }
}
