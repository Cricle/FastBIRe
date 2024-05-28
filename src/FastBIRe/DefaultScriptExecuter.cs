using DatabaseSchemaReader.DataSchema;
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
        public DefaultScriptExecuter(DbConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            var sqlType = Connection.GetSqlType();
            if (sqlType == null)
            {
                throw new NotSupportedException(connection.GetType().FullName);
            }
            SqlType = sqlType!.Value;
            Escaper = SqlType.GetEscaper();
            ScriptStated += OnScriptStated;
        }

        private void OnScriptStated(object? sender, ScriptExecuteEventArgs e)
        {
            ScriptExecuterEventSource.Instance.WriteScriptExecuteEventArgs(e);
        }

        public DbConnection Connection { get; }

        public SqlType SqlType { get; }

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
