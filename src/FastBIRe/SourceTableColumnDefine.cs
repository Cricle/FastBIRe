using System.Data;

namespace FastBIRe
{
    public static class DefaultDateTimePartNames
    {
        public const string Year = "_year";
        public const string Month = "_month";
        public const string Day = "_day";
        public const string Hour = "_hour";
        public const string Minute = "_minute";
        public const string Quarter = "_quarter";
        public const string Weak = "_weak";
    }
    public record SourceTableColumnDefine: TableColumnDefine
    {
        public SourceTableColumnDefine(string field, string raw, bool isGroup, TableColumnDefine destColumn, ToRawMethod method, string rawFormat, bool onlySet = false)
            :base(field,raw,rawFormat,onlySet)
        {
            Field = field;
            Raw = raw;
            IsGroup = isGroup;
            DestColumn = destColumn;
            Method = method;
            RawFormat = rawFormat;
            OnlySet = onlySet;
        }

        public bool IsGroup { get; set; }

        public TableColumnDefine DestColumn { get; set; }

        public ToRawMethod Method { get; set; }

        public SourceTableColumnDefine Copy()
        {
            return new SourceTableColumnDefine(this);
        }
        public new SourceTableColumnDefine SetExpandDateTime(bool expand=true)
        {
            base.SetExpandDateTime(expand);
            return this;
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
    }
}
