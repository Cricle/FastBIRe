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
        public string? Type(DbType dbType,params object[] formatArgs)
        {
            var rt = DatabaseReader.FindDataTypesByDbType(Helper.SqlType, dbType);
            if (string.IsNullOrEmpty(rt))
            {
                return null;
            }
            var dataType = DatabaseReader.GetDataTypes(Helper.SqlType).FirstOrDefault(x =>string.Equals(x.TypeName,rt, StringComparison.OrdinalIgnoreCase));
            if (dataType==null)
            {
                return rt;
            }
            if (dbType == DbType.String && "text".Equals(rt, StringComparison.OrdinalIgnoreCase))
            {
                return dataType.TypeName;
            }
            return string.Format(dataType.CreateFormat, formatArgs);
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
        public IEnumerable<SourceTableColumnDefine> CloneWith(IEnumerable<SourceTableColumnDefine> sources,Func<SourceTableColumnDefine, SourceTableColumnDefine> define)
        {
            foreach (var item in sources)
            {
                var def = new SourceTableColumnDefine(item.Field, item.Raw, item.IsGroup, item.DestColumn, item.Method, item.Raw, item.OnlySet)
                {
                    Type = item.Type,
                    Id = item.Id,
                };
                var res=define(def);
                if (res != null)
                {
                    yield return def;
                }
            }
        }
        public SourceTableColumnDefine Method(string field, string destField, ToRawMethod method, bool isGroup = false, bool onlySet = false, string? type = null, string? destFieldType = null,bool sourceNullable=true,bool destNullable=true)
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
                new TableColumnDefine(destField, destRaw, destFormat, false) { Type= destFieldType ,Nullable=destNullable},
                method, rawFormat, onlySet)
            { 
                Type= type,
                Nullable= sourceNullable,
            };
        }
    }
}
