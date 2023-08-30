using Ao.Stock.Querying;
using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public static class MethodWrapperHelper
    {
        public static IMethodWrapper GetMethodWrapper(SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return DefaultMethodWrapper.SqlServer;
                case SqlType.Oracle:
                    return DefaultMethodWrapper.Oracle;
                case SqlType.MySql:
                    return DefaultMethodWrapper.MySql;
                case SqlType.SQLite:
                    return DefaultMethodWrapper.Sqlite;
                case SqlType.PostgreSql:
                    return DefaultMethodWrapper.PostgreSql;
                case SqlType.Db2:
                default:
                    throw new NotSupportedException(sqlType.ToString());
            }
        }
    }
}
