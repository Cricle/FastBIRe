using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public static class ViewHelper
    {
        public static string Create(string viewName,string script, SqlType sqlType)
        {
            var qutoViewName = MergeHelper.GetMethodWrapper(sqlType).Quto(viewName);
            return $"CREATE VIEW {qutoViewName} AS {script};";
        }
        public static string Drop(string viewName, SqlType sqlType)
        {
            var qutoViewName = MergeHelper.GetMethodWrapper(sqlType).Quto(viewName);
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return $"IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'{qutoViewName}')) DROP VIEW {qutoViewName};";
                case SqlType.MySql:
                case SqlType.SQLite:
                case SqlType.PostgreSql:
                    return $"DROP VIEW IF EXISTS {qutoViewName};";
                default:
                    return string.Empty;
            }
        }
    }
}
