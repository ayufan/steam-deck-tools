namespace CommonHelpers
{
    public struct TimedValue<T> where T : struct
    {
        public T Value { get; }
        public DateTime ExpiryDate { get; }

        public bool Valid
        {
            get => !Expired;
        }

        public bool Expired
        {
            get => ExpiryDate < DateTime.UtcNow;
        }

        public TimedValue(T value, TimeSpan ts)
        {
            this.Value = value;
            this.ExpiryDate = DateTime.UtcNow.Add(ts);
        }

        public TimedValue(T value, int timeoutMs)
            : this(value, TimeSpan.FromMilliseconds(timeoutMs))
        {
        }

        public bool TryGetValue(out T value)
        {
            value = this.Value;
            return Valid;
        }

        public T? GetValue()
        {
            return Valid ? Value : null;
        }

        public T GetValueOrDefault(T defaultValue)
        {
            return Valid ? Value : defaultValue;
        }

        public static implicit operator T?(TimedValue<T> tv)
        {
            return tv.GetValue();
        }
    }
}
