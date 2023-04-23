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
    public static class TableColumnDateTimeExtensions
    {
        public static IList<SourceTableColumnDefine> SetGroup(this IList<SourceTableColumnDefine> defines,params string[] fieldNames)
        {
            foreach (var item in fieldNames)
            {
                var field = defines.First(x => x.Field == item);
                field.IsGroup = true;
            }
            return defines;
        }
        public static IList<TableColumnDefine> AddDateTimeParts(this IList<TableColumnDefine> defines, SourceTableColumnBuilder builder,string field,string tableName="NEW")
        {
            var col = defines.First(x => x.Field == field);
            defines.Add(builder.Column(col.Field+ DefaultDateTimePartNames.Year,builder.Type(DbType.DateTime)).Compute(builder, ToRawMethod.Year, field, tableName));
            defines.Add(builder.Column(col.Field + DefaultDateTimePartNames.Month, builder.Type(DbType.DateTime)).Compute(builder, ToRawMethod.Month, field, tableName));
            defines.Add(builder.Column(col.Field + DefaultDateTimePartNames.Day, builder.Type(DbType.DateTime)).Compute(builder, ToRawMethod.Day, field, tableName));
            defines.Add(builder.Column(col.Field + DefaultDateTimePartNames.Hour, builder.Type(DbType.DateTime)).Compute(builder, ToRawMethod.Hour, field, tableName));
            defines.Add(builder.Column(col.Field + DefaultDateTimePartNames.Minute, builder.Type(DbType.DateTime)).Compute(builder, ToRawMethod.Minute, field, tableName));
            defines.Add(builder.Column(col.Field + DefaultDateTimePartNames.Quarter, builder.Type(DbType.String,19)).Compute(builder, ToRawMethod.Quarter, field, tableName));
            defines.Add(builder.Column(col.Field + DefaultDateTimePartNames.Weak, builder.Type(DbType.String, 19)).Compute(builder, ToRawMethod.Weak, field, tableName));
            return defines;
        }
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
