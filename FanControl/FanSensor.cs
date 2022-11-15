using CommonHelpers;
using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace FanControl
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal class FanSensor
    {
        public string? Name { get; internal set; }
        public float? Value { get; internal set; }
        public ushort? CalculatedRPM { get; internal set; }

        public float ValueDeadZone { get; set; }
        public int AvgSamples { get; set; } = 5;
        public float? MaxValue { get; set; }

        internal string HardwareName { get; set; } = "";
        internal HardwareType HardwareType { get; set; }
        internal string SensorName { get; set; } = "";
        internal SensorType SensorType { get; set; }

        private List<float> AllSamples = new List<float>();

        internal Dictionary<FanMode, Profile> Profiles { get; set; } = new Dictionary<FanMode, Profile>();

        internal class Profile
        {
            public enum ProfileType
            {
                Constant,
                Quadratic,
                Pid
            }

            public ProfileType Type { get; set; }
            public float MinInput { get; set; }
            public float MaxInput { get; set; } = 90;
            public ushort MinRPM { get; set; }
            public ushort MaxRPM { get; set; } = ushort.MaxValue;

            public float A { get; set; }
            public float B { get; set; }
            public float C { get; set; }

            public float Kp { get; set; }
            public float Ki { get; set; }
            public float Kd { get; set; }
            public float PidSetPoint { get; set; }

            private float? pidLastInput { get; set; }
            private float pidLastError { get; set; }
            private DateTime pidLastTime { get; set; }
            private float pidP { get; set; }
            private float pidI { get; set; }
            private float pidD { get; set; }

            public ushort CalculateRPM(float input)
            {
                float rpm = 0;

                switch (Type)
                {
                    case ProfileType.Constant:
                        rpm = MinRPM;
                        break;

                    case ProfileType.Quadratic:
                        rpm = calculateQuadraticRPM(input);
                        break;

                    case ProfileType.Pid:
                        rpm = calculatePidRPM(input);
                        break;
                }

                if (input < MinInput)
                    rpm = MinRPM;
                else if (input > MaxInput)
                    rpm = MaxRPM;

                rpm = Math.Clamp(rpm, (float)MinRPM, (float)MaxRPM);

                return (ushort)rpm;
            }

            private float calculateQuadraticRPM(float input)
            {
                return A * input * input + B * input + C;
            }

            private float calculatePidRPM(float input)
            {
                if (!pidLastInput.HasValue)
                {
                    pidLastInput = input;
                    pidLastTime = DateTime.Now;
                    return 0;
                }

                float error = PidSetPoint - input;
                float dInput = input - pidLastInput.Value;
                float dt = Math.Min((float)(DateTime.Now - pidLastTime).TotalSeconds, 1.0f);

                this.pidP = Kp * error;
                this.pidI += Ki * error * dt;
                this.pidI = Math.Min(this.pidI, this.MaxRPM);
                this.pidD -= Kd * dInput / dt;

                pidLastInput = input;
                pidLastError = error;
                pidLastTime = DateTime.Now;

                return pidP + pidI + pidD;
            }
        }

        public void Reset()
        {
            Name = null;
            Value = null;
            CalculatedRPM = 0;
        }

        public bool Matches(ISensor sensor)
        {
            return sensor != null &&
                sensor.Hardware.HardwareType == HardwareType &&
                sensor.Hardware.Name.StartsWith(HardwareName) &&
                sensor.SensorType == SensorType &&
                sensor.Name == SensorName;
        }

        public bool Update(ISensor hwSensor, FanMode mode)
        {
            if (!Matches(hwSensor))
                return false;

            System.Diagnostics.Trace.WriteLine(String.Format("{0}: {1} {2}, value: {3}, type: {4}",
                hwSensor.Identifier, hwSensor.Hardware.Name, hwSensor.Name, hwSensor.Value, hwSensor.SensorType));

            return Update(
                String.Format("{0} {1}", hwSensor.Hardware.Name, hwSensor.Name),
                hwSensor.Value, mode);
        }

        public bool Update(string name, float? newValue, FanMode mode)
        {
            if (!newValue.HasValue || newValue <= 0.0)
                return false;

            if (MaxValue.HasValue)
                newValue = Math.Min(newValue.Value, MaxValue.Value);

            if (AllSamples.Count == 0 || Math.Abs(AllSamples.Last() - newValue.Value) >= ValueDeadZone)
            {
                AllSamples.Add(newValue.Value);
                while (AllSamples.Count > AvgSamples)
                    AllSamples.RemoveAt(0);
            }

            float avgValue = 0.0f;
            foreach (var value in AllSamples)
                avgValue += value;

            Name = name;
            Value = avgValue / AllSamples.Count;
            CalculatedRPM = CalculateRPM(mode);
            return true;
        }

        public bool IsValid(FanMode mode)
        {
            // If we have profile, but no sensor value to consume it
            // it is invalid
            if (Profiles.ContainsKey(mode) && !Value.HasValue)
                return false;

            return true;
        }

        private String Unit()
        {
            switch (SensorType)
            {
                case SensorType.Temperature:
                    return "℃";

                case SensorType.Power:
                    return "W";

                default:
                    return "";
            }
        }

        public String FormattedValue()
        {
            if (!Value.HasValue)
                return "";

            String value = "";
            
            if (AllSamples.Count > 0) 
                value += AllSamples.Last().ToString("F1") + Unit();

            value += " (avg: " + Value.Value.ToString("F1") + Unit() + ")";

            if (CalculatedRPM.HasValue)
                value += " (" + CalculatedRPM.ToString() + "RPM)";

            return value;
        }

        public ushort? CalculateRPM(FanMode mode)
        {
            if (!Profiles.ContainsKey(mode) || !Value.HasValue)
                return null;

            var profile = Profiles[mode];
            return profile.CalculateRPM(Value.Value);
        }
    }
}
