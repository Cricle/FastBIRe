namespace FastBIRe
{
    public record TableColumnDefine(string Field, string Raw, string RawFormat, bool OnlySet = false)
    {
        public string? Type { get; set; }
    }
}
