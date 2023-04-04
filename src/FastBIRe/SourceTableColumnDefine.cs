namespace FastBIRe
{
    public record SourceTableColumnDefine(string Field, string Raw, bool IsGroup, TableColumnDefine DestColumn, ToRawMethod Method, string RawFormat, bool OnlySet = false, string? Type = null)
        : TableColumnDefine(Field, Raw, RawFormat, OnlySet, Type);
}
