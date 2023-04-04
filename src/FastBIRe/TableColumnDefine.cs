namespace FastBIRe
{
    public record TableColumnDefine(string Field, string Raw, string RawFormat, bool OnlySet = false, string? Type = null);
}
