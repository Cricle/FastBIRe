using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using System.Data;

namespace FastBIRe
{
    public class SourceTableColumnBuilder
    {
        public const int DefaultStringLen = 255;
        public const int DefaultNumberLen = 13;
        public const int DefaultSqliteNumberLen = 16;
        public const int DefaultDateTimeLen = 8;
        public const int DefaultSqliteDateTimeLen = 23;

        public SourceTableColumnBuilder(MergeHelper helper, string? sourceAlias = null, string? destAlias = null)
        {
            Helper = helper;
            SourceAlias = sourceAlias;
            DestAlias = destAlias;
            NumberLen = helper.SqlType == SqlType.SQLite ? DefaultSqliteNumberLen : DefaultNumberLen;
            DateTimeLen = helper.SqlType == SqlType.SQLite ? DefaultSqliteDateTimeLen : DefaultDateTimeLen;

            if (!string.IsNullOrEmpty(sourceAlias))
            {
                SourceAliasQuto = helper.Wrap(sourceAlias!);
            }
            if (!string.IsNullOrEmpty(destAlias))
            {
                DestAliasQuto = helper.Wrap(destAlias!);
            }
        }

        public MergeHelper Helper { get; }

        public string? SourceAlias { get; }

        public string? SourceAliasQuto { get; }

        public string? DestAlias { get; }

        public string? DestAliasQuto { get; }

        public int StringLen { get; set; } = DefaultStringLen;

        public int DateTimeLen { get; set; } = DefaultDateTimeLen;

        public int NumberLen { get; set; }

        public int Precision { get; set; } = 25;

        public int Scale { get; set; } = 5;

