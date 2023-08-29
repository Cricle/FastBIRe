namespace FastBIRe.Project
{
    public interface ITableFactory<TResult,TId> :IDisposable
       where TResult : ProjectCreateContextResult<TId>
    {
        TResult ProjectResult { get; }

        MigrationService Service { get; }

        ITableIniter TableIniter { get; }

        bool TableExists(string tableName);

        bool TryGetCreateScript(string tableName, out string? script);

        Task<MigrateToSqlRestul> MigrateToSqlAsync(string tableName, IReadOnlyList<TableColumnDefine> news, IEnumerable<TableColumnDefine>? olds, CancellationToken token = default);
    }

}
