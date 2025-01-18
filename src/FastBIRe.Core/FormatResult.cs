namespace FastBIRe
{
    public readonly struct FormatResult
    {
        public FormatResult(string rawString, int rawStart, int rawEnd, int indexStart, int indexEnd, int formatStart, int formatEnd)
        {
            RawString = rawString;
            RawStart = rawStart;
            RawEnd = rawEnd;
            IndexStart = indexStart;
            IndexEnd = indexEnd;
            FormatStart = formatStart;
            FormatEnd = formatEnd;
        }

        public string RawString { get; }

        public int RawStart { get; }

        public int RawEnd { get; }

        public int IndexStart { get; }

        public int IndexEnd { get; }

        public int FormatStart { get; }

        public int FormatEnd { get; }

        public ReadOnlySpan<char> Raw => RawString.AsSpan(RawStart, RawEnd);

        public ReadOnlySpan<char> Index => RawString.AsSpan(IndexStart, IndexEnd);

        public ReadOnlySpan<char> Format
        {
            get
            {
                if (FormatStart == -1)
                {
                    return default;
                }
                return RawString.AsSpan(FormatStart, FormatEnd);
            }
        }

        public override string ToString()
        {
            return $"{{Raw:{Raw.ToString()}, Index:{Index.ToString()}, Format:{Format.ToString()}}}";
        }
    }
}
