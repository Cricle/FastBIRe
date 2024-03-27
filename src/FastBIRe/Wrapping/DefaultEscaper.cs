using System.Text;

namespace FastBIRe.Wrapping
{
    public class DefaultEscaper : IEscaper
    {
        public static readonly DefaultEscaper MySql = new DefaultEscaper('`', '`', '\'', '\'', '@', true, true);
        public static readonly DefaultEscaper SqlServer = new DefaultEscaper('[', ']', '\'', '\'','@', true);
        public static readonly DefaultEscaper MariaDB = MySql;
        public static readonly DefaultEscaper Sqlite = new DefaultEscaper('`', '`', '\'', '\'','@', true);
        public static readonly DefaultEscaper Oracle = new DefaultEscaper('\"', '\"', '\'', '\'',':', false);
        public static readonly DefaultEscaper PostgreSql = new DefaultEscaper('\"', '\"', '\'', '\'',':', false);
        public static readonly DefaultEscaper DuckDB = new DefaultEscaper('\"', '\"', '\'', '\'',':', false);

        public DefaultEscaper(char qutoStart, char qutoEnd, char valueStart, char valueEnd, char paramterPrefix, bool boolAsInteger, bool escapeBackslash = false)
        {
            QutoStart = qutoStart;
            QutoEnd = qutoEnd;
            ValueStart = valueStart;
            ValueEnd = valueEnd;
            BoolAsInteger = boolAsInteger;
            EscapeBackslash = escapeBackslash;
            ParamterPrefix = paramterPrefix;
        }

        public char QutoStart { get; }

        public char QutoEnd { get; }

        public char ValueStart { get; }

        public char ValueEnd { get; }

        public bool BoolAsInteger { get; }

        public bool EscapeBackslash { get; }

        public char ParamterPrefix { get; }

        public string Quto<T>(T? input)
        {
            return $"{QutoStart}{input}{QutoEnd}";
        }

        public string? WrapValue<T>(T? input)
        {
            if (input == null || Equals(input, DBNull.Value))
            {
                return "NULL";
            }
            else if (input is string || input is Guid)
            {
                var str = ValueStart + input.ToString()?.Replace("'", "''") + ValueEnd;
                if (EscapeBackslash)
                {
                    str = str.Replace("\\", "\\\\");
                }
                return str;
            }
            else if (input is DateTime dt)
            {
                if (dt.Date == dt)
                {
                    return ValueStart + dt.ToString("yyyy-MM-dd") + ValueEnd;
                }
                return ValueStart + dt.ToString("yyyy-MM-dd HH:mm:ss") + ValueEnd;
            }
            else if (input is DateTimeOffset timeOffset)
            {
                return ValueStart + timeOffset.ToLocalTime().DateTime.ToString("yyyy-MM-dd HH:mm:ss") + ValueEnd;
            }
            else if (input is byte[] buffer)
            {
                return "0x" + BitConverter.ToString(buffer).Replace("-", string.Empty);
            }
            else if (input is bool b)
            {
                if (BoolAsInteger)
                {
                    return b ? "1" : "0";
                }
                return b ? boolTrue : boolFalse;
            }
            return input.ToString();
        }

        public string? ReplaceParamterPrefixSql(string? sql, char originPrefix)
        {
            if (string.IsNullOrWhiteSpace(sql) || originPrefix == ParamterPrefix)
            {
                return sql;
            }
            if (!sql.Contains(originPrefix))
            {
                return sql;
            }
            var inString = false;
            var inQuto = false;
            var s = new StringBuilder(sql.Length);
            for (int i = 0; i < sql.Length; i++)
            {
                var c = sql[i];
                if (c == originPrefix && !inString && !inQuto)
                {
                    s.Append(ParamterPrefix);
                }
                else
                {
                    s.Append(c);
                }
                if (c == '\'')
                {
                    inString = !inString;
                }
                else if (c == QutoStart)
                {
                    inQuto = true;
                }
                else if (c == QutoEnd)
                {
                    inQuto = false;
                }
            }
            return s.ToString();
        }

        public string? ReplaceQutoSql(string? sql, char startQuto, char endQuto)
        {
            if (string.IsNullOrWhiteSpace(sql) || (startQuto == QutoStart && endQuto == QutoEnd))
            {
                return sql;
            }
            if (!sql.Contains(startQuto)&&!sql.Contains(endQuto))
            {
                return sql;
            }
            var inString = false;
            var inQuto = false;
            var s = new StringBuilder(sql.Length);
            for (int i = 0; i < sql.Length; i++)
            {
                var c = sql[i];
                if (c == startQuto && !inString && !inQuto)
                {
                    s.Append(QutoStart);
                }
                else if (c == endQuto && !inString && inQuto)
                {
                    s.Append(QutoEnd);
                }
                else
                {
                    s.Append(c);
                }
                if (c == '\'')
                {
                    inString = !inString;
                }
                else if (c == startQuto)
                {
                    inQuto = true;
                }
                else if (c == endQuto)
                {
                    inQuto = false;
                }
            }
            return s.ToString();
        }

        private static readonly string boolTrue = bool.TrueString.ToLower();
        private static readonly string boolFalse = bool.FalseString.ToLower();
    }

}
