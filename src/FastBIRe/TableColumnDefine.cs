using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public record TableColumnDefine
    {
        public TableColumnDefine(string field, string raw, string rawFormat, bool onlySet = false)
        {
            Field = field;
            Raw = raw;
            RawFormat = rawFormat;
            OnlySet = onlySet;
        }

        public string? Field { get; set; }

        public string? Raw { get; set; }

        public string? RawFormat { get; set; }

        public bool OnlySet { get; set; }

        public string? Type { get; set; }

        public string? Id { get; set; }

        public bool Nullable { get; set; } = true;

        public int Length { get; set; }

        public bool ExpandDateTime { get; set; }

        public string? IndexGroup { get; set; }

        public int IndexOrder { get; set; }

        public bool Desc { get; set; }

        public TableColumnDefine SetIndex(string group, int order = 0, bool desc = false)
        {
            IndexGroup = group;
            IndexOrder = order;
            Desc = desc;
            return this;
        }

        public TableColumnDefine SetExpandDateTime(bool expand = true)
        {
            ExpandDateTime = expand;
            return this;
        }
    }
}
