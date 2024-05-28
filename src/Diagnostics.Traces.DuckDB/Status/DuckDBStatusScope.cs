using Diagnostics.Traces.Status;
using DuckDB.NET.Data;

namespace Diagnostics.Traces.DuckDB.Status
{
    public class DuckDBStatusScope : StatusScopeBase
    {
        internal DuckDBStatusScope(DuckDBConnection connection, string name, string tableName, DuckDBPrepare prepare)
        {
            Connection = connection;
            Key = name;
            Name = tableName;
            this.prepare = prepare;
        }
        internal readonly DuckDBPrepare prepare;

        public override string Name { get; }

        public DuckDBConnection Connection { get; }

        public override string Key { get; }

        public override void Dispose()
        {
            if (!IsComplated)
            {
                OnComplate(StatusTypes.Unset);
            }
        }

        public override bool Log(string message)
        {
            ThrowIfComplated();            ;
            return prepare.Log(Key, DateTime.Now, message) > 0;
        }

        public override bool Set(string status)
        {
            ThrowIfComplated();
            return prepare.Set(Key, DateTime.Now, status) > 0;
        }

        protected override void OnComplate(StatusTypes types = StatusTypes.Unset)
        {
            prepare.Complate(Key, types);
        }
    }
}
