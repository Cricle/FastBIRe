
using System.Text.RegularExpressions;

namespace FastBIRe
{
    public static class ConnectionStringHelper
    {
        private static readonly Regex databaseRegex = new Regex(";?Database=(?<=Database=)[^;]+;?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex sqliteRegex = new Regex(";?Data Source=(?<=Data Source=)[^;]+;?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex databaseSqlServerRegex = new Regex(";?Initial Catalog=(?<=Initial Catalog=)[^;]+;?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string GetNoDatabase(string connectString)
        {
            return databaseSqlServerRegex.Replace(sqliteRegex.Replace(databaseRegex.Replace(connectString, ";"), ";"), ";");
        }
        public static string SetDatabase(string connectString, string database, FSqlType sqlType)
        {
            string repl;
            if (!connectString.EndsWith(";"))
            {
                connectString += ";";
            }
            switch (sqlType)
            {
                case FSqlType.SQLite:
                case FSqlType.DuckDB:
                    repl = $"Data Source={database};";
                    break;
                default:
                    repl = $"Database={database};";
                    break;
            }
            var replaceRes= databaseSqlServerRegex.Replace(databaseRegex.Replace(connectString, repl), repl);
            if (replaceRes==connectString)
            {
                replaceRes += $"Database={database}";
            }

            return replaceRes;
        }
    }
}
