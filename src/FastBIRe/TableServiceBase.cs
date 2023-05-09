namespace FastBIRe
{
    public abstract class TableServiceBase
    {
        public TableServiceBase(MigrationService migration)
        {
            Migration = migration;
        }

        public MigrationService Migration { get; }

        public Task<int> SyncIndexAsync(string destTable, SourceTableDefine tableDefine, CancellationToken token = default)
        {
            return Migration.SyncIndexAutoAsync(destTable, tableDefine, token: token);
        }
        public Task<int> MigrationAsync(string tableName, IReadOnlyList<TableColumnDefine> news, CancellationToken token = default)
        {
            var builder = Migration.GetColumnBuilder();
            return MigrationAsync(tableName, news, builder.CloneWith(news, x =>
            {
                x.Id = x.Field;
                return x;
            }), token);
        }
        public Task<int> MigrationAsync(string destTable, SourceTableDefine tableDefine, bool syncSource, CancellationToken token = default)
        {
            var builder = Migration.GetColumnBuilder();
            return MigrationAsync(destTable, tableDefine, builder.CloneWith(tableDefine.Columns, x =>
            {
                x.Id = x.Field;
                return x;
            }), syncSource, token);
        }
        public Task<int> MigrationAsync(string destTable, SourceTableDefine tableDefine, IEnumerable<TableColumnDefine> olds, bool syncSource, CancellationToken token = default)
        {
            var commands = Migration.RunMigration(destTable, tableDefine, olds,syncSource);
            return Migration.ExecuteNonQueryAsync(commands, token);
        }
        public Task<int> MigrationAsync(string tableName, IReadOnlyList<TableColumnDefine> news, IEnumerable<TableColumnDefine> olds, CancellationToken token = default)
        {
            var commands = Migration.RunMigration(tableName, news, olds);
            return Migration.ExecuteNonQueryAsync(commands, token);
        }

        public abstract Task<int> CreateTableIfNotExistsAsync(string tableName, CancellationToken token = default);
    }

}
