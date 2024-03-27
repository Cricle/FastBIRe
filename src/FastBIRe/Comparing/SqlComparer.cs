using System.Text.RegularExpressions;

namespace FastBIRe.Comparing
{
    public class SqlComparer : IEqualityComparer<string?>
    {
        private static Regex spaceMatch = new Regex("\\s{1,}");
        private static Regex spaceLeftMatch = new Regex("\\s{1,}\\(");
        private static Regex spaceRightMatch = new Regex("\\s{1,}\\)");

        public static readonly SqlComparer Instance = new SqlComparer();

        private static string MiniSql(string sql)
        {
            return spaceMatch.Replace(spaceLeftMatch.Replace(spaceRightMatch.Replace(sql, ")"), "("), " ");
        }

        public static bool Compare(string left, string right)
        {
            return string.Equals(MiniSql(left).Trim().Replace(";", string.Empty), MiniSql(right).Trim().Replace(";", string.Empty), StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(string? x, string? y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            return Compare(x, y);
        }

        public int GetHashCode(string? obj)
        {
            return obj?.GetHashCode() ?? 0;
        }
    }
}
