namespace FastBIRe
{
    public record SourceTableDefine(string Table, IReadOnlyList<SourceTableColumnDefine> Columns)
    {
        public List<TableColumnDefine> DestColumn => Columns.Select(x => x.DestColumn).ToList();
    }
}
