namespace Diagnostics.Traces.Mini
{
    public readonly struct CounterValue
    {
        public CounterValue(DateTime time, double?[] values)
        {
            Time = time;
            Values = values;
        }

        public DateTime Time { get; }

        public double?[] Values { get; }
    }
}
