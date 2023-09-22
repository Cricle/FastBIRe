using DatabaseSchemaReader.DataSchema;

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
                    return $"ALTER INDEX ALL ON [{table}] REBUILD;";
                case SqlType.MySql:
                    return $"OPTIMIZE TABLE `{table}`;";
                case SqlType.SQLite:
                    return "VACUUM;";
                case SqlType.PostgreSql:
                    return $"VACUUM FULL \"{table}\";";
                case SqlType.Oracle:
                    return $"ALTER TABLE TRUNCATE TABLE \"{table}\" MOVE;";
                case SqlType.Db2:
                    return $"REORG TABLE \"{table}\";";
                default:
                    throw new NotSupportedException(sqlType.ToString());
            }
        }
    }
}
