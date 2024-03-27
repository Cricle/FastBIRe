using System.Data.Common;

namespace FastBIRe.Creating
{
    public static class DatabaseCreateAdapterExtensions
    {
        record class DatabaseCreator : IDatabaseCreator
        {
            public DatabaseCreator(IScriptExecuter scriptExecuter, string existsSql, string createSql, Action<DbCommand>? commandAction)
            {
                ScriptExecuter = scriptExecuter;
                ExistsSql = existsSql;
                CreateSql = createSql;
                CommandAction = commandAction;
            }

            public IScriptExecuter ScriptExecuter { get; }

            public Action<DbCommand>? CommandAction { get; }

            public string ExistsSql { get; }

            public string CreateSql { get; }

            public Task<int> CreateAsync(CancellationToken token = default)
            {
                return ScriptExecuter.ExecuteAsync(ExistsSql, token: token);
            }

            public async Task<bool> ExistsAsync(CancellationToken token = default)
            {
                var ok = false;
                await ScriptExecuter.ReadAsync(ExistsSql, (o, e) =>
                {
                    ok = e.Reader.Read();
                    return Task.CompletedTask;
                }, token: token);
                return ok;
            }
        }

        public static IDatabaseCreator GetDatabaseCreator(this IDatabaseCreateAdapter adapter, string database, IScriptExecuter scriptExecuter, Action<DbCommand>? commandAction = null)
        {
            var existsSql = adapter.CheckDatabaseExists(database);
            var createSql = adapter.CreateDatabase(database);
            return new DatabaseCreator(scriptExecuter, existsSql, createSql, commandAction);
        }
    }
    public interface IDatabaseCreator
    {
        string ExistsSql { get; }

        string CreateSql { get; }

        Task<bool> ExistsAsync(CancellationToken token = default);

        Task<int> CreateAsync(CancellationToken token = default);
    }
    public interface IDatabaseCreateAdapter
    {
        string CreateDatabase(string database);

        string CreateDatabaseIfNotExists(string database);

        string DropDatabase(string database);

        string DropDatabaseIfExists(string database);

        string DropTable(string table);

        string DropTableIfExists(string table);

        string CheckDatabaseExists(string database);
    }

}
