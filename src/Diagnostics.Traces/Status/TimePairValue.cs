namespace Diagnostics.Traces.Status
{
    public readonly record struct TimePairValue
    {
        public TimePairValue(DateTime time, string value)
        {
            Time = time;
            Value = value;
        }

        public DateTime Time { get; }

        public string Value { get; }
    }
}