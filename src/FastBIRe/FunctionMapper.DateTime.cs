using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public partial class FunctionMapper
    {
        public string? LastDay(string date)
        {
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return $"DATEADD(day, -1, DATEADD(month, DATEDIFF(month, 0, {date}) + 1, 0))";
                case SqlType.MySql:
                    return $"LAST_DAY({date})";
                case SqlType.SQLite:
                    return $"DATE(strftime('%Y-%m-', {date}) || '01', '+1 month', '-1 day')";
                case SqlType.PostgreSql:
                    return $"DATE_TRUNC('month', {date}) + INTERVAL '1 month - 1 day'";
                default:
                    return null;
            }
        }
        public string? DayOfYear(string date)
        {
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return $"DATEPART(dayofyear, {date})";
                case SqlType.MySql:
                    return $"DAYOFYEAR({date})";
                case SqlType.SQLite:
                    return $"STRFTIME('%j', {date})";
                case SqlType.PostgreSql:
                    return $"EXTRACT(DOY FROM {date})";
                default:
                    return null;
            }
        }
        public string? Date(string year, string month, string day)
        {
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return $"DATEFROMPARTS({year}, {month}, {day})";
                case SqlType.MySql:
                    return $"STR_TO_DATE(CONCAT({year}, '-', {month}, '-', {day}), '%Y-%m-%d')";
                case SqlType.SQLite:
                    return $"DATE({year}|| '-'|| {month}|| '-'|| {day})";
                case SqlType.PostgreSql:
                    return $"TO_DATE({year}|| '-'|| {month}|| '-'|| {day},'YYYY-MM-DD')";
                default:
                    return null;
            }
        }
        public string DateDif(string timeA, string timeB, string unit)
        {
            return $@"
CASE WHEN {unit}='Y' THEN YEAR({timeA})-YEAR({timeB}) 
WHEN {unit}='M' THEN MONTH({timeA})-MONTH({timeB}) 
WHEN {unit}='D' THEN DAY({timeA})-DAY({timeB}) 
END
";
        }
        public string? GetUnitString(DateTimeUnit unit)
        {
            return unit.ToString().ToUpper();
        }
        public string? DateAdd(string time,string num, DateTimeUnit unit)
        {
            var unitStr = GetUnitString(unit);
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return $"DATEADD({unitStr}, {num}, {time})";
                case SqlType.MySql:
                    return $"DATE_ADD({time}, INTERVAL {num} {unitStr})";
                case SqlType.SQLite:
                    return $"DATE({time}, '{num} {unitStr}')";
                case SqlType.PostgreSql:
                    return $"{time} + INTERVAL '{num} {unitStr}'";
                default:
                    return null;
            }
        }
        public string Day(string time)
        {
            if (SqlType == SqlType.SQLite)
            {
                return $"strftime('%d', {time})";
            }
            else if (SqlType== SqlType.SqlServer||SqlType== SqlType.SqlServerCe)
            {
                return $"DATEPART(day,{time})";
            }
            return $"DAY({time})";
        }
        public string Year(string time)
        {
            if (SqlType == SqlType.SQLite)
            {
                return $"strftime('%Y', {time})";
            }
            else if (SqlType == SqlType.SqlServer || SqlType == SqlType.SqlServerCe)
            {
                return $"DATEPART(year,{time})";
            }
            return $"YEAR({time})";
        }
        public string Month(string time)
        {
            if (SqlType == SqlType.SQLite)
            {
                return $"strftime('%m', {time})";
            }
            else if (SqlType == SqlType.SqlServer || SqlType == SqlType.SqlServerCe)
            {
                return $"DATEPART(month,{time})";
            }
            return $"MONTH({time})";
        }
        public string Hour(string time)
        {
            if (SqlType == SqlType.SQLite)
            {
                return $"strftime('%H', {time})";
            }
            else if (SqlType == SqlType.SqlServer || SqlType == SqlType.SqlServerCe)
            {
                return $"DATEPART(hour,{time})";
            }
            return $"HOUR({time})";
        }
        public string Minute(string time)
        {
            if (SqlType == SqlType.SQLite)
            {
                return $"strftime('%M', {time})";
            }
            else if (SqlType == SqlType.SqlServer || SqlType == SqlType.SqlServerCe)
            {
                return $"DATEPART(minute,{time})";
            }
            return $"Minute({time})";
        }
        public string Second(string time)
        {
            if (SqlType == SqlType.SQLite)
            {
                return $"strftime('%S', {time})";
            }
            else if (SqlType == SqlType.SqlServer || SqlType == SqlType.SqlServerCe)
            {
                return $"DATEPART(second,{time})";
            }
            return $"Second({time})";
        }
        public string? NetWorkDays(string timeA, string timeB, IEnumerable<string> times)
        {
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    {
                        return $@"
SELECT COUNT(*)
FROM (
    SELECT DATEADD(d, number, {timeA}) AS Date
    FROM master..spt_values
    WHERE type = 'P'
        AND DATEADD(d, number, {timeA}) <= {timeB}
        AND DATEPART(dw, DATEADD(d, number, {timeA})) NOT IN (1, 7)
        AND DATEADD(d, number, {timeA}) NOT IN ( {string.Join(",", times.Select(x => $"CONVERT(DATETIME,{x})"))} )
) AS [___NetWorkDays]
";
                    }
                case SqlType.MySql:
                    {
                        return $@"
SELECT COUNT(*)
FROM (
    SELECT DATE_ADD({timeA}, INTERVAL d DAY) AS Date
    FROM (
        SELECT (t4 + t3*10 + t2*100 + t1*1000) AS d
        FROM (SELECT 0 AS t1 UNION SELECT 1 UNION SELECT 2 UNION SELECT 3) AS a
            JOIN (SELECT 0 AS t2 UNION SELECT 1 UNION SELECT 2 UNION SELECT 3 UNION SELECT 4 UNION SELECT 5 UNION SELECT 6 UNION SELECT 7 UNION SELECT 8 UNION SELECT 9) AS b
            JOIN (SELECT 0 AS t3 UNION SELECT 1 UNION SELECT 2 UNION SELECT 3 UNION SELECT 4 UNION SELECT 5 UNION SELECT 6 UNION SELECT 7 UNION SELECT 8 UNION SELECT 9) AS c
            JOIN (SELECT 0 AS t4 UNION SELECT 1 UNION SELECT 2 UNION SELECT 3 UNION SELECT 4 UNION SELECT 5 UNION SELECT 6 UNION SELECT 7 UNION SELECT 8 UNION SELECT 9) AS d
    ) AS days
    WHERE DATE_ADD({timeA}, INTERVAL d DAY) <= {timeB}
        AND DAYOFWEEK(DATE_ADD({timeA}, INTERVAL d DAY)) NOT IN (1, 7)
        AND DATE_ADD({timeA}, INTERVAL d DAY) NOT IN ({string.Join(",", times)})
) AS `___NetWorkDays`
";
                    }
                case SqlType.SQLite:
                    {
                        return $@"
SELECT COUNT(*) FROM (
    SELECT DISTINCT strftime('%Y-%m-%d', date) AS date FROM (
        SELECT date({timeA}, '+' || (a.a + (10 * b.a) + (100 * c.a)) || ' days') AS date
        FROM (SELECT 0 AS a UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) AS a
        CROSS JOIN (SELECT 0 AS a UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) AS b
        CROSS JOIN (SELECT 0 AS a UNION ALL SELECT 1 UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8 UNION ALL SELECT 9) AS c
    ) WHERE date BETWEEN {timeA} AND {timeB} AND date NOT IN ({string.Join(",", times)})
)
";
                    }
                case SqlType.PostgreSql:
                    {
                        return $@"
SELECT COUNT(*)
FROM (
    SELECT d::DATE
    FROM generate_series({timeA}::DATE, {timeB}::DATE, '1 day'::INTERVAL) AS s(d)
    WHERE EXTRACT('isodow' FROM d) < 6
      AND d::DATE NOT IN ({string.Join(",", times)})
      AND d::DATE NOT IN (
          SELECT unnest(ARRAY[{string.Join(",", times)}]::DATE[])
      )
) AS ""___NetWorkDays""
";
                    }
                default:
                    return null;
            }
        }
        public string Now()
        {
            switch (SqlType)
            {
                case SqlType.SQLite:
                    return "strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')";
                case SqlType.SqlServer:
                    return "GETDATE()";
                case SqlType.PostgreSql:
                    return "CURRENT_TIMESTAMP";
                case SqlType.MySql:
                default:
                    return "NOW()";
            }
        }
        public string ToDay()
        {
            switch (SqlType)
            {
                case SqlType.SQLite:
                    return "strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')";
                case SqlType.SqlServer:
                    return "GETDATE()";
                case SqlType.PostgreSql:
                    return "CURRENT_TIMESTAMP";
                case SqlType.MySql:
                default:
                    return "NOW()";
            }
        }
        public string? Days(string timeA, string timeB)
        {
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return $"DATEDIFF(DAY, {timeA},{timeB})";
                case SqlType.MySql:
                    return $"DATEDIFF({timeA}, {timeB})";
                case SqlType.SQLite:
                    return $"JULIANDAY({timeA}) - JULIANDAY({timeB})";
                case SqlType.PostgreSql:
                    return $"DATE_PART('day', {timeA}::timestamp - {timeB}::timestamp)";
                default:
                    return null;
            }
        }
        public string? Weakday(string time, string returnType)
        {
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer://@@DATEFIRST = ?
                    return $@" 
    CASE {returnType} 
        WHEN 1 THEN DATEPART(WEEKDAY, {time})
        WHEN 2 THEN DATEPART(WEEKDAY, {time}) - 1
        WHEN 3 THEN (DATEPART(WEEKDAY, {time}) + 1 - 2) % 7 + 1
        WHEN 11 THEN (DATEPART(WEEKDAY, {time}) + 1 - 1) % 7
        ELSE NULL
    END
";
                case SqlType.MySql:
                    return $@"
    CASE {returnType} 
        WHEN 1 THEN DAYOFWEEK({time})
        WHEN 2 THEN DAYOFWEEK({time}) - 1
        WHEN 3 THEN CASE DAYOFWEEK({time})
                        WHEN 1 THEN 7
                        ELSE DAYOFWEEK({time}) - 1
                    END
        WHEN 11 THEN CASE DAYOFWEEK({time})
                        WHEN 1 THEN 6
                        ELSE DAYOFWEEK({time}) - 2
                    END
        ELSE NULL
    END
";
                case SqlType.SQLite:
                    return $@"
SELECT
    CASE {returnType}
        WHEN 1 THEN CAST(strftime('%w', {time}, 'iso8601', 'weekday 1') AS INT)
        WHEN 2 THEN CAST(strftime('%w', {time}, 'iso8601', 'weekday 1') - 1 AS INT)
        WHEN 3 THEN CAST(CASE CAST(strftime('%w', {time}, 'iso8601', 'weekday 1') AS INT)
                          WHEN 0 THEN 7
                          ELSE CAST(strftime('%w', {time}, 'iso8601', 'weekday 1') AS INT)
                          END AS INT)
        WHEN 11 THEN CAST(CASE CAST(strftime('%w', {time}, 'iso8601', 'weekday 1') AS INT)
                          WHEN 0 THEN 6
                          ELSE CAST(strftime('%w', {time}, 'iso8601', 'weekday 1') - 1 AS INT)
                          END AS INT)
        ELSE NULL
    END
";
                case SqlType.PostgreSql:
                    return $@"
SELECT 
    CASE {returnType}
        WHEN 1 THEN EXTRACT(DOW FROM TIMESTAMP {time}::timestamp)
        WHEN 2 THEN EXTRACT(DOW FROM TIMESTAMP {time}::timestamp) - 1
        WHEN 3 THEN ((EXTRACT(DOW FROM TIMESTAMP {time}::timestamp) + 6) % 7) + 1
        WHEN 11 THEN (EXTRACT(DOW FROM TIMESTAMP {time}::timestamp) + 6) % 7
        ELSE NULL
    END
";
                default:
                    return null;
            }
        }
        public string? WeakNum(string time)
        {
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return $"DATEPART(WEEK, {time})";
                case SqlType.MySql:
                    return $"WEEK({time})";
                case SqlType.SQLite:
                    return $"STRFTIME('%W', {time})";
                case SqlType.PostgreSql:
                    return $"EXTRACT(WEEK FROM DATE {time}::timestamp)";
                default:
                    return null;
            }
        }
        public string? WorkDay(string time, string days, IEnumerable<string> holidays)
        {
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return $@"
SELECT COUNT(*)
FROM(
SELECT TOP ({days})
    DATEADD(day, number, {time}) AS Workday
FROM master..spt_values
WHERE type = 'P'
  AND DATEADD(day, number, {time}) NOT IN ({string.Join(",", holidays)})
  AND DATEPART(WEEKDAY, DATEADD(day, number, {time})) NOT IN (1, 7)
) AS [___WeakNum]";
                case SqlType.MySql:
                    return $"WEEK({time})";
                case SqlType.SQLite:
                    return $"STRFTIME('%W', {time})";
                case SqlType.PostgreSql:
                    return $"EXTRACT(WEEK FROM DATE {time}::timestamp)";
                default:
                    return null;
            }
        }
    }
}
