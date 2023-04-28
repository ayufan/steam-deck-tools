using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using static PerformanceOverlay.Sensors;
using CommonHelpers;

namespace PerformanceOverlay
{
    internal class Sensors : IDisposable
    {
        public abstract class Sensor
        {
            public abstract string? GetValue(Sensors sensors);
        }

        public abstract class ValueSensor : Sensor
        {
            public String? Format { get; set; }
            public float Multiplier { get; set; } = 1.0f;
            public bool IgnoreZero { get; set; }

            protected string? ConvertToString(float? value)
            {
                if (value is null)
                    return null;
                if (value == 0 && IgnoreZero)
                    return null;

                value *= Multiplier;
                return value.Value.ToString(Format, CultureInfo.GetCultureInfo("en-US"));
            }
        }

        public class UserValueSensor : ValueSensor
        {
            public delegate float? ValueDelegate();

            public ValueDelegate Value { get; set; }

            public override string? GetValue(Sensors sensors)
            {
                return ConvertToString(Value());
            }
        }

        public class HardwareSensor : ValueSensor
        {
            public string HardwareName { get; set; } = "";
            public IList<string> HardwareNames { get; set; } = new List<string>();
            public HardwareType HardwareType { get; set; }
            public string SensorName { get; set; } = "";
            public SensorType SensorType { get; set; }

            public bool Matches(ISensor sensor)
            {
                return sensor != null &&
                    sensor.Hardware.HardwareType == HardwareType &&
                    MatchesHardwareName(sensor.Hardware.Name) &&
                    sensor.SensorType == SensorType &&
                    sensor.Name == SensorName;
            }

            private bool MatchesHardwareName(string sensorHardwareName)
            {
                if (HardwareNames.Count > 0)
                {
                    if (HardwareNames.Any(hardwareName => sensorHardwareName.StartsWith(hardwareName)))
                        return true;
                }

                // Empty string matches always
                if (HardwareName.Length == 0)
                    return true;

                if (sensorHardwareName.StartsWith(HardwareName))
                    return true;

                return false;
            }

            public string? GetValue(ISensor sensor)
            {
                if (!sensor.Value.HasValue)
                    return null;

                return ConvertToString(sensor.Value.Value);
            }

            public override string? GetValue(Sensors sensors)
            {
                foreach (var hwSensor in sensors.AllHardwareSensors)
                {
                    if (Matches(hwSensor))
                    {
                        return GetValue(hwSensor);
                    }
                }
                return null;
            }
        }

        public class CompositeSensor : ValueSensor
        {
            public enum AggregateType
            {
                First,
                Min,
                Max,
                Avg
            };

            public IList<Sensor> Sensors { get; set; } = new List<Sensor>();
            public AggregateType Aggregate { get; set; } = AggregateType.First;
            public String? Format { get; set; }

            private IEnumerable<string> GetValues(Sensors sensors)
            {
                foreach (var sensor in Sensors)
                {
                    var result = sensor.GetValue(sensors);
                    if (result is not null)
                        yield return result;
                }
            }

            private IEnumerable<float> GetNumericValues(Sensors sensors)
            {
                return GetValues(sensors).Select((value) => float.Parse(value));
            }

            public override string? GetValue(Sensors sensors)
            {
                if (Aggregate == AggregateType.First)
                    return GetValues(sensors).FirstOrDefault();

                var numbers = GetNumericValues(sensors);
                if (numbers.Count() == 0)
                    return null;

                switch (Aggregate)
                {
                    case AggregateType.Min:
                        return ConvertToString(numbers.Min());

                    case AggregateType.Max:
                        return ConvertToString(numbers.Max());

                    case AggregateType.Avg:
                        return ConvertToString(numbers.Average());
                }

                return null;
            }
        }

