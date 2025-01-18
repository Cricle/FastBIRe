namespace FastBIRe
{
    public static class SqlTypTableHelperGetExtensions
    {
        public static TableHelper? GetTableHelper(this FSqlType sqlType)
        {
            switch (sqlType)
            {
                case FSqlType.SqlServer:
                case FSqlType.SqlServerCe:
                    return TableHelper.SqlServer;
                case FSqlType.Oracle:
                    return TableHelper.Oracle;
                case FSqlType.MySql:
                    return TableHelper.MySql;
                case FSqlType.SQLite:
                    return TableHelper.Sqlite;
                case FSqlType.PostgreSql:
                    return TableHelper.PostgreSql;
                case FSqlType.DuckDB:
                    return TableHelper.DuckDB;
                case FSqlType.Db2:
                default:
                    return null;
            }
        }
    }
}
