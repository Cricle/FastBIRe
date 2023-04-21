namespace FastBIRe
{
    public record SourceTableColumnDefine(string Field, string Raw, bool IsGroup, TableColumnDefine DestColumn, ToRawMethod Method, string RawFormat, bool OnlySet = false)
        : TableColumnDefine(Field, Raw, RawFormat, OnlySet)
    {
        public SourceTableColumnDefine Copy()
        {
            return new SourceTableColumnDefine(this);
        }

        public SourceTableColumnDefine AllNotNull()
        {
            Nullable = false;
            if (DestColumn != null)
            {
                DestColumn.Nullable = false;
            }
            return this;
        }
        public new SourceTableColumnDefine Compute(string define)
        {
            base.Compute(define);
            return this;
        }
        public new SourceTableColumnDefine Compute(SourceTableColumnBuilder builder, ToRawMethod method, string forField, string tableName="NEW")
        {
            base.Compute(builder, method, forField, tableName);
            return this;
        }
    }
}
