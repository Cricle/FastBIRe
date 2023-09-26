using DatabaseSchemaReader.DataSchema;
using FastBIRe.Wrapping;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace FastBIRe
{
    public partial class FunctionMapper
    {
        public static readonly FunctionMapper MySql = new FunctionMapper(SqlType.MySql);
        public static readonly FunctionMapper Sqlite = new FunctionMapper(SqlType.SQLite);
        public static readonly FunctionMapper PostgreSql = new FunctionMapper(SqlType.PostgreSql);
        public static readonly FunctionMapper SqlServer = new FunctionMapper(SqlType.SqlServer);

        public static FunctionMapper? Get(SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return SqlServer;
                case SqlType.MySql:
                    return MySql;
                case SqlType.SQLite:
                    return Sqlite;
                case SqlType.PostgreSql:
                    return PostgreSql;
                case SqlType.Db2:
                case SqlType.Oracle:
                default:
                    return null;
            }
        }

#if false
(?<!\\)" "->'
(?<![\"'])\b[A-Za-z]+\b(?=\()
#endif
#if NET7_0_OR_GREATER
        [GeneratedRegex("(?<!\\\\)\"")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static partial Regex GetQutoRegex();
        [GeneratedRegex("(?<![\\\"'])\\b[A-Za-z]+\\b(?=\\()")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static partial Regex GetMethodNameRegex();
#else
        private static readonly Regex qutoRegex = new Regex("(?<!\\\\)\"", RegexOptions.Compiled);
        private static readonly Regex methodNameRegex = new Regex("(?<![\\\"'])\\b[A-Za-z]+\\b(?=\\()", RegexOptions.Compiled);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Regex GetQutoRegex()
        {
            return qutoRegex;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Regex GetMethodNameRegex()
        {
            return methodNameRegex;
        }
#endif

        public FunctionMapper(SqlType sqlType)
        {
            SqlType = sqlType;
            Escaper = sqlType.GetMethodWrapper();
        }

        public IEscaper Escaper { get; }

        public SqlType SqlType { get; }

        private string CastMySql(string input, DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.Guid:
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.AnsiStringFixedLength:
                    return $"CAST({input} AS CHAR)";
                case DbType.Date:
                    return $"CAST({input} AS DATE)";
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return $"CAST({input} AS DATETIME)";
                case DbType.Decimal:
                    return $"CAST({input} AS DECIMAL)";
                case DbType.Double:
                    return $"CAST({input} AS DOUBLE)";
                case DbType.Boolean:
                case DbType.Byte:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.SByte:
                    return $"CAST({input} AS SIGNED)";
                case DbType.Single:
                    return $"CAST({input} AS FLOAT)";
                case DbType.Time:
                    return $"CAST({input} AS TIME)";
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                    return $"CAST({input} AS UNSIGNED)";
                default:
                    return input;
            }
        }
        private string CastPostgresql(string input, DbType dbType)
        {
            switch (dbType)
            {
                case DbType.Guid:
                    return $"{input}::UUID";
                case DbType.AnsiString:
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.AnsiStringFixedLength:
                    return $"{input}::VARCHAR";
                case DbType.Date:
                    return $"{input}::DATE";
                case DbType.DateTime2:
                case DbType.DateTime:
                case DbType.DateTimeOffset:
                    return $"{input}::TIMESTAMP";
                case DbType.Decimal:
                case DbType.Double:
                    return $"{input}::DECIMAL";
                case DbType.Boolean:
                case DbType.Byte:
                case DbType.Int16:
                case DbType.SByte:
                    return $"{input}::SMALLINT";
                case DbType.Int32:
                    return $"{input}::INT";
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                case DbType.Int64:
                    return $"{input}::BIGINT";
                case DbType.Single:
                    return $"{input}::FLOAT";
                case DbType.Time:
                    return $"{input}::TIME";
                default:
                    return input;
            }
        }
        private string CastSqlServer(string input, DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.Guid:
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.AnsiStringFixedLength:
                    return $"CONVERT(VARCHAR,{input},120)";
                case DbType.Date:
                    return $"CONVERT(DATE,{input},120)";
                case DbType.DateTime2:
                    return $"CONVERT(DATETIME2,{input},120)";
                case DbType.DateTime:
                case DbType.DateTimeOffset:
                    return $"CONVERT(DATETIME,{input},120)";
                case DbType.Decimal:
                case DbType.Double:
                    return $"CONVERT(DECIMAL,{input},120)";
                case DbType.Boolean:
                case DbType.Byte:
                case DbType.Int16:
                case DbType.SByte:
                case DbType.Int32:
                    return $"CONVERT(INT,{input},120)";
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                case DbType.Int64:
                    return $"CONVERT(BIGINT,{input},120)";
                case DbType.Single:
                    return $"CONVERT(FLOAT,{input},120)";
                case DbType.Time:
                    return $"CONVERT(TIME,{input},120)";
                default:
                    return input;
            }
        }
        private string CastSqlite(string input, DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.Guid:
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.AnsiStringFixedLength:
                    return $"CAST({input} AS TEXT)";
                case DbType.Date:
                case DbType.Time:
                case DbType.DateTime2:
                case DbType.DateTime:
                case DbType.DateTimeOffset:
                    return $"CAST({input} AS DATETIME)";
                case DbType.Decimal:
                case DbType.Single:
                case DbType.Double:
                    return $"CAST({input} AS REAL)";
                case DbType.Boolean:
                case DbType.Byte:
                case DbType.Int16:
                case DbType.SByte:
                case DbType.Int32:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                case DbType.Int64:
                    return $"CAST({input} AS INTEGER)";
                default:
                    return input;
            }
        }
        public string? Cast(string input, DbType dbType)
        {
            switch (SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    return CastSqlServer(input, dbType);
                case SqlType.MySql:
                    return CastMySql(input, dbType);
                case SqlType.SQLite:
                    return CastSqlite(input, dbType);
                case SqlType.PostgreSql:
                    return CastPostgresql(input, dbType);
                case SqlType.Db2:
                case SqlType.Oracle:
                default:
                    return null;
            }
        }
        public string? Value<T>(T value)
        {
            var str = Escaper.WrapValue(value);
            if (value is string && str != null && SqlType == SqlType.SqlServer || SqlType == SqlType.SqlServerCe)
            {
                str = "N" + str;
            }
            return str;
        }
        public string PartitionBy(params string[] areas)
        {
            return $"PARTITION BY {string.Join(",", areas)}";
        }
        public string Over(string inner)
        {
            return $"OVER({inner})";
        }
        public string Quto(string name)
        {
            return Escaper.Quto(name);
        }
        public string Bracket(string input)
        {
            return $"({input})";
        }
        public string IsNull(string input)
        {
            return $"ISNULL({input})";
        }
        public string RowNumber(string? orderby = null)
        {
            if (string.IsNullOrEmpty(orderby))
            {
                orderby = "(SELECT NULL)";
            }
            return $"ROW_NUMBER() OVER (ORDER BY {orderby})";
        }
        public string Rank(string? orderby = null)
        {
            if (string.IsNullOrEmpty(orderby))
            {
                orderby = "(SELECT NULL)";
            }
            return $"RANK() OVER (ORDER BY {orderby})";
        }
        public string? Version()
        {
            switch (SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    return $"@@VERSION";
                case SqlType.PostgreSql:
                case SqlType.MySql:
                    return $"VERSION()";
                case SqlType.SQLite:
                    return $"sqlite_version()";
                default:
                    return null;
            }
        }
        public string Coalesce(string input, string nullIf)
        {
            if (SqlType == SqlType.SQLite)
            {
                return $"IFNULL({input}, {nullIf})";
            }
            return $"COALESCE({input}, {nullIf})";
        }
        public string? Map(SQLFunctions func, params string[] args)
        {
            switch (func)
            {
                case SQLFunctions.Date:
                    return Date(args[0], args[1], args[2]);
                case SQLFunctions.DateDif:
                    return DateDif(args[0], args[1], args[2]);
                case SQLFunctions.Day:
                    return Day(args[0]);
                case SQLFunctions.Year:
                    return Year(args[0]);
                case SQLFunctions.Month:
                    return Month(args[0]);
                case SQLFunctions.Minute:
                    return Minute(args[0]);
                case SQLFunctions.Second:
                    return Second(args[0]);
                case SQLFunctions.NetWorkDays:
                    return NetWorkDays(args[0], args[1], args.Skip(2));
                case SQLFunctions.Now:
                    return Now();
                case SQLFunctions.ToDay:
                    return ToDay();
                case SQLFunctions.Days:
                    return Days(args[0], args[1]);
                case SQLFunctions.Weakday:
                    return Weakday(args[0], args[1]);
                case SQLFunctions.WeakNum:
                    return WeakNum(args[0]);
                case SQLFunctions.WorkDay:
                    return WorkDay(args[0], args[1], args.Skip(2));
                case SQLFunctions.Abs:
                    return Abs(args[0]);
                case SQLFunctions.Rand:
                    return Rand();
                case SQLFunctions.RandBetween:
                    return RandBetween(args[0], args[1]);
                case SQLFunctions.Round:
                    return Round(args[0], args[1]);
                case SQLFunctions.RoundUp:
                    return RoundUp(args[0], args[1]);
                case SQLFunctions.RoundDown:
                    return RoundDown(args[0], args[1]);
                case SQLFunctions.Sum:
                    return Sum(args);
                case SQLFunctions.Count:
                    return Count(args);
                case SQLFunctions.CountA:
                    return CountA(args);
                case SQLFunctions.Average:
                    return Average(args);
                case SQLFunctions.Max:
                    return Max(args);
                case SQLFunctions.Min:
                    return Min(args);
                case SQLFunctions.Median:
                    return Median(args);
                case SQLFunctions.Char:
                    return Char(args[1]);
                case SQLFunctions.Concatenate:
                    return Concatenate(args);
                case SQLFunctions.Left:
                    return Left(args[0], args[1]);
                case SQLFunctions.Right:
                    return Right(args[0], args[1]);
                case SQLFunctions.Len:
                    return Len(args[0]);
                case SQLFunctions.Lower:
                    return Lower(args[0]);
                case SQLFunctions.Mid:
                    return Mid(args[0], args[1], args[2]);
                case SQLFunctions.Replace:
                    return Replace(args[0], args[1], args[2], args[3]);
                case SQLFunctions.ToDate:
                    return ToDate(args[0]);
                case SQLFunctions.Trim:
                    return Trim(args[0]);
                case SQLFunctions.Upper:
                    return Upper(args[0]);
                case SQLFunctions.If:
                    return If(args[0], args[1], args[2]);
                case SQLFunctions.True:
                    return True();
                case SQLFunctions.False:
                    return False();
                case SQLFunctions.And:
                    return And(args);
                case SQLFunctions.Or:
                    return Or(args);
                case SQLFunctions.Not:
                    return Not(args[0]);
                case SQLFunctions.Bracket:
                    return Bracket(args[0]);
                case SQLFunctions.DateAdd:
                    return DateAdd(args[0], args[1], args[2]);
                case SQLFunctions.IsNull:
                    return IsNull(args[0]);
                case SQLFunctions.Like:
                    return Like(args[0]);
                case SQLFunctions.Reverse:
                    return Reverse(args[0]);
                case SQLFunctions.DayOfYear:
                    return DayOfYear(args[0]);
                case SQLFunctions.LastDay:
                    return LastDay(args[0]);
                case SQLFunctions.MinC:
                    return MinC(args[0]);
                case SQLFunctions.MaxC:
                    return MaxC(args[0]);
                case SQLFunctions.AverageC:
                    return AverageC(args[0]);
                case SQLFunctions.SumC:
                    return SumC(args[0]);
                case SQLFunctions.CountC:
                    return CountC(args[0]);
                case SQLFunctions.DistinctCountC:
                    return DistinctCountC(args[0]);
                case SQLFunctions.Coalesce:
                    return Coalesce(args[0], args[1]);
                case SQLFunctions.RowNumber:
                    return RowNumber(args.Length > 0 ? args[0] : null);
                case SQLFunctions.Rank:
                    return Rank(args.Length > 0 ? args[0] : null);
                case SQLFunctions.ACos:
                    return ACos(args[0]);
                case SQLFunctions.ASin:
                    return ASin(args[0]);
                case SQLFunctions.ATan:
                    return ATan(args[0]);
                case SQLFunctions.ATan2:
                    return ATan2(args[0], args[2]);
                case SQLFunctions.Cos:
                    return Cos(args[0]);
                case SQLFunctions.Sin:
                    return Sin(args[0]);
                case SQLFunctions.Cot:
                    return Cot(args[0]);
                case SQLFunctions.Degress:
                    return Degress(args[0]);
                case SQLFunctions.Exp:
                    return Exp(args[0]);
                case SQLFunctions.Ln:
                    return Ln(args[0]);
                case SQLFunctions.PI:
                    return PI();
                case SQLFunctions.Pow:
                    return Pow(args[0], args[1]);
                case SQLFunctions.Sqrt:
                    return Sqrt(args[0]);
                case SQLFunctions.Log:
                    return Log(args[0], args.Length > 1 ? args[1] : null);
                case SQLFunctions.Week:
                    return Week(args[0]);
                case SQLFunctions.Quarter:
                    return Quarter(args[0]);
                case SQLFunctions.Hour:
                    return Hour(args[0]);
                case SQLFunctions.HourTo:
                    return HourTo(args[0]);
                case SQLFunctions.MinuteTo:
                    return MinuteTo(args[0]);
                case SQLFunctions.DayTo:
                    return DayTo(args[0]);
                case SQLFunctions.MonthTo:
                    return MonthTo(args[0]);
                case SQLFunctions.YearTo:
                    return YearTo(args[0]);
                default:
                    return null;
            }
        }
    }
}
