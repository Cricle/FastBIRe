
using System.Data;

namespace FastBIRe
{
    public partial class FunctionMapper
    {
        public string Reverse(string input)
        {
            return $"REVERSE({input})";
        }
        public string Like(string input)
        {
            return $"like {input}";
        }
        public string? Stdev(string input)
        {
            switch (FSqlType)
            {
                case FSqlType.SqlServer:
                case FSqlType.SqlServerCe:
                case FSqlType.SQLite:
                    return $"STDEV({input})";
                case FSqlType.MySql:
                case FSqlType.PostgreSql:
                    return $"STDDEV_POP({input})";
                default:
                    return null;
            }
        }
        public string? Var(string input)
        {
            switch (FSqlType)
            {
                case FSqlType.SqlServer:
                case FSqlType.SqlServerCe:
                    return $"VAR({input})";
                case FSqlType.MySql:
                    return $"VAR_POP({input})";
                case FSqlType.SQLite:
                case FSqlType.PostgreSql:
                    return $"VARIANCE({input})";
                default:
                    return null;
            }
        }
        public string? Char(string input)
        {
            switch (FSqlType)
            {
                case FSqlType.SqlServer:
                case FSqlType.SqlServerCe:
                case FSqlType.MySql:
                case FSqlType.SQLite:
                    return $"CHAR({input})";
                case FSqlType.PostgreSql:
                    return $"CHR({input})";
                default:
                    return null;
            }
        }
        public string? Concatenate(params string[] inputs)
        {
            var inputCast = inputs.Select(x => Cast(x, DbType.String));
            switch (FSqlType)
            {
                case FSqlType.SqlServer:
                case FSqlType.SqlServerCe:
                case FSqlType.MySql:
                    return $"CONCAT({string.Join(" , ", inputCast)})";
                case FSqlType.SQLite:
                case FSqlType.PostgreSql:
                    return string.Join(" || ", inputCast);
                default:
                    return null;
            }
        }
        public string? Ascii(string input)
        {
            switch (FSqlType)
            {
                case FSqlType.SqlServerCe:
                case FSqlType.SqlServer:
                case FSqlType.PostgreSql:
                case FSqlType.MySql:
                    return $"ASCII({input})";
                case FSqlType.SQLite:
                    return $"UNICODE(substr({input}, 1, 1))";
                case FSqlType.Db2:
                case FSqlType.Oracle:
                default:
                    return null;
            }
        }
        public string Left(string input, string length)
        {
            var addition = FSqlType == FSqlType.PostgreSql ? "::VARCHAR" : string.Empty;
            return $"LEFT({input}{addition},{length})";
        }
        public string Right(string input, string length)
        {
            var addition = FSqlType == FSqlType.PostgreSql ? "::VARCHAR" : string.Empty;
            return $"RIGHT({input}{addition},{length})";
        }
        public string? Len(string input)
        {
            switch (FSqlType)
            {
                case FSqlType.SqlServer:
                case FSqlType.SqlServerCe:
                    return $"LEN({input})";
                case FSqlType.MySql:
                    return $"CHAR_LENGTH(CAST({input} AS CHAR))";
                case FSqlType.SQLite:
                    return $"LENGTH(CAST({input} AS TEXT))";
                case FSqlType.PostgreSql:
                    return $"LENGTH(CAST({input} AS VARCHAR))";
                default:
                    return null;
            }
        }
        public string? Lower(string input)
        {
            switch (FSqlType)
            {
                case FSqlType.SqlServer:
                case FSqlType.SqlServerCe:
                    return $"LOWER({input})";
                case FSqlType.MySql:
                    return $"LOWER(CAST({input} AS CHAR))";
                case FSqlType.SQLite:
                    return $"LOWER(CAST({input} AS TEXT))";
                case FSqlType.PostgreSql:
                    return $"LOWER(CAST({input} AS VARCHAR))";
                default:
                    return null;
            }
        }
        public string? Mid(string input, string startIndex, string length)
        {
            switch (FSqlType)
            {
                case FSqlType.SqlServer:
                case FSqlType.SqlServerCe:
                    return $"SUBSTRING({input},{startIndex},{length})";
                case FSqlType.MySql:
                    return $"SUBSTR(CAST({input} AS CHAR),{startIndex},{length})";
                case FSqlType.SQLite:
                    return $"SUBSTR(CAST({input} AS TEXT),{startIndex},{length})";
                case FSqlType.PostgreSql:
                    return $"SUBSTRING(CAST({input} AS VARCHAR),{startIndex},{length})";
                default:
                    return null;
            }
        }
        public string? Replace(string input, string startIndex, string length, string newText)
        {
            switch (FSqlType)
            {
                case FSqlType.SqlServer:
                case FSqlType.SqlServerCe:
                    return $"STUFF({input},{startIndex},{length},{newText})";
                case FSqlType.MySql:
                    return $"INSERT(CAST({input} AS CHAR),{startIndex},{length},{newText})";
                case FSqlType.SQLite:
                    return $"SUBSTR(CAST({input} AS TEXT),{startIndex},{length})";
                case FSqlType.PostgreSql:
                    return $"OVERLAY(CAST({input} AS VARCHAR) PLACING {newText} FROM {startIndex} FOR {length})";
                default:
                    return null;
            }
        }
        public string? ToDate(string input)
        {
            switch (FSqlType)
            {
                case FSqlType.SqlServer:
                case FSqlType.SqlServerCe:
                    return $"CONVERT(datetime, {input})";
                case FSqlType.MySql:
                    return $"CAST({input} AS DATETIME)";
                case FSqlType.SQLite:
                    return $"strftime('%Y-%m-%d %H:%M:%S', {input})";
                case FSqlType.PostgreSql:
                    return $"to_timestamp({input},\"YYYY-MM-DD HH:mm:ss\")";
                default:
                    return null;
            }
        }
        public string? Trim(string input)
        {
            switch (FSqlType)
            {
                case FSqlType.SqlServer:
                case FSqlType.SqlServerCe:
                    return $"TRIM({input})";
                case FSqlType.MySql:
                    return $"TRIM(CAST({input} AS CHAR))";
                case FSqlType.SQLite:
                    return $"TRIM(CAST({input} AS TEXT))";
                case FSqlType.PostgreSql:
                    return $"TRIM(CAST({input} AS VARCHAR))";
                default:
                    return null;
            }
        }
        public string? Upper(string input)
        {
            switch (FSqlType)
            {
                case FSqlType.SqlServer:
                case FSqlType.SqlServerCe:
                    return $"UPPER({input})";
                case FSqlType.MySql:
                    return $"UPPER(CAST({input} AS CHAR))";
                case FSqlType.SQLite:
                    return $"UPPER(CAST({input} AS TEXT))";
                case FSqlType.PostgreSql:
                    return $"UPPER(CAST({input} AS VARCHAR))";
                default:
                    return null;
            }
        }
    }
}
