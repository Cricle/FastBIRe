using System.Text.RegularExpressions;

namespace FastBIRe.Comparing
{
    public static partial class SqlComparer
    {
        private static Regex spaceMatch = new Regex("\\s{1,}");
        private static Regex spaceLeftMatch = new Regex("\\s{1,}\\(");
        private static Regex spaceRightMatch = new Regex("\\s{1,}\\)");

        private static string MiniSql(string sql)
        {
            return spaceMatch.Replace(spaceLeftMatch.Replace(spaceRightMatch.Replace(sql, ")"), "("), " ");
        }

        public static bool Compare(string left, string right)
        {
            return string.Equals(MiniSql(left).Replace(";", string.Empty), MiniSql(right).Replace(";", string.Empty), StringComparison.OrdinalIgnoreCase);
        }
    }
}
