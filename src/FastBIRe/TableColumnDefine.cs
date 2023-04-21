namespace FastBIRe
{
    public record TableColumnDefine(string Field, string Raw, string RawFormat, bool OnlySet = false)
    {
        public string? Type { get; set; }

        public string? Id { get; set; }

        public bool Nullable { get; set; } = true;

        public int Length { get; set; }

        public string? ComputeDefine { get; set; }

        public bool IsCompute => !string.IsNullOrEmpty(ComputeDefine);

        public TableColumnDefine Compute(string define)
        {
            ComputeDefine = define;
            return this;
        }
        public TableColumnDefine Compute(SourceTableColumnBuilder builder,ToRawMethod method, string forField, string tableName = "NEW")
        {
            ComputeDefine = builder.WriteTimePart($"{tableName}.{builder.Helper.Wrap(forField)}", method,false);
            return this;
        }
    }
}
