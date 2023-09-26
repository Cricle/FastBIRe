﻿namespace FastBIRe.Wrapping
{
    public class DefaultEscaper : IEscaper
    {
        public static readonly DefaultEscaper MySql = new DefaultEscaper("`", "`", "'", "'", true);
        public static readonly DefaultEscaper SqlServer = new DefaultEscaper("[", "]", "'", "'", true);
        public static readonly DefaultEscaper MariaDB = MySql;
        public static readonly DefaultEscaper Sqlite = new DefaultEscaper("`", "`", "'", "'", true);
        public static readonly DefaultEscaper Oracle = new DefaultEscaper("\"", "\"", "'", "'", false);
        public static readonly DefaultEscaper PostgreSql = new DefaultEscaper("\"", "\"", "'", "'", false);

        public DefaultEscaper(string qutoStart, string qutoEnd, string valueStart, string valueEnd, bool boolAsInteger)
        {
            QutoStart = qutoStart;
            QutoEnd = qutoEnd;
            ValueStart = valueStart;
            ValueEnd = valueEnd;
            BoolAsInteger = boolAsInteger;
        }

        public string QutoStart { get; }

        public string QutoEnd { get; }

        public string ValueStart { get; }

        public string ValueEnd { get; }

        public bool BoolAsInteger { get; }

        public string Quto<T>(T input)
        {
            return QutoStart + input + QutoEnd;
        }

        public string? WrapValue<T>(T input)
        {
            if (input == null || Equals(input, DBNull.Value))
            {
                return "NULL";
            }
            else if (input is string)
            {
                return ValueStart + input + ValueEnd;
            }
            else if (input is DateTime || input is DateTime?)
            {
                var dt = (DateTime)(object)input;
                if (dt.Date == dt)
                {
                    return ValueStart + dt.ToString("yyyy-MM-dd") + ValueEnd;
                }
                return ValueStart + DateTimeToStringHelper.ToFullString(dt) + ValueEnd;
            }
            else if (input is byte[] buffer)
            {
                return "0x" + BitConverter.ToString(buffer).Replace("-", string.Empty);
            }
            else if (input is Guid guid)
            {
                return ValueStart + guid + ValueEnd;
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
        private static readonly string boolTrue = bool.TrueString.ToLower();
        private static readonly string boolFalse = bool.FalseString.ToLower();
    }

}
