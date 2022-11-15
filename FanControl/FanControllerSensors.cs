using CommonHelpers;
using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FanControl
{
    internal partial class FanController
    {
        private Dictionary<string, FanSensor> allSensors = new Dictionary<string, FanSensor>
        {
            {
                "APU", new FanSensor()
                {
                    // TODO: Is this correct?
                    HardwareName = "AMD Custom APU 0405",
                    HardwareType = HardwareType.Cpu,
                    SensorName = "Package",
                    SensorType = SensorType.Power,
                    ValueDeadZone = 0.1f,
                    AvgSamples = 20,
                    MaxValue = 25, // TODO: On resume a bogus value is returned
                    Profiles = new Dictionary<FanMode, FanSensor.Profile>()
                    {
                        {
                            FanMode.Max, new FanSensor.Profile()
                            {
                                Type = FanSensor.Profile.ProfileType.Constant,
                                MinRPM = CommonHelpers.Vlv0100.MAX_FAN_RPM
                            }
                        },
                        {
                            FanMode.SteamOS, new FanSensor.Profile()
                            {
                                Type = FanSensor.Profile.ProfileType.Constant,
                                MinRPM = 1500
                            }
                        },
                    }
                }
            },
            {
                "CPU", new FanSensor()
                {
                    HardwareName = "AMD Custom APU 0405",
                    HardwareType = HardwareType.Cpu,
                    SensorName = "Core (Tctl/Tdie)",
                    SensorType = SensorType.Temperature,
                    ValueDeadZone = 0.0f,
                    AvgSamples = 20,
                    Profiles = new Dictionary<FanMode, FanSensor.Profile>()
                    {
                        {
                            FanMode.SteamOS, new FanSensor.Profile()
                            {
                                Type = FanSensor.Profile.ProfileType.Quadratic,
                                MinInput = 55,
                                MaxInput = 90,
                                A = 2.286f,
                                B = -188.6f,
                                C = 5457.0f
                            }
                        }
                    }
                }
            },
            {
                "GPU", new FanSensor()
                {
                    HardwareName = "AMD Custom GPU 0405",
                    HardwareType = HardwareType.GpuAmd,
                    SensorName = "GPU Core",
                    SensorType = SensorType.Temperature,
                    ValueDeadZone = 0.0f,
                    AvgSamples = 20,
                    Profiles = new Dictionary<FanMode, FanSensor.Profile>()
                    {
                        {
                            FanMode.SteamOS, new FanSensor.Profile()
                            {
                                Type = FanSensor.Profile.ProfileType.Quadratic,
                                MinInput = 55,
                                MaxInput = 90,
                                A = 2.286f,
                                B = -188.6f,
                                C = 5457.0f
                            }
                        }
                    }
                }
            },
            {
                "SSD", new FanSensor()
                {
                    HardwareType = HardwareType.Storage,
                    SensorName = "Temperature",
                    SensorType = SensorType.Temperature,
                    ValueDeadZone = 0.5f,
                    Profiles = new Dictionary<FanMode, FanSensor.Profile>()
                    {
                        {
                            FanMode.SteamOS, new FanSensor.Profile()
                            {
                                Type = FanSensor.Profile.ProfileType.Pid,
                                MinInput = 30,
                                MaxInput = 70,
                                MaxRPM = 3000,
                                PidSetPoint = 70,
                                Kp = 0,
                                Ki = -20,
                                Kd = 0
                            }
                        }
                    }
                }
            },
            {
                "Batt", new FanSensor()
                {
                    HardwareType = HardwareType.Battery,
                    SensorName = "Temperature",
                    SensorType = SensorType.Temperature,
                    ValueDeadZone = 0.0f,
                    Profiles = new Dictionary<FanMode, FanSensor.Profile>()
                    {
                        {
                            FanMode.SteamOS, new FanSensor.Profile()
                            {
                                // If battery goes over 40oC require 2kRPM
                                Type = FanSensor.Profile.ProfileType.Constant,
                                MinInput = 0,
                                MaxInput = 40,
                                MinRPM = 0,
                                MaxRPM = 2000,
                            }
                        }
                    }
                }
            }
        };

        #region Sensor Properties for Property Grid
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

        #endregion Sensor Properties for Property Grid
    }
}
