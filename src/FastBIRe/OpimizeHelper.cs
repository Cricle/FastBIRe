using DatabaseSchemaReader.DataSchema;
using System.Runtime.CompilerServices;

namespace FastBIRe
{
    public static class OpimizeHelper
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
                    return Sqlite();
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
            return $"OPTIMIZE TABLE `{table}`;";
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SqlServer(string table)
        {
            return $"ALTER INDEX ALL ON [{table}] REBUILD ;";
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Sqlite()
        {
            return $"VACUUM;";
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string PostgreSql(string table)
        {
            return $"VACUUM FULL \"{table}\";";
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Oracle(string table)
        {
            return $"ALTER TABLE TRUNCATE TABLE \"{table}\" MOVE;";
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string DB2(string table)
        {
            return $"REORG TABLE \"{table}\";";
        }
    }
}
