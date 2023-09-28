using System.Text.RegularExpressions;

namespace FastBIRe
{
    public static class ConnectionStringHelper
    {
        private static readonly Regex databaseRegex = new Regex(";?Database=(?<=Database=)[^;]+;?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex databaseSqlServerRegex = new Regex(";?Initial Catalog=(?<=Initial Catalog=)[^;]+;?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string GetNoDatabase(string connectString)
        {
            return databaseSqlServerRegex.Replace(databaseRegex.Replace(connectString, ";"), ";");
        }
        public static string SetDatabase(string connectString, string database)
        {
            var repl = $";Database={database};";
            return databaseSqlServerRegex.Replace(databaseRegex.Replace(connectString, repl), repl);
        }
    }
}
