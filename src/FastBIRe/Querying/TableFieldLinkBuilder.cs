using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using FastBIRe.Timing;

namespace FastBIRe.Querying
{
    public class TableFieldLinkBuilder
    {
        public TableFieldLinkBuilder(DatabaseTable sourceTable, DatabaseTable destTable, FunctionMapper functionMapper)
        {
            SourceTable = sourceTable ?? throw new ArgumentNullException(nameof(sourceTable));
            DestTable = destTable ?? throw new ArgumentNullException(nameof(destTable));
            FunctionMapper = functionMapper ?? throw new ArgumentNullException(nameof(functionMapper));
        }

        public DatabaseTable SourceTable { get; }

        public DatabaseTable DestTable { get; }

        public FunctionMapper FunctionMapper { get; }

        public ITableFieldLink Direct(string destFieldName, string sourceFieldName)
        {
            var sourceField = SourceTable.FindColumn(sourceFieldName);
            if (sourceField == null)
                Throws.ThrowFieldNotFound(sourceFieldName, SourceTable.Name);
            var destField = DestTable.FindColumn(destFieldName);
            if (destField == null)
                Throws.ThrowFieldNotFound(destFieldName, DestTable.Name);
            return new DirectTableFieldLink(destField!, sourceField!);
        }
        public ITableFieldLink Expand(string destFieldName, IExpandResult expandResult)
        {
            var destField = DestTable.FindColumn(destFieldName);
            if (destField == null)
                Throws.ThrowFieldNotFound(destFieldName, DestTable.Name);
            return new ExpandTableFieldLink(destField!, expandResult);
        }
        public FluentTableFieldLinkBuilder Fluent()
        {
            return new FluentTableFieldLinkBuilder(this, FunctionMapper);
        }
        public static TableFieldLinkBuilder From(DatabaseReader reader, string sourceTableName, string destTableName)
        {
            var sourceTable = reader.Table(sourceTableName);
            if (sourceTable == null)
                Throws.ThrowTableNotFound(sourceTableName);
            var destTable = reader.Table(destTableName);
            if (destTable == null)
                Throws.ThrowTableNotFound(destTableName);
            return new TableFieldLinkBuilder(sourceTable!, destTable!, FunctionMapper.Get(reader.SqlType!.Value)!);
        }
    }
}
