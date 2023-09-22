using Ao.Stock.Querying;
using DatabaseSchemaReader.DataSchema;
using FastBIRe.Creating;

namespace FastBIRe
{
    public static class MethodWrapperExtensions
    {
        public static string Wrap(this SqlType sqlType, string field)
        {
            return GetMethodWrapper(sqlType).Quto(field);
        }
        public static string? WrapValue<T>(this SqlType sqlType, T value)
        {
            return GetMethodWrapper(sqlType).WrapValue(value);
        }
        public static IDatabaseCreateAdapter? GetDatabaseCreateAdapter<T>(this SqlType sqlType)
        {
            return DatabaseCreateAdapter.Get(sqlType);
        }
        public static IMethodWrapper GetMethodWrapper(this SqlType sqlType)
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
