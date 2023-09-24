namespace FastBIRe.Timing
{
    public readonly record struct TimeExpandResult
    {
        public readonly TimeTypes Type;

        public readonly string Name;

        public readonly string? Trigger;

        public TimeExpandResult(TimeTypes type, string name, string? trigger)
        {
            Type = type;
            Name = name;
            Trigger = trigger;
        }
    }
}
