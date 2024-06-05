using DuckDB.NET.Native;
using System.Data.Common;

namespace Diagnostics.Traces.DuckDB.Exceptions
{
    public class DuckTraceDBException : DbException
    {
        internal DuckTraceDBException()
        {
        }

        internal DuckTraceDBException(string message) : base(message)
        {
        }

        internal DuckTraceDBException(string message, DuckDBState state) : base(message, (int)state)
        {

        }
    }
}
