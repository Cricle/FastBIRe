namespace FastBIRe
{
    public record CompileOptions
    {
        public bool IncludeEffectJoin { get; set; }

        public string? EffectTable { get; set; }

        public static CompileOptions EffectJoin(string effectTable)
        {
            return new CompileOptions { IncludeEffectJoin = true, EffectTable = effectTable };
        }
    }
}
