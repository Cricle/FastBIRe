using DuckDB.NET.Native;
using System.Data.Common;

namespace Diagnostics.Traces.DuckDB
{
    public class TraceDuckDbException : DbException
    {
        public TraceDuckDbException()
        {
        }

        public TraceDuckDbException(string? message) : base(message)
        {
        }

        public TraceDuckDbException(string? message, DuckDBState errorCode) : base(message, (int)errorCode)
        {
        }
    }
}
