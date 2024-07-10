using Diagnostics.Generator.Core;
using Diagnostics.Traces.Stores;
using FastBIRe;
using ValueBuffer;

namespace Diagnostics.Traces.DuckDB
{
    public class DuckDBCounterStoreProvider : ICounterStoreProvider
    {
        public DuckDBCounterStoreProvider(IUndefinedDatabaseSelector<DuckDBDatabaseCreatedResult> databaseSelector, bool createDropSQL = false, Func<string, string>? nameCreator = null)
        {
            DatabaseSelector = databaseSelector ?? throw new ArgumentNullException(nameof(databaseSelector));
            CreateDropSQL = createDropSQL;
            NameCreator = nameCreator ?? DefaultNameCreator;
            initializeSqls = new List<string>();
            DatabaseSelector.Initializers.Add(new DelegateResultInitializer<DuckDBDatabaseCreatedResult>(r =>
            {
                lock (initializeSqls)
                {
                    foreach (var item in initializeSqls)
                    {
                        DuckDBNativeHelper.DuckDBQuery(r.NativeConnection, item);
                    }
                }
            }));
        }
        private readonly List<string> initializeSqls;

        public IUndefinedDatabaseSelector<DuckDBDatabaseCreatedResult> DatabaseSelector { get; }

        public bool CreateDropSQL { get; }

        public Func<string, string> NameCreator { get; }

        private static string DefaultNameCreator(string name)
        {
            return $"{name}_counter";
        }

        public Task InitializeAsync(string name, IEnumerable<CounterStoreColumn> columns)
        {
            var sql = GetCreateTableSql(name, CreateDropSQL, columns);

            lock (initializeSqls)
            {
                initializeSqls.Add(sql);
            }
            DatabaseSelector.UsingDatabaseResult(sql,static (res,ql) =>
            {
                DuckDBNativeHelper.DuckDBQuery(res.NativeConnection, ql);
            });
            return Task.CompletedTask;
        }

        public Task InsertAsync(string name, IEnumerable<double?> values)
        {
            return InsertManyAsync(name, new OneEnumerable<IEnumerable<double?>>(values));
        }

        public Task InsertManyAsync(string name, IEnumerable<IEnumerable<double?>> values)
        {
            var tableName = NameCreator(name);
            DatabaseSelector.UnsafeUsingDatabaseResult(values, (res, values) =>
            {
                var now = DateTime.Now;
                using var appender = res.Connection.CreateAppender(tableName);
                foreach (var item in values)
                {
                    var row = appender.CreateRow();
                    row.AppendValue(now);
                    foreach (var val in item)
                    {
                        row.AppendValue(val);
                    }
                    row.EndRow();
                }
            });
            return Task.CompletedTask;
        }
        private string GetCreateTableSql(string name, bool createDropSQL, IEnumerable<CounterStoreColumn> columns)
        {
            using var s = new ValueStringBuilder();
            var tableName = NameCreator(name);
            var dropSql = string.Empty;
            if (createDropSQL)
            {
                s.Append($"DROP TABLE IF EXISTS \"{tableName}\";");
            }
            s.Append($"CREATE TABLE IF NOT EXISTS \"{tableName}\"(ts DATETIME");
            foreach (var item in columns)
            {
                s.Append($",\"{item.Name}\" DOUBLE\n");
            }
            s.Append(");");
            return s.ToString();
        }
    }
}
