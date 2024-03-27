namespace FastBIRe
{
    public readonly struct InterpolatedResult
    {
        public InterpolatedResult(string sql, string format, KeyValuePair<string, object?>[] arguments, FormattableString raw)
        {
            Sql = sql;
            Format = format;
            Arguments = arguments;
            Raw = raw;
        }

        public string Sql { get; }

        public string Format { get; }

        public KeyValuePair<string, object?>[] Arguments { get; }

        public FormattableString Raw { get; }

        public List<FormatResult> GetFormatResults()
        {
            return InterpolatedHelper.GetFormatResults(Format);
        }

        public string RawSql
        {
            get
            {
                var args = InterpolatedHelper.AllocArray<object?>(Arguments.Length);
                for (int i = 0; i < Arguments.Length; i++)
                {
                    args[i] = Arguments[i].Value;
                }
                return string.Format(Format, args);
            }
        }
        public override string ToString()
        {
            return RawSql;
        }
    }
}
