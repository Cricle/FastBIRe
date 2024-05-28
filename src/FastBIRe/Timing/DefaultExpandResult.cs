namespace FastBIRe.Timing
{
    public readonly record struct DefaultExpandResult : IExpandResult
    {
        public DefaultExpandResult(string originName, string name, string? exparessionFormatter)
        {
            OriginName = originName;
            Name = name;
            ExparessionFormatter = exparessionFormatter;
        }

        public string OriginName { get; }

        public string Name { get; }

        public string? ExparessionFormatter { get; }

        public string FormatExpression(string input)
        {
            if (ExparessionFormatter == null)
            {
                return string.Empty;
            }
            return string.Format(ExparessionFormatter, input);
        }

        public static DefaultExpandResult Expression(string field, string? expressionFormatter)
        {
            return new DefaultExpandResult(field, field, expressionFormatter);
        }
    }
}
