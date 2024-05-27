using Diagnostics.Traces.Status;
using DuckDB.NET.Data;

namespace Diagnostics.Traces.DuckDB.Status
{
    public class DuckDBStatusScope : StatusScopeBase
    {
        internal DuckDBStatusScope(DuckDBConnection connection, string name, string tableName, DuckDBPrepare prepare)
        {
            Connection = connection;
            Name = name;
            TableName = tableName;
            this.prepare = prepare;
        }
        internal readonly DuckDBPrepare prepare;

        public DuckDBConnection Connection { get; }

        public string TableName { get; }

        public override string Name { get; }

        public override void Dispose()
        {
            if (!IsComplated)
            {
                OnComplate(StatuTypes.Unset);
            }
        }

        public override bool Log(string message)
        {
            ThrowIfComplated();            ;
            return prepare.Log(Name, DateTime.Now, message) > 0;
        }

        public override bool Set(string status)
        {
            ThrowIfComplated();
            return prepare.Set(Name, DateTime.Now, status) > 0;
        }

        protected override void OnComplate(StatuTypes types = StatuTypes.Unset)
        {
            prepare.Complate(Name, types);
        }
    }
}
