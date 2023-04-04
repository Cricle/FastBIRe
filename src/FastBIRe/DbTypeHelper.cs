using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public static class DbTypeHelper
{
    public static SqlType CastSqlType(SqlType types)
    {
        switch (types)
        {
            case SqlType.MySql:
                return SqlType.MySql;
            case SqlType.SQLite:
                return SqlType.SQLite;
            case SqlType.SqlServer:
                return SqlType.SqlServer;
            case SqlType.PostgreSql:
                return SqlType.PostgreSql;
            default:
                throw new NotSupportedException(types.ToString());
        }
    }
}
}
