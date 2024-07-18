using Diagnostics.Traces.Stores;
using FastBIRe;

namespace Diagnostics.Traces.DuckDB
{
    public class DuckDBStringStore : StringStoreBase, IDisposable
    {
        public DuckDBStringStore(IUndefinedDatabaseSelector<DuckDBDatabaseCreatedResult> databaseSelector, string name)
        {
            DatabaseSelector = databaseSelector;
            Name = name;
            initer = new DelegateResultInitializer<DuckDBDatabaseCreatedResult>(r =>
            {
                r.Connection.ExecuteNoQuery(CreateCreateTableSql());
            });
            databaseSelector.UsingDatabaseResult(initer, static (r, initer) =>
            {
                initer.InitializeResult(r);
            });
            databaseSelector.Initializers.Add(initer);
        }

        private readonly DelegateResultInitializer<DuckDBDatabaseCreatedResult> initer;

        public IUndefinedDatabaseSelector<DuckDBDatabaseCreatedResult> DatabaseSelector { get; }

        public override string Name { get; }

        private string CreateCreateTableSql()
        {
            return string.Intern($"CREATE TABLE IF NOT EXISTS \"{Name}\" (ts DATETIME,v BLOB)");
        }

        public override int Count()
        {
            return DatabaseSelector.UsingDatabaseResult(Name, static (res, tableName) =>
            {
                var sql = $"SELECT COUNT(*) FROM \"{tableName}\"";
                using (var command = res.Connection.CreateCommand())
                {
                    command.CommandText = sql;
                    return (int)command.ExecuteScalar()!;
                }
            });
        }

        public override void Dispose()
        {
            DatabaseSelector.Initializers.Remove(initer);
        }

        public override void Insert(BytesStoreValue value)
        {
            InsertMany(new OneEnumerable<BytesStoreValue>(value));
        }

        public override void InsertMany(IEnumerable<BytesStoreValue> strings)
        {
            var count = DatabaseSelector.UsingDatabaseResult(strings, (r, s) =>
            {
                var count = 0;
                using (var appender = r.Connection.CreateAppender(Name))
                {
                    foreach (var item in s)
                    {
                        var row = appender.CreateRow();
                        row.AppendValue(item.Time);
                        row.AppendValue(item.Value);
                        row.EndRow();
                        count++;
                    }
                }
                return count;
            });
            DatabaseSelector.ReportInserted(count);
        }
    }
}
