using FastBIRe.Creating;
using FastBIRe.Wrapping;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public static class SqlTypGetExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Ors(this FSqlType sqlType, params FSqlType[] types)
        {
            return types.Any(x => x == sqlType);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Wrap(this FSqlType sqlType, string? field)
        {
            return GetEscaper(sqlType).Quto(field);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? WrapValue<T>(this FSqlType sqlType, T value)
        {
            return GetEscaper(sqlType).WrapValue(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDatabaseCreateAdapter? GetDatabaseCreateAdapter(this FSqlType sqlType)
        {
            return DatabaseCreateAdapter.Get(sqlType);
        }
        public static IEscaper GetEscaper(this FSqlType sqlType)
        {
            switch (sqlType)
            {
                case FSqlType.SqlServerCe:
                case FSqlType.SqlServer:
                    return DefaultEscaper.SqlServer;
                case FSqlType.Oracle:
                    return DefaultEscaper.Oracle;
                case FSqlType.MySql:
                    return DefaultEscaper.MySql;
                case FSqlType.SQLite:
                    return DefaultEscaper.Sqlite;
                case FSqlType.PostgreSql:
                    return DefaultEscaper.PostgreSql;
                case FSqlType.DuckDB:
                    return DefaultEscaper.DuckDB;
                case FSqlType.Db2:
                default:
                    throw new NotSupportedException(sqlType.ToString());
            }
        }
    }
}
