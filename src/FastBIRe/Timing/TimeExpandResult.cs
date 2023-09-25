namespace FastBIRe.Timing
{
    public readonly record struct TimeExpandResult: IExpandResult
    {
        /// <summary>
        /// Gets the value of current result time
        /// </summary>
        public TimeTypes Type { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string? ExparessionFormatter { get; }

        /// <inheritdoc/>
        public string OriginName { get; }

        public TimeExpandResult(TimeTypes type, string name, string originName, string? trigger)
        {
            Type = type;
            Name = name;
            ExparessionFormatter = trigger;
            OriginName = originName;
        }
        /// <inheritdoc/>
        public string FormatExpression(string input)
        {
            return string.Format(ExparessionFormatter, input);
        }
    }
}
