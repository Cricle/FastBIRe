using DatabaseSchemaReader.DataSchema;
using System.Data;

namespace FastBIRe.Project.WebSample
{
    internal class TableIniter : ITableIniter
    {
        public static readonly TableIniter Instance = new TableIniter();

        public string GetInitScript(MigrationService migrationService, string tableName)
        {
            var ddl = migrationService.DdlGeneratorFactory.MigrationGenerator();
            var table = new DatabaseTable
            {
                Name = tableName,
            };
            table.AddColumn("_id", DbType.Int64).AddPrimaryKey($"IX_{tableName}").AddIdentity();
            table.AddColumn("_time", DbType.DateTime).AddIndex($"IDX_time");
            return ddl.AddTable(table);
        }

        public IReadOnlyList<TableColumnDefine> GetInitTableColumnDefines(SourceTableColumnBuilder builder)
        {
            return new[]
            {
                builder.Column("_id", builder.Type( DbType.Int64)),
                builder.Column("_time", builder.Type( DbType.DateTime)),
            };
        }
    }
}