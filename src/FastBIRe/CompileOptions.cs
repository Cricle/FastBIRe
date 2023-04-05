namespace FastBIRe
{
    public record CompileOptions
    {
        public bool IncludeEffectJoin { get; set; }

        public string? EffectTable { get; set; }
    }
}
