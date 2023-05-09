using DatabaseSchemaReader.DataSchema;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public static class TruncateHelper
    {
        public static string Sql(string table, SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return SqlServer(table);
                case SqlType.MySql:
                    return MySql(table);
                case SqlType.SQLite:
                    return Sqlite(table);
                case SqlType.PostgreSql:
                    return PostgreSql(table);
                case SqlType.Oracle:
                    return Oracle(table);
                case SqlType.Db2:
                    return DB2(table);
                default:
                    throw new NotSupportedException(sqlType.ToString());
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MySql(string table)
        {
            return @$"DELETE FROM `{table}`;";
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SqlServer(string table)
        {
            return $"TRUNCATE TABLE [{table}];";
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Sqlite(string table)
        {
            return $"DELETE FROM \"{table}\";";
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string PostgreSql(string table)
        {
            return $"TRUNCATE TABLE \"{table}\";";
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Oracle(string table)
        {
            return $"TRUNCATE TABLE \"{table}\";";
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string DB2(string table)
        {
            return $"TRUNCATE TABLE \"{table}\";";
        }
    }
}