        public readonly Dictionary<String, Sensor> AllSensors = new Dictionary<string, Sensor>
        {
            {
                "CPU_%", new HardwareSensor()
                {
                    HardwareType = HardwareType.Cpu,
                    HardwareName = "AMD Custom APU 0405",
                    SensorType = SensorType.Load,
                    SensorName = "CPU Total",
                    Format = "F0"
                }
            },
            {
                "CPU_W", new HardwareSensor()
                {
                    HardwareType = HardwareType.Cpu,
                    HardwareName = "AMD Custom APU 0405",
                    SensorType = SensorType.Power,
                    SensorName = "Package",
                    Format = "F1"
                }
            },
            {
                "CPU_T", new HardwareSensor()
                {
                    HardwareType = HardwareType.Cpu,
                    HardwareName = "AMD Custom APU 0405",
                    SensorType = SensorType.Temperature,
                    SensorName = "Core (Tctl/Tdie)",
                    Format = "F1",
                    IgnoreZero = true
                }
            },
            {
                "CPU_MHZ", new CompositeSensor()
                {
                    Format = "F0",
                    Aggregate = CompositeSensor.AggregateType.Max,
                    Sensors = Enumerable.Range(1, 4).Select((index) => {
                        return new HardwareSensor()
                        {
                            HardwareType = HardwareType.Cpu,
                            HardwareName = "AMD Custom APU 0405",
                            SensorType = SensorType.Clock,
                            SensorName = "Core #" + index.ToString(),
                            Format = "F0",
                            IgnoreZero = true
                        };
                    }).ToList<Sensor>()
                }
            },
            {
                "MEM_GB", new HardwareSensor()
                {
                    HardwareType = HardwareType.Memory,
                    HardwareName = "Generic Memory",
                    SensorType = SensorType.Data,
                    SensorName = "Memory Used",
                    Format = "F1"
                }
            },
            {
                "MEM_MB", new HardwareSensor()
                {
                    HardwareType = HardwareType.Memory,
                    HardwareName = "Generic Memory",
                    SensorType = SensorType.Data,
                    SensorName = "Memory Used",
                    Format = "F0",
                    Multiplier = 1024
                }
            },
            {
                "GPU_%", new HardwareSensor()
                {
                    HardwareType = HardwareType.GpuAmd,
                    HardwareNames = { "AMD Custom GPU 0405", "AMD Radeon 670M" },
                    SensorType = SensorType.Load,
                    SensorName = "D3D 3D",
                    Format = "F0"
                }
            },
            {
                "GPU_MB", new HardwareSensor()
                {
                    HardwareType = HardwareType.GpuAmd,
                    HardwareNames = { "AMD Custom GPU 0405", "AMD Radeon 670M" },
                    SensorType = SensorType.SmallData,
                    SensorName = "D3D Dedicated Memory Used",
                    Format = "F0"
                }
            },
            {
                "GPU_GB", new HardwareSensor()
                {
                    HardwareType = HardwareType.GpuAmd,
                    HardwareNames = { "AMD Custom GPU 0405", "AMD Radeon 670M" },
                    SensorType = SensorType.SmallData,
                    SensorName = "D3D Dedicated Memory Used",
                    Format = "F0",
                    Multiplier = 1.0f/1024.0f
                }
            },
            {
                "GPU_W", new HardwareSensor()
                {
                    HardwareType = HardwareType.GpuAmd,
                    HardwareNames = { "AMD Custom GPU 0405", "AMD Radeon 670M" },
                    SensorType = SensorType.Power,
                    SensorName = "GPU SoC",
                    Format = "F1"
                }
            },
            {
                "GPU_MHZ", new HardwareSensor()
                {
                    HardwareType = HardwareType.GpuAmd,
                    HardwareNames = { "AMD Custom GPU 0405", "AMD Radeon 670M" },
                    SensorType = SensorType.Clock,
                    SensorName = "GPU Core",
                    Format = "F0"
                }
            },
            {
                "GPU_T", new HardwareSensor()
                {
                    HardwareType = HardwareType.GpuAmd,
                    HardwareNames = { "AMD Custom GPU 0405", "AMD Radeon 670M" },
                    SensorType = SensorType.Temperature,
                    SensorName = "GPU Temperature",
                    Format = "F1",
                    IgnoreZero = true
                }
            },
            {
                "BATT_%", new HardwareSensor()
                {
                    HardwareType = HardwareType.Battery,
                    SensorType = SensorType.Level,
                    SensorName = "Charge Level",
                    Format = "F0"
                }
            },
            {
                "BATT_MIN", new HardwareSensor()
                {
                    HardwareType = HardwareType.Battery,
                    SensorType = SensorType.TimeSpan,
                    SensorName = "Remaining Time (Estimated)",
                    Format = "F0",
                    Multiplier = 1.0f/60.0f
                }
            },
            {
                "BATT_W", new HardwareSensor()
                {
                    HardwareType = HardwareType.Battery,
                    SensorType = SensorType.Power,
                    SensorName = "Discharge Rate",
                    Format = "F1"
                }
            },
            {
                "BATT_CHARGE_W", new HardwareSensor()
                {
                    HardwareType = HardwareType.Battery,
                    SensorType = SensorType.Power,
                    SensorName = "Charge Rate",
                    Format = "F1"
                }
            },
            {
                "FAN_RPM", new UserValueSensor()
                {
                    Value = delegate ()
                    {
                        return CommonHelpers.Vlv0100.GetFanRPM();
                    },
                    Format = "F0"
                }
            }
        };
        public IList<ISensor> AllHardwareSensors { get; private set; } = new List<ISensor>();

        public Sensors()
        {
        }

        public void Dispose()
        {
        }

        public void Update()
        {
            Instance.WithGlobalMutex(200, () =>
            {
                var allSensors = new List<ISensor>();

                foreach (IHardware hardware in Instance.HardwareComputer.Hardware)
                {
                    try
                    {
                        hardware.Update();
                    }
                    catch (SystemException) { }
                    hardware.Accept(new SensorVisitor(sensor => allSensors.Add(sensor)));
                }

                this.AllHardwareSensors = allSensors;
                return true;
            });
        }

        public string? GetValue(String name)
        {
            if (!AllSensors.ContainsKey(name))
                return null;

            return AllSensors[name].GetValue(this);
        }
    }
}
