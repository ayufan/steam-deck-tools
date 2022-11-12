using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.CPU;
using Microsoft.VisualBasic.Devices;
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
    internal class FanControl : IDisposable
    {
        public enum FanMode
        {
            Default,
            MidWay,
            Max
        }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        internal class FanSensor
        {
            public string? Name { get; internal set; }
            public float? Value { get; internal set; }
            public ushort InducedRPM { get; internal set; }

            internal string HardwareName { get; set; } = "";
            internal HardwareType HardwareType { get; set; }
            internal string SensorName { get; set; } = "";
            internal SensorType SensorType { get; set; }

            public void Reset()
            {
                Name = null;
                Value = null;
                InducedRPM = 0;
            }

            public bool Matches(ISensor sensor)
            {
                return sensor != null &&
                    sensor.Hardware.HardwareType == HardwareType && 
                    sensor.Hardware.Name.StartsWith(HardwareName) &&
                    sensor.SensorType == SensorType &&
                    sensor.Name == SensorName;
            }

            public bool Update(ISensor hwSensor)
            {
                if (!Matches(hwSensor))
                    return false;

                System.Diagnostics.Trace.WriteLine(String.Format("{0}: {1} {2}, value: {3}, type: {4}",
                    hwSensor.Identifier, hwSensor.Hardware.Name, hwSensor.Name, hwSensor.Value, hwSensor.SensorType));

                return Update(
                    String.Format("{0} {1}", hwSensor.Hardware.Name, hwSensor.Name),
                    hwSensor.Value);
            }

            public bool Update(string name, float? value)
            {
                if (!value.HasValue || value <= 0.0)
                    return false;

                Name = name;
                Value = value;
                return true;
            }

            public String FormattedValue()
            {
                if (!Value.HasValue)
                    return "";

                switch (SensorType)
                {
                    case SensorType.Temperature:
                        return Value.Value.ToString("F1") + "℃";
                    case SensorType.Power:
                        return Value.Value.ToString("F1") + "W";
                    default:
                        return Value.Value.ToString("F1");
                }
            }

            public override string ToString()
            {
                return Name;
            }
        }

        [CategoryAttribute("Fan")]
        public FanMode Mode { get; private set; }

        [CategoryAttribute("Fan")]
        public ushort CurrentRPM { get; private set; }

        [CategoryAttribute("Fan")]
        public ushort DesiredRPM { get; private set; }

        [CategoryAttribute("Sensor - APU"), DisplayName("Name")]
        public String? APUName { get { return allSensors["APU"].Name; } }
        [CategoryAttribute("Sensor - APU"), DisplayName("Power")]
        public String? APUPower { get { return allSensors["APU"].FormattedValue(); } }

        [CategoryAttribute("Sensor - CPU"), DisplayName("Name")]
        public String? CPUName { get { return allSensors["CPU"].Name; } }
        [CategoryAttribute("Sensor - CPU"), DisplayName("Temperature")]
        public String? CPUTemperature { get { return allSensors["CPU"].FormattedValue(); } }

        [CategoryAttribute("Sensor - GPU"), DisplayName("Name")]
        public String? GPUName { get { return allSensors["GPU"].Name; } }
        [CategoryAttribute("Sensor - GPU"), DisplayName("Temperature")]
        public String? GPUTemperature { get { return allSensors["GPU"].FormattedValue(); } }

        [CategoryAttribute("Sensor - SSD"), DisplayName("Name")]
        public String? SSDName { get { return allSensors["SSD"].Name; } }
        [CategoryAttribute("Sensor - SSD"), DisplayName("Temperature")]
        public String? SSDTemperature { get { return allSensors["SSD"].FormattedValue(); } }
        [CategoryAttribute("Sensor - Battery"), DisplayName("Name")]
        public String? BatteryName { get { return allSensors["Batt"].Name; } }
        [CategoryAttribute("Sensor - Battery"), DisplayName("Temperature")]
        public String? BatteryTemperature { get { return allSensors["Batt"].FormattedValue(); } }

        private Dictionary<string, FanSensor> allSensors = new Dictionary<string, FanSensor>
        {
            { "APU",
            new FanSensor()
            {
                // TODO: Is this correct?
                HardwareName = "AMD Custom APU 0405",
                HardwareType = HardwareType.Cpu,
                SensorName = "Package",
                SensorType = SensorType.Power
            } },
            { "CPU",
            new FanSensor()
            {
                HardwareName = "AMD Custom APU 0405",
                HardwareType = HardwareType.Cpu,
                SensorName = "Core (Tctl/Tdie)",
                SensorType = SensorType.Temperature
            } },
            { "GPU",
            new FanSensor()
            {
                HardwareName = "AMD Custom GPU 0405",
                HardwareType = HardwareType.GpuAmd,
                SensorName = "GPU Core",
                SensorType = SensorType.Temperature
            }        },
            { "SSD",
            new FanSensor()
            {
                HardwareType = HardwareType.Storage,
                SensorName = "Temperature",
                SensorType = SensorType.Temperature
            } },
            { "Batt",
            new FanSensor()
            {
                HardwareType = HardwareType.Battery,
                SensorName = "Temperature",
                SensorType = SensorType.Temperature
            } }
        };

        LibreHardwareMonitor.Hardware.Computer libreHardwareComputer = new LibreHardwareMonitor.Hardware.Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsStorageEnabled = true,
            IsBatteryEnabled = true
        };

        public FanControl()
        {
            libreHardwareComputer.Open();
        }

        private void visitHardware(IHardware hardware)
        {
            Dictionary<FanSensor, ISensor> matched = new Dictionary<FanSensor, ISensor>();

            foreach (ISensor hwSensor in hardware.Sensors)
            {
                foreach (var sensor in allSensors.Values)
                {
                    if (sensor.Matches(hwSensor))
                        matched[sensor] = hwSensor;
                }
            }

            if (matched.Any())
            {
                hardware.Update();
                foreach (var sensor in matched)
                    sensor.Key.Update(sensor.Value);
            }

            foreach (IHardware subhardware in hardware.SubHardware)
            {
                visitHardware(subhardware);
            }
        }

        public void Update()
        {
            CurrentRPM = Vlv0100.GetFanRPM();
            DesiredRPM = Vlv0100.GetFanDesiredRPM();

            foreach (var sensor in allSensors.Values)
                sensor.Reset();

            foreach(var hardware in libreHardwareComputer.Hardware)
                visitHardware(hardware);

            allSensors["Batt"].Update("VLV0100", Vlv0100.GetBattTemperature());
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

        public void Dispose()
        {
            libreHardwareComputer.Close();
        }
    }
}
