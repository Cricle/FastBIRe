namespace FastBIRe
{
    public record TableColumnDefine(string Field, string Raw, string RawFormat, bool OnlySet = false)
    {
        public string? Type { get; set; }

        public string? Id { get; set; }

        public bool Nullable { get; set; } = true;
    }
}
