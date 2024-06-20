using Diagnostics.Generator.Core;
using Diagnostics.Traces.Stores;
using FastBIRe;
using ValueBuffer;

namespace Diagnostics.Traces.DuckDB
{
    public class DuckDBCounterStoreProvider : ICounterStoreProvider, IOpetatorHandler<string>, IDisposable
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
            executeBuffer = new BufferOperator<string>(this, false, false);
        }
        private readonly List<string> initializeSqls;
        private readonly BufferOperator<string> executeBuffer;

        public IUndefinedDatabaseSelector<DuckDBDatabaseCreatedResult> DatabaseSelector { get; }

        public bool CreateDropSQL { get; }

        public Func<string, string> NameCreator { get; }

        public int UnComplateSqlCount => executeBuffer.UnComplatedCount;

        public event EventHandler<BufferOperatorExceptionEventArgs<string>>? ExceptionRaised
        {
            add { executeBuffer.ExceptionRaised += value; }
            remove { executeBuffer.ExceptionRaised -= value; }
        }

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
            using var s = new ValueStringBuilder();
            s.Append("INSERT INTO \"");
            s.Append(tableName);
            s.Append("\" VALUES ");
            var nowStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff");
            foreach (var item in values)
            {
                s.Append("('");
                s.Append(nowStr);
                s.Append('\'');
                foreach (var val in item)
                {
                    s.Append(',');
                    s.Append(DuckHelper.WrapValue(val));
                }

                s.Append(')');
            }
            var sql = s.ToString();
            executeBuffer.Add(sql);
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

        Task IOpetatorHandler<string>.HandleAsync(string input, CancellationToken token)
        {
            DatabaseSelector.UsingDatabaseResult(input, static (res, sql) =>
            {
                DuckDBNativeHelper.DuckDBQuery(res.NativeConnection, sql);
            });
            DatabaseSelector.ReportInserted(1);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            executeBuffer.Dispose();
        }
    }
}
