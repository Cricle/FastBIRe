namespace FastBIRe
{
    public record TableColumnDefine(string Field, string Raw, string RawFormat, bool OnlySet = false)
    {
        public string? Type { get; set; }

        public string? Id { get; set; }

        public bool Nullable { get; set; } = true;

        public int Length { get; set; } = 255;

        public int Precision { get; set; } = 25;

        public int Scale { get; set; } = 5;
    }
}
