namespace Diagnostics.Traces.Mini
{
    public readonly struct CounterValue
    {
        public CounterValue(DateTime time, double?[] values, string[] columns)
        {
            Time = time;
            Values = values;
            Columns = columns;
        }

        public DateTime Time { get; }

        public double?[] Values { get; }

        public string[] Columns { get; }
    }
}
