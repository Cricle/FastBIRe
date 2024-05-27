using Diagnostics.Generator.Core;
using Diagnostics.Traces.DuckDB.Status;
using DuckDB.NET.Data;
using DuckDB.NET.Native;
using FastBIRe;
using ValueBuffer;

namespace Diagnostics.Traces.DuckDB
{
    public class DuckDBCounterStoreProvider : ICounterStoreProvider, IOpetatorHandler<string>, IDisposable
    {
        public DuckDBCounterStoreProvider(DuckDBConnection connection, bool createDropSQL = true, Func<string, string>? nameCreator = null)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            CreateDropSQL = createDropSQL;
            NameCreator = nameCreator ?? DefaultNameCreator;
            nativeConnection= DuckDBNativeHelper.GetNativeConnection(connection);

            executeBuffer = new BufferOperator<string>(this, false, false);
        }

        private readonly DuckDBNativeConnection nativeConnection;
        private readonly BufferOperator<string> executeBuffer;

        public DuckDBConnection Connection { get; }

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
            Connection.ExecuteNoQuery(sql);
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

            var isFirst = true;
            foreach (var item in values)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    s.Append(',');
                }
                var isInnerFirst = true;
                s.Append('(');
                foreach (var val in item)
                {
                    if (isInnerFirst)
                    {
                        isInnerFirst = false;
                    }
                    else
                    {
                        s.Append(',');
                    }
                    s.Append(DuckHelper.WrapValue(item));
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
            s.Append($"CREATE TABLE IF NOT EXISTS \"{tableName}\"(");
            var isFirst = true;
            foreach (var item in columns)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    s.Append(',');
                }
                s.Append($"\"{item.Name}\" DOUBLE\n");
            }
            s.Append(");");
            return s.ToString();
        }

        Task IOpetatorHandler<string>.HandleAsync(string input, CancellationToken token)
        {
            DuckDBNativeHelper.DuckDBQuery(nativeConnection, input);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            executeBuffer.Dispose();
        }
    }
}
