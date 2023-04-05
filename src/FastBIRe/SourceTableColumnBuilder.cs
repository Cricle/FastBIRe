using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
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
        public string Type(DbType dbType)
        {
            return DatabaseReader.FindDataTypesByDbType(Helper.SqlType, dbType);
        }
        public void FillColumns(IEnumerable<SourceTableColumnDefine> columns, DatabaseTable sourceTable,DatabaseTable destTable)
        {
            FillColumns(columns, sourceTable);
            FillColumns(columns.Select(x => x.DestColumn), destTable);
        }
        public void FillColumns(IEnumerable<TableColumnDefine> columns,DatabaseTable table)
        {
            foreach (var column in columns)
            {
                var col = table.FindColumn(column.Field);
                if (col!=null)
                {
                    column.Type = col.DbDataType;
                }
            }
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
    }
}
