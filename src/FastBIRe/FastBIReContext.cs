using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using FastBIRe.Wrapping;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public class FastBIReContext : IFastBIReContext
    {
        public static FastBIReContext FromDbConnection(DbConnection dbConnection,ITableProvider tableProvider)
        {
            return new FastBIReContext(
                new DefaultScriptExecuter(dbConnection),
                tableProvider);
        }

        public FastBIReContext(IDbScriptExecuter executer, ITableProvider tableProvider)
        {
            this.executer = executer ?? throw new ArgumentNullException(nameof(executer));
            TableProvider = tableProvider ?? throw new ArgumentNullException(nameof(tableProvider));
            SqlType = executer.SqlType;
            Escaper = SqlType.GetEscaper();
        }

        private bool isDisposed;
        private IDbScriptExecuter? executer;
        private DatabaseReader? databaseReader;
        public IDbScriptExecuter Executer
        {
            get
            {
                ThrowIfDisposed();
                return executer!;
            }
        }

        public ITableProvider TableProvider { get; }

        public bool IsDisposed => isDisposed;

        public DatabaseReader DatabaseReader
        {
            get
            {
                ThrowIfDisposed();
                if (databaseReader == null)
                {
                    databaseReader = executer!.CreateReader();
                }
                else
                {
                    databaseReader!.Owner = Executer.Connection.Database;
                }
                return databaseReader!;
            }
        }

        public SqlType SqlType { get; }

        public DbConnection Connection => Executer.Connection;

        public IEscaper Escaper { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                executer?.Dispose();
                executer = null;
                databaseReader = null;
                isDisposed = true;
            }
        }
    }
}
