using DatabaseSchemaReader.DataSchema;
using FastBIRe.Creating;
using FastBIRe.Wrapping;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public static class SqlTypGetExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Ors(this SqlType sqlType, params SqlType[] types)
        {
            return types.Any(x => x == sqlType);
        }
        public static TableHelper? GetTableHelper(this SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    return TableHelper.SqlServer;
                case SqlType.Oracle:
                    return TableHelper.Oracle;
                case SqlType.MySql:
                    return TableHelper.MySql;
                case SqlType.SQLite:
                    return TableHelper.Sqlite;
                case SqlType.PostgreSql:
                    return TableHelper.PostgreSql;
                case SqlType.DuckDB:
                    return TableHelper.DuckDB;
                case SqlType.Db2:
                default:
                    return null;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Wrap(this SqlType sqlType, string? field)
        {
            return GetEscaper(sqlType).Quto(field);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? WrapValue<T>(this SqlType sqlType, T value)
        {
            return GetEscaper(sqlType).WrapValue(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDatabaseCreateAdapter? GetDatabaseCreateAdapter(this SqlType sqlType)
        {
            return DatabaseCreateAdapter.Get(sqlType);
        }
        public static IEscaper GetEscaper(this SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return DefaultEscaper.SqlServer;
                case SqlType.Oracle:
                    return DefaultEscaper.Oracle;
                case SqlType.MySql:
                    return DefaultEscaper.MySql;
                case SqlType.SQLite:
                    return DefaultEscaper.Sqlite;
                case SqlType.PostgreSql:
                    return DefaultEscaper.PostgreSql;
                case SqlType.DuckDB:
                    return DefaultEscaper.DuckDB;
                case SqlType.Db2:
                default:
                    throw new NotSupportedException(sqlType.ToString());
            }
        }
    }
}
