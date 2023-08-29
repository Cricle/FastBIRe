using FastBIRe.Project.Models;

namespace FastBIRe.Project
{
    public interface ITableFactory<TResult,TProject,TId> :IDisposable
        where TProject:IProject<TId>
        where TResult : ProjectCreateContextResult<TProject,TId>
    {
        TResult ProjectResult { get; }

        MigrationService Service { get; }

        ITableIniter TableIniter { get; }

        bool TableExists(string tableName);

        bool TryGetCreateScript(string tableName, out string? script);

        Task<MigrateToSqlRestul> MigrateToSqlAsync(string tableName, IReadOnlyList<TableColumnDefine> news, IEnumerable<TableColumnDefine>? olds, CancellationToken token = default);
    }

}
