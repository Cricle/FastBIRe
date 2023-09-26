using DatabaseSchemaReader.DataSchema;
using FastBIRe.Creating;
using FastBIRe.Wrapping;

namespace FastBIRe
{
    public static class MethodWrapperExtensions
    {
        public static bool Ors(this SqlType sqlType,params SqlType[] types)
        {
            foreach (var item in types)
            {
                if (sqlType==item)
                {
                    return true;
                }
            }
            return false;
        }

        public static string Wrap(this SqlType sqlType, string? field)
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
        public static IEscaper GetMethodWrapper(this SqlType sqlType)
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
                case SqlType.Db2:
                default:
                    throw new NotSupportedException(sqlType.ToString());
            }
        }
    }
}
