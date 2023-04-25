namespace FastBIRe
{
    public record CompileOptions
    {
        public bool IncludeEffectJoin { get; set; }

        public string? EffectTable { get; set; }

        public bool NoLock { get; set; }

        public bool UseExpandField { get; set; }

        public CompileOptions WithNoLock(bool noLock = true)
        {
            NoLock = noLock;
            return this;
        }

        public static CompileOptions EffectJoin(string effectTable)
        {
            return new CompileOptions { IncludeEffectJoin = true, EffectTable = effectTable };
        }
    }
}
