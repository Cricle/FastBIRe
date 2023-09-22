using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public static class ViewHelper
    {
        public static string Create(string viewName, string script, SqlType sqlType)
        {
            var qutoViewName = sqlType.Wrap(viewName);
            return $"CREATE VIEW {qutoViewName} AS {script};";
        }
        public static string Drop(string viewName, SqlType sqlType)
        {
            if (sqlType== SqlType.Db2||sqlType== SqlType.Oracle)
            {
                return string.Empty;
            }
            var qutoViewName = sqlType.Wrap(viewName);
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return $"IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'{viewName}')) DROP VIEW {qutoViewName};";
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
