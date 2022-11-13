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
        public Overlays.Mode OSDModeParsed
        {
            get
            {
                try
                {
                    return (Mode)Enum.Parse<Mode>(OSDMode);
                }
                catch (ArgumentException)
                {
                    return Mode.FPS;
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
