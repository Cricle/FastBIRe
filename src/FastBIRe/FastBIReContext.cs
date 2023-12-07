using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public class FastBIReContext : IFastBIReContext
    {
        private bool isDisposed;

        public FastBIReContext(DbConnection connection, ITableProvider tableProvider)
            : this((IDbScriptExecuter)new DefaultScriptExecuter(connection), tableProvider)
        {

        }
        public FastBIReContext(IDbScriptExecuter executer, ITableProvider tableProvider)
        {
            this.executer = executer ?? throw new ArgumentNullException(nameof(executer));
            TableProvider = tableProvider ?? throw new ArgumentNullException(nameof(tableProvider));
            databaseReader = executer.CreateReader();
            SqlType = databaseReader.SqlType!.Value;
        }

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
                return databaseReader!;
            }
        }

        public SqlType SqlType { get; }

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
