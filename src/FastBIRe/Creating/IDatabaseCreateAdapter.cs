using System.Data;
using System.Data.Common;

namespace FastBIRe.Creating
{
    public static class DatabaseCreateAdapterExtensions
    {
        record class DatabaseCreator : IDatabaseCreator
        {
            public DatabaseCreator(DbConnection connection, string existsSql, string createSql, Action<DbCommand>? commandAction)
            {
                Connection = connection;
                ExistsSql = existsSql;
                CreateSql = createSql;
                CommandAction = commandAction;
            }

            public DbConnection Connection { get; }

            public Action<DbCommand>? CommandAction { get; }

            public string ExistsSql { get; }

            public string CreateSql { get; }

            private async Task EnsureOpenAsync(CancellationToken token = default)
            {
                if (Connection.State!= ConnectionState.Open)
                {
                    await Connection.OpenAsync(token);
                }
            }

            public async Task<int> CreateAsync(CancellationToken token = default)
            {
                await EnsureOpenAsync(token);
                using (var command = Connection.CreateCommand())
                {
                    command.CommandText = ExistsSql;
                    CommandAction?.Invoke(command);
                    return await command.ExecuteNonQueryAsync(token);
                }
            }

            public async Task<bool> ExistsAsync(CancellationToken token = default)
            {
                await EnsureOpenAsync(token);
                using (var command = Connection.CreateCommand())
                {
                    command.CommandText = ExistsSql;
                    CommandAction?.Invoke(command);
                    using (var reader = await command.ExecuteReaderAsync(token))
                    {
                        return reader.Read();
                    }
                }
            }
        }

        public static IDatabaseCreator GetDatabaseCreator(this IDatabaseCreateAdapter adapter,string database,DbConnection dbConnection,Action<DbCommand>? commandAction=null)
        {
            var existsSql = adapter.CheckDatabaseExists(database);
            var createSql = adapter.CreateDatabase(database);
            return new DatabaseCreator(dbConnection, existsSql, createSql,commandAction);
        }
    }
    public interface IDatabaseCreator
    {
        string ExistsSql { get; }

        string CreateSql { get; }

        Task<bool> ExistsAsync(CancellationToken token = default);

        Task<int> CreateAsync(CancellationToken token=default);
    }
    public interface IDatabaseCreateAdapter
    {
        string CreateDatabase(string database);

        string CreateDatabaseIfNotExists(string database);

        string DropDatabase(string database);

        string DropDatabaseIfExists(string database);

        string DropTable(string database);

        string DropTableIfExists(string database);

        string CheckDatabaseExists(string database);
    }

}
