using Diagnostics.Traces.DuckDB.Exceptions;
using DuckDB.NET.Data;
using DuckDB.NET.Native;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using ValueBuffer;

namespace Diagnostics.Traces.DuckDB
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DuckDBNativeConnection GetNativeConnection(DuckDBConnection conn)
        {
            if (conn.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("The connection must be opened");
            }
            return connectionGetter(conn);
        }

        public static unsafe long DuckDBQuery(DuckDBNativeConnection connection, string input)
        {
            var state = NativeMethods.Query.DuckDBQuery(connection, input, out var res);
            try
            {
                if (state == DuckDBState.Error)
                {
                    ThrowDuckExcpetion(ref res);
                }
                return NativeMethods.Query.DuckDBRowsChanged(ref res);
            }
            finally
            {
                NativeMethods.Query.DuckDBDestroyResult(ref res);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ThrowDuckExcpetion(ref DuckDBResult result)
        {
            var str = NativeMethods.Query.DuckDBResultError(ref result).ToManagedString();
            throw new DuckTraceDBException(str, DuckDBState.Error);

        }
        public static unsafe long DuckDBQuery(DuckDBNativeConnection connection, in ValueStringBuilder builder)
        {
            var lst = builder._chars;
            var count = Encoding.UTF8.GetMaxByteCount(lst.Size);
            var intPtr = Marshal.AllocCoTaskMem(count + 1);
            using var handler = new SafeUnmanagedMemoryHandle(intPtr);
            var offset = 0;
            for (int i = 0; i < lst.BufferSlotIndex; i++)
            {
                var array = lst.DangerousGetArray(i);
                var length = i == lst.BufferSlotIndex - 1 ? lst.LocalUsed : array.Length;
                offset += Encoding.UTF8.GetBytes((char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(array.AsSpan())), length, (byte*)intPtr + offset, count - offset);
            }
            *((byte*)intPtr + offset) = 0;
            var state = NativeMethods.Query.DuckDBQuery(connection, handler, out var res);
            try
            {
                if (state == DuckDBState.Error)
                {
                    ThrowDuckExcpetion(ref res);
                }
                return NativeMethods.Query.DuckDBRowsChanged(ref res);
            }
            finally
            {
                NativeMethods.Query.DuckDBDestroyResult(ref res);
            }
        }
    }
}
