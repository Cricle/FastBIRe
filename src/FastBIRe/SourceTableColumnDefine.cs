namespace FastBIRe
{
    public static class DefaultDateTimePartNames
    {
        public const string SystemPrefx = "__$";

        public const string Year = "_year";
        public const string Month = "_month";
        public const string Day = "_day";
        public const string Hour = "_hour";
        public const string Minute = "_minute";
        public const string Quarter = "_quarter";
        public const string Week = "_week";

        public static string CombineField(string field, string part)
        {
            return SystemPrefx + field + part;
        }
        public static IReadOnlyList<KeyValuePair<string, ToRawMethod>> GetDatePartNames(string field)
        {
            return new KeyValuePair<string, ToRawMethod>[]
            {
                new KeyValuePair<string, ToRawMethod>(CombineField(field,Year), ToRawMethod.Year),
                new KeyValuePair<string, ToRawMethod>(CombineField(field,Month), ToRawMethod.Month),
                new KeyValuePair<string, ToRawMethod>(CombineField(field,Day), ToRawMethod.Day),
                new KeyValuePair<string, ToRawMethod>(CombineField(field,Hour), ToRawMethod.Hour),
                new KeyValuePair<string, ToRawMethod>(CombineField(field,Minute), ToRawMethod.Minute),
                new KeyValuePair<string, ToRawMethod>(CombineField(field,Quarter), ToRawMethod.Quarter),
                new KeyValuePair<string, ToRawMethod>(CombineField(field,Week), ToRawMethod.Week),
            };
        }
        public static string GetField(ToRawMethod method, string field, out bool ok)
        {
            ok = true;
            switch (method)
            {
                case ToRawMethod.Year:
                    return CombineField(field, Year);
                case ToRawMethod.Month:
                    return CombineField(field, Month);
                case ToRawMethod.Day:
                    return CombineField(field, Day);
                case ToRawMethod.Hour:
                    return CombineField(field, Hour);
                case ToRawMethod.Minute:
                    return CombineField(field, Minute);
                case ToRawMethod.Quarter:
                    return CombineField(field, Quarter);
                case ToRawMethod.Week:
                    return CombineField(field, Week);
                default:
                    ok = false;
                    return field;
            }
        }
    }
    public record SourceTableColumnDefine : TableColumnDefine
    {
        public SourceTableColumnDefine(string? field, string raw, bool isGroup, TableColumnDefine destColumn, ToRawMethod method, string rawFormat, bool onlySet = false)
            : base(field, raw, rawFormat, onlySet)
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
        public new SourceTableColumnDefine SetIndex(string group, int order = 0, bool desc = false)
        {
            base.SetIndex(group, order, desc);
            return this;
        }

        public SourceTableColumnDefine SetExpandDateTime(bool expand = true, bool destExpand = false)
        {
            base.SetExpandDateTime(expand);
            if (destExpand && DestColumn != null)
            {
                DestColumn.SetExpandDateTime(destExpand);
            }
            return this;
        }
        public new SourceTableColumnDefine SetExpandDateTime(bool expand = true)
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
