using hidapi;
using PowerControl.External;
using static CommonHelpers.Log;

namespace SteamController.Devices
{
    public abstract class SteamAction
    {
        public SteamController? Controller { get; internal set; }
        public String Name { get; internal set; } = "";

        /// This is action controlled by Lizard mode
        public bool LizardButton { get; internal set; }
        public bool LizardMouse { get; internal set; }

        internal abstract void Reset();
        internal abstract bool BeforeUpdate(byte[] buffer);
        internal abstract void Update();

        internal SteamAction()
        {
        }

        protected bool ValueCanBeUsed
        {
            get
            {
                if (LizardButton && Controller?.LizardButtons == true)
                    return false;
                if (LizardMouse && Controller?.LizardMouse == true)
                    return false;
                return true;
            }
        }
    }

    public class SteamButton : SteamAction
    {
        public static readonly TimeSpan DefaultHoldDuration = TimeSpan.FromMilliseconds(10);
        public static readonly TimeSpan DefaultFirstHold = TimeSpan.FromMilliseconds(400);
        public static readonly TimeSpan DefaultRepeatHold = TimeSpan.FromMilliseconds(45);

        private bool rawValue, rawLastValue;

        public bool Value
        {
            get { return ValueCanBeUsed ? rawValue : false; }
        }
        public bool LastValue
        {
            get { return ValueCanBeUsed ? rawLastValue : false; }
        }

        /// Last press was already consumed by other
        internal string? Consumed { get; private set; }

        /// Set on raising edge
        private DateTime? HoldSince { get; set; }
        private DateTime? HoldRepeated { get; set; }

        internal SteamButton()
        {
        }

        public static implicit operator bool(SteamButton button) => button.Hold(DefaultHoldDuration, null);

        /// Generated when button is pressed for the first time
        public bool JustPressed()
        {
            if (!LastValue && Value)
                return true;
            return false;
        }

        /// Generated on failing edge of key press
        public bool Pressed(TimeSpan? duration = null)
        {
            // We expect Last to be true, and now to be false (failing edge)
            if (!(LastValue && !Value))
                return false;

            if (Consumed is not null)
                return false;

            if (duration.HasValue && HoldSince?.Add(duration.Value) >= DateTime.Now)
                return false;

            return true;
        }

        private bool Consume(string consume)
        {
            if (Consumed is not null && Consumed != consume)
                return false;

            Consumed = consume;
            return true;
        }

        public bool Hold(string? consume)
        {
            return Hold(null, consume);
        }

        /// Generated when button was hold for a given period
        public bool Hold(TimeSpan? duration, string? consume)
        {
            if (!Value)
                return false;

            if (Consumed is not null && Consumed != consume)
                return false;

            if (duration.HasValue && HoldSince?.Add(duration.Value) >= DateTime.Now)
                return false;

            if (consume is not null)
                Consumed = consume;

            return true;
        }

        public bool HoldOnce(string consume)
        {
            return HoldOnce(null, consume);
        }

        /// Generated when button was hold for a given period
        /// but triggered exactly once
        public bool HoldOnce(TimeSpan? duration, string consume)
        {
            if (!Hold(duration, null))
                return false;

            Consumed = consume;
            return true;
        }

        /// Generated when button was hold for a given period
        /// but triggered exactly after previously being hold
        public bool HoldChain(TimeSpan? duration, string previousConsume, string replaceConsme)
        {
            if (!Hold(duration, previousConsume))
                return false;

            Consumed = replaceConsme;
            return true;
        }

        /// Generated when button was repeated for a given period
        /// but triggered exactly once
        public bool HoldRepeat(TimeSpan duration, TimeSpan repeatEvery, string? consume)
        {
            // always generate at least one keypress
            if (Pressed(duration))
                return true;

            if (!Hold(duration, consume))
                return false;

            // first keypress
            if (!HoldRepeated.HasValue)
            {
                HoldRepeated = DateTime.Now;
                return true;
            }

            // repeated keypress
            if (HoldRepeated.Value.Add(repeatEvery) <= DateTime.Now)
            {
                HoldRepeated = DateTime.Now;
                return true;
            }

            return false;
        }

        public bool HoldRepeat(string consume)
        {
            return HoldRepeat(DefaultFirstHold, DefaultRepeatHold, consume);
        }

        internal override void Reset()
        {
            rawLastValue = rawValue;
            rawValue = false;
            HoldSince = null;
            HoldRepeated = null;
            Consumed = null;
        }

        internal void SetValue(bool newValue)
        {
            rawLastValue = rawValue;
            rawValue = newValue;

            if (!rawLastValue && rawValue)
            {
                HoldSince = DateTime.Now;
                HoldRepeated = null;
            }
        }

