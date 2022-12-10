using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace SteamController.ProfilesSettings
{
    public enum VirtualX360Code
    {
        None = 0,

        X360_UP,
        X360_DOWN,
        X360_LEFT,
        X360_RIGHT,
        X360_BACK,
        X360_START,

        X360_A,
        X360_B,
        X360_X,
        X360_Y,

        X360_LB,
        X360_RB,
        X360_LS,
        X360_RS
    }

    public static class VirtualX360CodeExtensions
    {
        private static readonly Dictionary<VirtualX360Code, Xbox360Button> codeToButton = new Dictionary<VirtualX360Code, Xbox360Button>()
        {
            { VirtualX360Code.X360_UP, Xbox360Button.Up },
            { VirtualX360Code.X360_DOWN, Xbox360Button.Down },
            { VirtualX360Code.X360_LEFT, Xbox360Button.Left },
            { VirtualX360Code.X360_RIGHT, Xbox360Button.Right },
            { VirtualX360Code.X360_BACK, Xbox360Button.Back },
            { VirtualX360Code.X360_START, Xbox360Button.Start },
            { VirtualX360Code.X360_A, Xbox360Button.A },
            { VirtualX360Code.X360_B, Xbox360Button.B },
            { VirtualX360Code.X360_X, Xbox360Button.X },
            { VirtualX360Code.X360_Y, Xbox360Button.Y },
            { VirtualX360Code.X360_LB, Xbox360Button.LeftShoulder },
            { VirtualX360Code.X360_RB, Xbox360Button.RightShoulder },
            { VirtualX360Code.X360_LS, Xbox360Button.LeftThumb },
            { VirtualX360Code.X360_RS, Xbox360Button.RightThumb }
        };

        public static Xbox360Button? ToViGEm(this VirtualX360Code code)
        {
            if (codeToButton.TryGetValue(code, out var button))
                return button;
            return null;
        }
    }
}
