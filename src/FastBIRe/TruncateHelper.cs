using DatabaseSchemaReader.DataSchema;

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
                    return $"TRUNCATE TABLE [{table}];";
                case SqlType.MySql:
                    return $"DELETE FROM `{table}`;";
                case SqlType.SQLite:
                    return $"DELETE FROM `{table}`;";
                case SqlType.PostgreSql:
                    return $"TRUNCATE TABLE \"{table}\";";
                case SqlType.Oracle:
                    return $"TRUNCATE TABLE \"{table}\";";
                case SqlType.Db2:
                    return $"TRUNCATE TABLE \"{table}\";";
                default:
                    throw new NotSupportedException(sqlType.ToString());
            }
        }
    }
}
