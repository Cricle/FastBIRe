using DatabaseSchemaReader.DataSchema;
using FastBIRe;
using System.Data;

namespace rsa
{
    public class TableService: TableServiceBase
    {
        public TableService(MigrationService migration) : base(migration)
        {
        }

        public override async Task<int> CreateTableIfNotExistsAsync(string tableName, CancellationToken token = default)
        {
            if (Migration.Reader.TableExists(tableName))
            {
                return 0;
            }
            var migGen = Migration.DdlGeneratorFactory.MigrationGenerator();
            var tb = new DatabaseTable { Name = tableName };
            tb.AddColumn("_id", DbType.Int64, x => x.AddIdentity().AddPrimaryKey($"PK_{tableName}_id"));
            tb.AddColumn("记录时间", DbType.DateTime, x =>
            {
                x.Nullable = false;
                x.AddIndex($"IDX_{tableName}_记录时间");
            });
            var script = migGen.AddTable(tb);
            return await Migration.ExecuteNonQueryAsync(script, token);
        }
    }
}