        internal override bool BeforeUpdate(byte[] buffer)
        {
            return true;
        }

        internal override void Update()
        {
            if (!Value)
                Consumed = null;
        }

        public override string? ToString()
        {
            if (Name != "")
                return String.Format("{0}: {1} (last: {2})", Name, Value, LastValue);
            return base.ToString();
        }
    }

    public class SteamButton2 : SteamButton
    {
        private int offset;
        private uint mask;

        internal SteamButton2(int offset, object mask)
        {
            this.offset = offset;
            this.mask = (uint)mask.GetHashCode();

            while (this.mask > 0xFF)
            {
                this.mask >>= 8;
                this.offset++;
            }
        }

        internal override bool BeforeUpdate(byte[] buffer)
        {
            if (offset < buffer.Length)
            {
                SetValue((buffer[offset] & mask) != 0);
                return true;
            }
            else
            {
                SetValue((buffer[offset] & mask) != 0);
                return false;
            }
        }
    }

    public enum DeltaValueMode
    {
        Absolute,
        AbsoluteTime,
        Delta,
        DeltaTime
    }

    public class SteamAxis : SteamAction
    {
        public const short VirtualLeftThreshold = short.MinValue / 2;
        public const short VirtualRightThreshold = short.MaxValue / 2;

        private int offset;
        private short rawValue, rawLastValue;

        public SteamButton? ActiveButton { get; internal set; }
        public SteamButton? VirtualLeft { get; internal set; }
        public SteamButton? VirtualRight { get; internal set; }
        public short Deadzone { get; internal set; }
        public short MinChange { get; internal set; }
        public DeltaValueMode DeltaValueMode { get; internal set; } = DeltaValueMode.Absolute;

        public short Value
        {
            get { return ValueCanBeUsed ? rawValue : (short)0; }
        }
        public short LastValue
        {
            get { return ValueCanBeUsed ? rawLastValue : (short)0; }
        }

        public SteamAxis(int offset)
        {
            this.offset = offset;
        }

        public static implicit operator bool(SteamAxis button) => button.Active;
        public static implicit operator short(SteamAxis button)
        {
            return Math.Abs(button.Value) > button.Deadzone ? button.Value : (short)0;
        }

        public bool Active
        {
            get { return ActiveButton?.Value ?? true; }
        }

        public double DeltaValue
        {
            get { return GetDeltaValue(-1, 1, DeltaValueMode); }
        }

        public double GetDeltaValue(double min, double max, DeltaValueMode mode)
        {
            int value = 0;

            switch (mode)
            {
                case DeltaValueMode.Absolute:
                    if (Math.Abs(Value) < Deadzone)
                        return 0.0;
                    value = Value;
                    break;

                case DeltaValueMode.AbsoluteTime:
                    if (Math.Abs(Value) < Deadzone)
                        return 0.0;
                    value = (int)(Value * (Controller?.DeltaTime ?? 0.0));
                    break;

                case DeltaValueMode.Delta:
                    value = Value - LastValue;
                    if (Math.Abs(Value) < MinChange)
                        return 0.0;
                    break;

                case DeltaValueMode.DeltaTime:
                    value = Value - LastValue;
                    if (Math.Abs(Value) < MinChange)
                        return 0.0;
                    value = (int)(value * (Controller?.DeltaTime ?? 0.0));
                    break;
            }

            if (value == 0)
                return 0.0;

            double factor = (double)(value - short.MinValue) / (short.MaxValue - short.MinValue);
            return factor * (max - min) + min;
        }

        internal override void Reset()
        {
            rawLastValue = rawValue;
            rawValue = 0;
        }

        internal void SetValue(short newValue)
        {
            rawLastValue = rawValue;
            rawValue = newValue;

            // first time pressed, reset value as this is a Pad
            if (ActiveButton is not null && ActiveButton.JustPressed())
                rawLastValue = newValue;

            if (VirtualRight is not null)
                VirtualRight.SetValue(newValue > VirtualRightThreshold);

            if (VirtualLeft is not null)
                VirtualLeft.SetValue(newValue < VirtualLeftThreshold);
        }

        internal override bool BeforeUpdate(byte[] buffer)
        {
            if (offset + 1 < buffer.Length)
            {
                SetValue(BitConverter.ToInt16(buffer, offset));
                return true;
            }
            else
            {
                SetValue(0);
                return false;
            }
        }

        internal override void Update()
        {
        }

        public override string? ToString()
        {
            if (Name != "")
                return String.Format("{0}: {1} (last: {2})", Name, Value, LastValue);
            return base.ToString();
        }
    }
}
