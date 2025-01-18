using FastBIRe.Internals;
using FastBIRe.Wrapping;
using System.Data.Common;

namespace FastBIRe
{
    public partial class DefaultScriptExecuter : IDbScriptExecuter, IDbStackTraceScriptExecuter
    {
        static DefaultScriptExecuter()
        {
            _ = ScriptExecuterEventSource.Instance;//Active event source
        }
        private static FSqlType? Convert(string providerName)
        {
            if (string.IsNullOrEmpty(providerName)) return null;

            if (providerName.Equals("System.Data.SqlClient", StringComparison.OrdinalIgnoreCase))
                return FSqlType.SqlServer;
            if (providerName.Equals("Microsoft.Data.SqlClient", StringComparison.OrdinalIgnoreCase))
                return FSqlType.SqlServer;
            if (providerName.IndexOf("SQLite", StringComparison.OrdinalIgnoreCase) != -1)
            {
                return FSqlType.SQLite;
            }
            if (providerName.IndexOf("Oracle", StringComparison.OrdinalIgnoreCase) != -1)
            {
                return FSqlType.Oracle;
            }
            if (providerName.IndexOf("MySql", StringComparison.OrdinalIgnoreCase) != -1)
            {
                return FSqlType.MySql;
            }
            if (providerName.Equals("System.Data.SqlServerCe.4.0", StringComparison.OrdinalIgnoreCase))
                return FSqlType.SqlServerCe;
            if (providerName.Equals("Npgsql", StringComparison.OrdinalIgnoreCase) ||
                providerName.Equals("Devart.Data.PostgreSql", StringComparison.OrdinalIgnoreCase))
                return FSqlType.PostgreSql;
            if (providerName.Equals("IBM.Data.DB2", StringComparison.OrdinalIgnoreCase))
                return FSqlType.Db2;
            if (providerName.Equals("DuckDB.NET.Data", StringComparison.OrdinalIgnoreCase))
                return FSqlType.DuckDB;

            return null;
        }
        public DefaultScriptExecuter(DbConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            SqlType = Convert(Connection.GetType().Name) ?? throw new NotSupportedException(connection.GetType().FullName);
            Escaper = SqlType.GetEscaper();
            ScriptStated += OnScriptStated;
        }

        private void OnScriptStated(object? sender, ScriptExecuteEventArgs e)
        {
            ScriptExecuterEventSource.Instance.WriteScriptExecuteEventArgs(e);
        }

        public DbConnection Connection { get; }

        public FSqlType SqlType { get; }

        public IEscaper Escaper { get; }

        public int CommandTimeout { get; set; } = DefaultCommandTimeout;

        public bool UseBatch { get; set; } = true;

        public bool CaptureStackTrace { get; set; }

        public bool StackTraceNeedFileInfo { get; set; } = true;

        public bool EnableSqlParameterConversion { get; set; } = true;

        public bool EnableSqlQutoConversion { get; set; }

        public char SqlParameterPrefix { get; set; } = '@';

        public char QutoStart { get; set; } = '[';

        public char QutoEnd { get; set; } = ']';

        public event EventHandler<ScriptExecuteEventArgs>? ScriptStated;
        public event EventHandler? Disposed;

        public void SafeRegistStated(EventHandler<ScriptExecuteEventArgs> handler)
        {
            ScriptStated += handler;
            void DisposeEvent(object? _, EventArgs __)
            {
                ScriptStated -= handler;
                Disposed -= DisposeEvent;
            }

            Disposed += DisposeEvent;
        }

        private IEnumerable<ScriptUnit> CreateScriptUnits(IEnumerable<string> scripts, IEnumerable<IEnumerable<KeyValuePair<string, object?>>>? argss)
        {
            if (argss == null)
            {
                return scripts.Select(x => new ScriptUnit(x)).ToList();
            }
            var units = new List<ScriptUnit>();
            using (var scriptsEnu = scripts.GetEnumerator())
            using (var argssEnu = argss.GetEnumerator())
            {
                while (scriptsEnu.MoveNext())
                {
                    var argssOk = argssEnu.MoveNext();
                    if (argssOk)
                    {
                        units.Add(new ScriptUnit(scriptsEnu.Current, argssEnu.Current));
                    }
                    else
                    {
                        units.Add(new ScriptUnit(scriptsEnu.Current));
                    }
                }
            }
            return units;
        }
        public void Dispose()
        {
            Connection?.Dispose();
            DetchEventSource();
            Disposed?.Invoke(this, EventArgs.Empty);
        }
        public void DetchEventSource()
        {
            ScriptStated -= OnScriptStated;
        }
        public override string ToString()
        {
            return $"{{Connection: {Connection}}}";
        }
    }
}
