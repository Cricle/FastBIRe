using DatabaseSchemaReader;
using System.Data;

namespace FastBIRe
{
    public class SourceTableColumnBuilder
    {
        public SourceTableColumnBuilder(MergeHelper helper, string? sourceAlias = null, string? destAlias = null)
        {
            Helper = helper;
            SourceAlias = sourceAlias;
            DestAlias = destAlias;
        }

        public MergeHelper Helper { get; }

        public string? SourceAlias { get; }

        public string? DestAlias { get; }

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
        public SourceTableColumnDefine Method(string field, string destField, ToRawMethod method, bool isGroup = false, bool onlySet = false, string? type = null, string? destFieldType = null)
        {
            var sourceFormat = string.IsNullOrEmpty(SourceAlias) ? string.Empty : $"{Helper.Wrap("{0}")}." + Helper.Wrap(field);
            var destFormat = string.IsNullOrEmpty(DestAlias) ? string.Empty : $"{Helper.Wrap("{0}")}." + Helper.Wrap(destField);
            var sourceRaw = string.Format(sourceFormat, SourceAlias);
            var destRaw = string.Format(destFormat, SourceAlias);

            var rawFormat = Helper.ToRaw(method, method == ToRawMethod.DistinctCount ? "{{0}}" : "{0}", false);
            var raw = string.Format(rawFormat, sourceRaw);

            return new SourceTableColumnDefine(field,
                raw,
                isGroup,
                new TableColumnDefine(destField, destRaw, destFormat, false, destFieldType),
                method, rawFormat, onlySet, type);
        }
        public string? GetRawTypeByStartSuffer(string start, object? formatArg0 = null)
        {
            var dataLists = DatabaseReader.GetDataTypes(DbTypeHelper.CastSqlType(Helper.SqlType));
            var target = dataLists.FirstOrDefault(x => x.TypeName.StartsWith(start));
            if (target == null)
            {
                return null;
            }
            return string.Format(target.CreateFormat, formatArg0);
        }
        public string? GetRawType(string netDataType, object? formatArg0 = null)
        {
            var dataLists = DatabaseReader.GetDataTypes(DbTypeHelper.CastSqlType(Helper.SqlType));
            var target = dataLists.FirstOrDefault(x => x.NetDataType == netDataType);
            if (target == null)
            {
                return null;
            }
            return string.Format(target.CreateFormat, formatArg0);
        }
        public string? GetRawType(DbType type, object? formatArg0 = null)
        {
            switch (type)
            {
                case DbType.Binary:
                    return GetRawType("System.Byte[]", formatArg0);
                case DbType.SByte:
                case DbType.Byte:
                    return GetRawType("System.Byte", formatArg0);
                case DbType.Boolean:
                    return GetRawType("System.Boolean", formatArg0);
                case DbType.Currency:
                    return GetRawType("System.Byte[]", formatArg0);
                case DbType.Date:
                    return GetRawType("System.DateTime", formatArg0);
                case DbType.DateTime:
                    return GetRawType("System.DateTime", formatArg0);
                case DbType.Decimal:
                    return GetRawType("System.Decimal", formatArg0);
                case DbType.Double:
                    return GetRawType("System.Double", formatArg0);
                case DbType.Guid:
                    return GetRawType("System.Guid", formatArg0);
                case DbType.UInt16:
                case DbType.Int16:
                    return GetRawType("System.Int16", formatArg0);
                case DbType.UInt32:
                case DbType.Int32:
                    return GetRawType("System.Int32", formatArg0);
                case DbType.Int64:
                case DbType.UInt64:
                    return GetRawType("System.Int64", formatArg0);
                case DbType.Single:
                    return GetRawType("System.Single", formatArg0);
                case DbType.StringFixedLength:
                case DbType.AnsiStringFixedLength:
                case DbType.AnsiString:
                case DbType.String:
                    return GetRawTypeByStartSuffer("varchar", formatArg0);
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                case DbType.Time:
                    return GetRawType("System.DateTime", formatArg0);
                default:
                    return null;
            }
        }
    }
}