        public WhereItem WhereRaw(string field, ToRawMethod method, string rawValue)
        {
            var sourceRaw = string.IsNullOrEmpty(SourceAlias) ? string.Empty : $"{Helper.Wrap(SourceAlias)}." + Helper.Wrap(field);
            var raw = Helper.ToRaw(method, sourceRaw, false);
            var val = Helper.ToRaw(method, rawValue, false);
            return new WhereItem(field, raw, val);
        }
        public WhereItem Where(string field, ToRawMethod method, object value)
        {
            return WhereRaw(field, method, Helper.MethodWrapper.WrapValue(value)!);
        }
        public string? Type(DbType dbType, params object[] formatArgs)
        {
            var rt = DatabaseReader.FindDataTypesByDbType(Helper.SqlType, dbType).TypeName;
            if (string.IsNullOrEmpty(rt))
            {
                return null;
            }
            var dataType = DatabaseReader.GetDataTypes(Helper.SqlType).FirstOrDefault(x => string.Equals(x.TypeName, rt, StringComparison.OrdinalIgnoreCase));
            if (dataType == null)
            {
                return rt;
            }
            if (dbType == DbType.String&&!dataType.CreateFormat.Contains("{0}"))
            {
                return dataType.TypeName;
            }
            return string.Format(dataType.CreateFormat, formatArgs);
        }
        public void FillColumns(IEnumerable<SourceTableColumnDefine> columns, DatabaseTable sourceTable, DatabaseTable destTable)
        {
            FillColumns(columns, sourceTable);
            FillColumns(columns.Select(x => x.DestColumn), destTable);
        }
        public void FillColumns(IEnumerable<TableColumnDefine> columns, DatabaseTable table)
        {
            foreach (var column in columns)
            {
                var col = table.FindColumn(column.Field);
                if (col != null)
                {
                    column.Type = col.DbDataType;
                }
            }
        }
        public IEnumerable<TableColumnDefine> CloneWith(IEnumerable<TableColumnDefine> sources, Func<TableColumnDefine, TableColumnDefine> define)
        {
            foreach (var item in sources)
            {
                var def = new TableColumnDefine(item.Field, item.Raw, item.RawFormat, item.OnlySet)
                {
                    Type = item.Type,
                    Id = item.Id,
                };
                var res = define(def);
                if (res != null)
                {
                    yield return def;
                }
            }
        }
        public IEnumerable<SourceTableColumnDefine> CloneWith(IEnumerable<SourceTableColumnDefine> sources, Func<SourceTableColumnDefine, SourceTableColumnDefine> define)
        {
            foreach (var item in sources)
            {
                var def = new SourceTableColumnDefine(item.Field, item.Raw, item.IsGroup, item.DestColumn, item.Method, item.Raw, item.OnlySet)
                {
                    Type = item.Type,
                    Id = item.Id,
                };
                var res = define(def);
                if (res != null)
                {
                    yield return def;
                }
            }
        }
        public TableColumnDefine Column(string field, string? type = null, bool destNullable = true, int length = 0, string? id = null,string? indexGroup=null,int indexOrder=0)
        {
            var destFormat = string.IsNullOrEmpty(DestAlias) ? Helper.Wrap(field) : $"{Helper.Wrap("{0}")}." + Helper.Wrap(field);
            var destRaw = string.Format(destFormat, SourceAlias);
            return new TableColumnDefine(field, destRaw, destFormat, false)
            {
                Type = type,
                Nullable = destNullable,
                Length = length,
                Id = id, 
                IndexGroup=indexGroup,
                IndexOrder=indexOrder
            };
        }
        public SourceTableColumnDefine Method(string field, ToRawMethod method, TableColumnDefine destColumn, bool isGroup = false, bool onlySet = false, string? type = null,
            bool sourceNullable = true, int length = 0)
        {
            var sourceFormat = string.IsNullOrEmpty(SourceAlias) ? string.Empty : $"{Helper.Wrap("{0}")}." + Helper.Wrap(field);
            var sourceRaw = string.Format(sourceFormat, SourceAlias);

            var rawFormat = Helper.ToRaw(method, "{0}", false);
            var raw = string.Format(rawFormat, sourceRaw);

            return new SourceTableColumnDefine(field,
                raw,
                isGroup,
                destColumn,
                method, rawFormat, onlySet)
            {
                Type = type,
                Nullable = sourceNullable,
                Length = length
            };
        }
        public SourceTableColumnDefine MethodRaw(string destField, string rawFormat, string? field = null, bool isGroup = false, bool onlySet = false, string? type = null,
            string? destFieldType = null, bool sourceNullable = true, bool destNullable = true, int length = 0, int destLength = 0)
        {
            var sourceFormat = string.IsNullOrEmpty(SourceAlias) ? string.Empty : $"{Helper.Wrap("{0}")}." + Helper.Wrap(field);
            var sourceRaw = string.Format(sourceFormat, SourceAlias);

            var raw = string.Format(rawFormat, sourceRaw);

            return new SourceTableColumnDefine(field,
                raw,
                isGroup,
                Column(destField, destFieldType, destNullable, destLength),
                ToRawMethod.Unknow, rawFormat, onlySet)
            {
                Type = type,
                Nullable = sourceNullable,
                Length = length,
            };
        }
        public SourceTableColumnDefine Method(string field, string destField, ToRawMethod method, bool isGroup = false, bool onlySet = false, string? type = null,
            string? destFieldType = null, bool sourceNullable = true, bool destNullable = true, int length = 0, int destLength = 0)
        {
            var sourceFormat = string.IsNullOrEmpty(SourceAlias) ? string.Empty : $"{Helper.Wrap("{0}")}." + Helper.Wrap(field);
            var sourceRaw = string.Format(sourceFormat, SourceAlias);

            var rawFormat = Helper.ToRaw(method, "{0}", false);
            var raw = string.Format(rawFormat, sourceRaw);

            return new SourceTableColumnDefine(field,
                raw,
                isGroup,
                Column(destField, destFieldType, destNullable, destLength),
                method, rawFormat, onlySet)
            {
                Type = type,
                Nullable = sourceNullable,
                Length = length,
            };
        }
        private static bool NeedString(ToRawMethod method)
        {
            switch (method)
            {
                case ToRawMethod.Quarter:
                case ToRawMethod.Week:
                    return true;
                default:
                    return false;
            }
        }
        private static bool IsAggerMethod(ToRawMethod method)
        {
            switch (method)
            {
                case ToRawMethod.Year:
                case ToRawMethod.Month:
                case ToRawMethod.Day:
                case ToRawMethod.Hour:
                case ToRawMethod.Minute:
                case ToRawMethod.Second:
                case ToRawMethod.Week:
                case ToRawMethod.Quarter:
                    return true;
                default:
                    return false;
            }
        }
        public int GetDecimalByteLen(int len)
        {
            if (Helper.SqlType == SqlType.SqlServer || Helper.SqlType == SqlType.SqlServerCe)
            {
                return ((len + 1) / 2) + 1;
            }
            return (len / 9 + 1) * 4;
        }
        public SourceTableColumnDefine DateTime(string field,
            string destField,
            ToRawMethod method,
            bool isGroup = false,
            bool onlySet = false,
            bool sourceNullable = true,
            bool destNullable = true)
        {
            return Method(field,
                destField,
                method,
                isGroup,
                onlySet,
                Type(DbType.DateTime),
                Type(DbType.DateTime),
                sourceNullable,
                destNullable,
                DateTimeLen,
                DateTimeLen);
        }
        public SourceTableColumnDefine Decimal(string field,
            string destField,
            ToRawMethod method,
            bool isGroup = false,
            bool onlySet = false,
            bool sourceNullable = true,
            bool destNullable = true,
            int? precision = null,
            int? scale = null)
        {
            var isAggerMethod = IsAggerMethod(method);
            return Method(field,
                destField,
                method,
                isGroup,
                onlySet,
                Type(DbType.Decimal, precision ?? Precision, scale ?? Scale),
                isAggerMethod ? Type(DbType.String, StringLen) : Type(DbType.Decimal, precision ?? Precision, scale ?? Scale),
                sourceNullable,
                destNullable,
                GetDecimalByteLen(precision ?? Precision),
                isAggerMethod ? StringLen : NumberLen);
        }
        public SourceTableColumnDefine String(string field,
            string destField,
            ToRawMethod method,
            bool isGroup = false,
            bool onlySet = false,
            bool sourceNullable = true,
            bool destNullable = true,
            int? len = null)
        {
            return Method(field,
                destField,
                method,
                isGroup,
                onlySet,
                Type(DbType.String, len ?? StringLen),
                IsAggerMethod(method) ? Type(DbType.String, len ?? StringLen) : Type(DbType.String, len ?? StringLen),
                sourceNullable,
                destNullable,
                len ?? StringLen,
                len ?? StringLen);
        }

