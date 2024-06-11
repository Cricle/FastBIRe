using Diagnostics.Traces.DuckDB.Exceptions;
using DuckDB.NET.Data;
using DuckDB.NET.Native;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Diagnostics.Traces.DuckDB.Status
{
    internal static class DuckDBNativeHelper
    {
        private static readonly Func<DuckDBConnection, DuckDBNativeConnection> connectionGetter;

        static DuckDBNativeHelper()
        {
            var par = Expression.Parameter(typeof(DuckDBConnection));
            var connRef = typeof(DuckDBConnection).GetField("connectionReference", BindingFlags.NonPublic | BindingFlags.Instance);
            var connProp = connRef!.FieldType.GetProperty("NativeConnection", BindingFlags.Public | BindingFlags.Instance);
            var getRef = Expression.Field(par, connRef);
            var body = Expression.Call(Expression.Convert(getRef, connRef.FieldType), connProp!.GetMethod!);
            connectionGetter = Expression.Lambda<Func<DuckDBConnection, DuckDBNativeConnection>>(body, par).Compile();
        }

        public static DuckDBNativeConnection GetNativeConnection(DuckDBConnection conn)
        {
            if (conn.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("The connection must be opened");
            }
            return connectionGetter(conn);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DuckDBQuery(DuckDBNativeConnection connection, string input)
        {
            var state = NativeMethods.Query.DuckDBQuery(connection, input, out var res);
            try
            {
                if (state == DuckDBState.Error)
                {
                    var str = NativeMethods.Query.DuckDBResultError(ref res).ToManagedString(false);
                    throw new DuckTraceDBException(str, state);
                }
            }
            finally
            {
                NativeMethods.Query.DuckDBDestroyResult(ref res);
            }
        }

    }
}
