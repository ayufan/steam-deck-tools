using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FanControl
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal class FanControl
    {
        public enum FanMode
        {
            Default,
            MidWay,
            Max
        }

        [CategoryAttribute("Fan")]
        public FanMode Mode { get; private set; }

        [CategoryAttribute("Fan")]
        public ushort CurrentRPM { get; private set; }

        [CategoryAttribute("Fan")]
        public ushort DesiredRPM { get; private set; }

        public void Update()
        {
            CurrentRPM = Vlv0100.GetFanRPM();
            DesiredRPM = Vlv0100.GetFanDesiredRPM();
        }

        public void SetMode(FanMode mode)
        {
            switch (mode)
            {
                case FanMode.Default:
                    Vlv0100.SetFanControl(false);
                    break;

                case FanMode.MidWay:
                    Vlv0100.SetFanControl(true);
                    Vlv0100.SetFanDesiredRPM(Vlv0100.MAX_FAN_RPM/2);
                    break;

                case FanMode.Max:
                    Vlv0100.SetFanControl(true);
                    Vlv0100.SetFanDesiredRPM(Vlv0100.MAX_FAN_RPM);
                    break;
            }

            this.Mode = mode;
        }
    }
}
