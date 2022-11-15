using CommonHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PerformanceOverlay.Overlays;

namespace PerformanceOverlay
{
    internal partial class Settings
    {
        public OverlayMode OSDModeParsed
        {
            get
            {
                try
                {
                    return (OverlayMode)Enum.Parse<OverlayMode>(OSDMode);
                }
                catch (ArgumentException)
                {
                    return OverlayMode.FPS;
                }
            }
            set
            {
                OSDMode = value.ToString();
                Save();
            }
        }
    }
}