        public SourceTableColumnDefine DateTimeRaw(string destField,
            string rawFormat,
            string? field = null,
            bool isGroup = false,
            bool onlySet = false,
            bool sourceNullable = true,
            bool destNullable = true)
        {
            return MethodRaw(destField,
                rawFormat,
                field,
                isGroup,
                onlySet,
                Type(DbType.DateTime),
                Type(DbType.DateTime),
                sourceNullable,
                destNullable,
                DateTimeLen,
                DateTimeLen);
        }
        public SourceTableColumnDefine DecimalRaw(string destField,
            string rawFormat,
            string? field = null,
            bool isGroup = false,
            bool onlySet = false,
            bool sourceNullable = true,
            bool destNullable = true,
            int? precision = null,
            int? scale = null,
            bool needString = true)
        {
            return MethodRaw(destField,
                rawFormat,
                field,
                isGroup,
                onlySet,
                Type(DbType.Decimal, precision ?? Precision, scale ?? Scale),
                needString ? Type(DbType.String, StringLen) : Type(DbType.Decimal, precision ?? Precision, scale ?? Scale),
                sourceNullable,
                destNullable,
                GetDecimalByteLen(precision ?? Precision),
                needString ? StringLen : NumberLen);
        }
        public SourceTableColumnDefine StringRaw(
            string destField,
            string rawFormat,
            string? field = null,
            bool isGroup = false,
            bool onlySet = false,
            bool sourceNullable = true,
            bool destNullable = true,
            int? len = null)
        {
            return MethodRaw(destField,
                rawFormat,
                field,
                isGroup,
                onlySet,
                Type(DbType.String, len ?? StringLen),
                Type(DbType.String, len ?? StringLen),
                sourceNullable,
                destNullable,
                len ?? StringLen,
                len ?? StringLen);
        }
    }
}
