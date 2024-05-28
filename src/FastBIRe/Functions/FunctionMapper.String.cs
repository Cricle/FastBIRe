using DatabaseSchemaReader.DataSchema;
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
            switch (SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                case SqlType.SQLite:
                    return $"STDEV({input})";
                case SqlType.MySql:
                case SqlType.PostgreSql:
                    return $"STDDEV_POP({input})";
                default:
                    return null;
            }
        }
        public string? Var(string input)
        {
            switch (SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    return $"VAR({input})";
                case SqlType.MySql:
                    return $"VAR_POP({input})";
                case SqlType.SQLite:
                case SqlType.PostgreSql:
                    return $"VARIANCE({input})";
                default:
                    return null;
            }
        }
        public string? Char(string input)
        {
            switch (SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                case SqlType.MySql:
                case SqlType.SQLite:
                    return $"CHAR({input})";
                case SqlType.PostgreSql:
                    return $"CHR({input})";
                default:
                    return null;
            }
        }
        public string? Concatenate(params string[] inputs)
        {
            var inputCast = inputs.Select(x => Cast(x, DbType.String));
            switch (SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                case SqlType.MySql:
                    return $"CONCAT({string.Join(" , ", inputCast)})";
                case SqlType.SQLite:
                case SqlType.PostgreSql:
                    return string.Join(" || ", inputCast);
                default:
                    return null;
            }
        }
        public string? Ascii(string input)
        {
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                case SqlType.PostgreSql:
                case SqlType.MySql:
                    return $"ASCII({input})";
                case SqlType.SQLite:
                    return $"UNICODE(substr({input}, 1, 1))";
                case SqlType.Db2:
                case SqlType.Oracle:
                default:
                    return null;
            }
        }
        public string Left(string input, string length)
        {
            var addition = SqlType == SqlType.PostgreSql ? "::VARCHAR" : string.Empty;
            return $"LEFT({input}{addition},{length})";
        }
        public string Right(string input, string length)
        {
            var addition = SqlType == SqlType.PostgreSql ? "::VARCHAR" : string.Empty;
            return $"RIGHT({input}{addition},{length})";
        }
        public string? Len(string input)
        {
            switch (SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    return $"LEN({input})";
                case SqlType.MySql:
                    return $"CHAR_LENGTH(CAST({input} AS CHAR))";
                case SqlType.SQLite:
                    return $"LENGTH(CAST({input} AS TEXT))";
                case SqlType.PostgreSql:
                    return $"LENGTH(CAST({input} AS VARCHAR))";
                default:
                    return null;
            }
        }
        public string? Lower(string input)
        {
            switch (SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    return $"LOWER({input})";
                case SqlType.MySql:
                    return $"LOWER(CAST({input} AS CHAR))";
                case SqlType.SQLite:
                    return $"LOWER(CAST({input} AS TEXT))";
                case SqlType.PostgreSql:
                    return $"LOWER(CAST({input} AS VARCHAR))";
                default:
                    return null;
            }
        }
        public string? Mid(string input, string startIndex, string length)
        {
            switch (SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    return $"SUBSTRING({input},{startIndex},{length})";
                case SqlType.MySql:
                    return $"SUBSTR(CAST({input} AS CHAR),{startIndex},{length})";
                case SqlType.SQLite:
                    return $"SUBSTR(CAST({input} AS TEXT),{startIndex},{length})";
                case SqlType.PostgreSql:
                    return $"SUBSTRING(CAST({input} AS VARCHAR),{startIndex},{length})";
                default:
                    return null;
            }
        }
        public string? Replace(string input, string startIndex, string length, string newText)
        {
            switch (SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    return $"STUFF({input},{startIndex},{length},{newText})";
                case SqlType.MySql:
                    return $"INSERT(CAST({input} AS CHAR),{startIndex},{length},{newText})";
                case SqlType.SQLite:
                    return $"SUBSTR(CAST({input} AS TEXT),{startIndex},{length})";
                case SqlType.PostgreSql:
                    return $"OVERLAY(CAST({input} AS VARCHAR) PLACING {newText} FROM {startIndex} FOR {length})";
                default:
                    return null;
            }
        }
        public string? ToDate(string input)
        {
            switch (SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    return $"CONVERT(datetime, {input})";
                case SqlType.MySql:
                    return $"CAST({input} AS DATETIME)";
                case SqlType.SQLite:
                    return $"strftime('%Y-%m-%d %H:%M:%S', {input})";
                case SqlType.PostgreSql:
                    return $"to_timestamp({input},\"YYYY-MM-DD HH:mm:ss\")";
                default:
                    return null;
            }
        }
        public string? Trim(string input)
        {
            switch (SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    return $"TRIM({input})";
                case SqlType.MySql:
                    return $"TRIM(CAST({input} AS CHAR))";
                case SqlType.SQLite:
                    return $"TRIM(CAST({input} AS TEXT))";
                case SqlType.PostgreSql:
                    return $"TRIM(CAST({input} AS VARCHAR))";
                default:
                    return null;
            }
        }
        public string? Upper(string input)
        {
            switch (SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    return $"UPPER({input})";
                case SqlType.MySql:
                    return $"UPPER(CAST({input} AS CHAR))";
                case SqlType.SQLite:
                    return $"UPPER(CAST({input} AS TEXT))";
                case SqlType.PostgreSql:
                    return $"UPPER(CAST({input} AS VARCHAR))";
                default:
                    return null;
            }
        }
    }
}
