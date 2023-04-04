namespace FastBIRe
{
    public record SourceTableDefine(string Table, IReadOnlyList<SourceTableColumnDefine> Columns);
}
