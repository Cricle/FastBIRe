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

        //public string 

        public string DateDif(string timeA, string timeB, string unit)
        {
            return $@"
CASE WHEN {Ascii(unit)}={Ascii("'Y'")} THEN {Year(timeA)}-{Year(timeB)}
WHEN {Ascii(unit)}={Ascii("'M'")} THEN {Month(timeA)}-{Month(timeB)} 
WHEN {Ascii(unit)}={Ascii("'D'")} THEN {Day(timeA)}-{Day(timeB)}
WHEN {Ascii(unit)}={Ascii("'h'")} THEN {Hour(timeA)}-{Hour(timeB)}
WHEN {Ascii(unit)}={Ascii("'m'")} THEN {Minute(timeA)}-{Minute(timeB)}
WHEN {Ascii(unit)}={Ascii("'s'")} THEN {Second(timeA)}-{Second(timeB)}
END
";
        }
        public string? DateAdd(string time, string num, string unit)
        {
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return $"DATEADD({unit}, {num}, {time})";
                case SqlType.MySql:
                    return $"DATE_ADD({time}, INTERVAL {num} {unit})";
                case SqlType.SQLite:
                    return $"DATE({time}, '{num} {unit}')";
                case SqlType.PostgreSql:
                    return $"{time} + INTERVAL '{num} {unit}'";
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
            else if (SqlType == SqlType.SqlServer || SqlType == SqlType.SqlServerCe)
            {
                return $"DATEPART(day,{time})";
            }
            return $"DAY({time})";
        }
        public string DayTo(string time)
        {
            if (SqlType == SqlType.SqlServer)
            {
                return $"CONVERT(VARCHAR(10),{time} ,120)";
            }
            else if (SqlType == SqlType.SQLite)
            {
                return $"strftime('%Y-%m-%d 00:00:00', {time})";
            }
            else if (SqlType == SqlType.PostgreSql)
            {
                return $"date_trunc('day',{time})";
            }
            return $"LEFT({time},10)";
        }
        public string? DayFull(string time)
        {
            return Concatenate(DayTo(time), "' 00:00:00'");
        }
        public string HourTo(string time)
        {
            if (SqlType == SqlType.SqlServer)
            {
                return $"CONVERT(VARCHAR(13),{time} ,120)";
            }
            else if (SqlType == SqlType.SQLite)
            {
                return $"strftime('%Y-%m-%d %H:00:00', {time})";
            }
            else if (SqlType == SqlType.PostgreSql)
            {
                return $"date_trunc('hour',{time})";
            }
            return $"LEFT({time},13)";
        }
        public string? HourFull(string time)
        {
            return Concatenate(HourTo(time), "':00:00'");
        }
        public string MinuteTo(string time)
        {
            if (SqlType == SqlType.SqlServer)
            {
                return $"CONVERT(VARCHAR(16),{time} ,120)";
            }
            else if (SqlType == SqlType.SQLite)
            {
                return $"strftime('%Y-%m-%d %H:%M:00', {time})";
            }
            else if (SqlType == SqlType.PostgreSql)
            {
                return $"date_trunc('minute',{time})";
            }
            return $"LEFT({time},16)";
        }
        public string? MinuteFull(string time)
        {
            return Concatenate(MinuteTo(time), "':00'");
        }
        public string WeekTo(string time)
        {
            if (SqlType == SqlType.SQLite)
            {
                return $"date({time}, 'weekday 0', '-6 day')||' 00:00:00'";
            }
            else if (SqlType == SqlType.SqlServer)
            {
                return $"DATEADD(WEEK, DATEDIFF(WEEK, 0, CONVERT(DATETIME, {time}, 120) - 1), 0)";
            }
            else if (SqlType == SqlType.PostgreSql)
            {
                return $"date_trunc('day',{time}::timestamp) - ((EXTRACT(DOW FROM {time}::TIMESTAMP)::INTEGER+6)%7 || ' days')::INTERVAL";
            }
            return $"DATE_FORMAT(DATE_SUB({time}, INTERVAL WEEKDAY({time}) DAY),'%Y-%m-%d')";

#if false
SELECT DATE_FORMAT(DATE_SUB(NOW(), INTERVAL WEEKDAY(NOW()) DAY),'%Y-%m-%d')--mysql
SELECT DATEADD(WEEK, DATEDIFF(WEEK, 0, CONVERT(DATETIME, '2023-04-24', 120) - 1), 0);--sqlserver
SELECT date('now', 'weekday 0', '-6 day');--sqlite
SELECT '2022-01-30 00:00:00'::timestamp - ((EXTRACT(DOW FROM '2022-01-30 00:00:00'::TIMESTAMP)::INTEGER+6)%7 || ' days')::INTERVAL;--pgsql
#endif
        }
        public string? WeekFull(string time)
        {
            return Concatenate(WeekTo(time), "':00'");
        }
        public string QuarterFull(string time)
        {
            if (SqlType == SqlType.SQLite)
            {
                return $@"
    STRFTIME('%Y', {time})||'-'||(CASE 
        WHEN COALESCE(NULLIF((SUBSTR({time}, 4, 2) - 1) / 3, 0), 4) < 10 
        THEN '0' || COALESCE(NULLIF((SUBSTR({time}, 4, 2) - 1) / 3, 0), 4)
        ELSE COALESCE(NULLIF((SUBSTR({time}, 4, 2) - 1) / 3, 0), 4)
    END)||'-01 00:00:00'
";
            }
            else if (SqlType == SqlType.SqlServer)
            {
                return $"DATEADD(qq, DATEDIFF(qq, 0, {time}), 0)";
            }
            else if (SqlType == SqlType.PostgreSql)
            {
                return $"date_trunc('quarter', {time}::TIMESTAMP)";
            }
            return $"CONCAT(DATE_FORMAT(LAST_DAY(MAKEDATE(EXTRACT(YEAR FROM {time}),1) + interval QUARTER({time})*3-3 month),'%Y-%m-'),'01')";
#if false
select CONCAT(DATE_FORMAT(LAST_DAY(MAKEDATE(EXTRACT(YEAR FROM  '2023-12-24'),1) + interval QUARTER('2023-12-24')*3-3 month),'%Y-%m-'),'01'); --mysql
SELECT DATEADD(qq, DATEDIFF(qq, 0, '2023-010-23'), 0);--sqlserver
SELECT STRFTIME('%Y', '2023-04-24')||'-'||(CASE 
        WHEN COALESCE(NULLIF((SUBSTR('2023-10-24', 4, 2) - 1) / 3, 0), 4) < 10 
        THEN '0' || COALESCE(NULLIF((SUBSTR('2023-10-24', 4, 2) - 1) / 3, 0), 4)
        ELSE COALESCE(NULLIF((SUBSTR('2023-10-24', 4, 2) - 1) / 3, 0), 4)
    END)||'-01'; --sqlite
SELECT date_trunc('quarter', '2023-10-23'::TIMESTAMP);--pgsql
#endif
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
        public string? YearFull(string time)
        {
            return Concatenate(YearTo(time), "'-01-01 00:00:00'");
        }
        public string MonthTo(string time)
        {
            if (SqlType == SqlType.SqlServer)
            {
                return $"CONVERT(VARCHAR(7),{time} ,120)";
            }
            else if (SqlType == SqlType.SQLite)
            {
                return $"strftime('%Y-%m-01 00:00:00', {time})";
            }
            else if (SqlType == SqlType.PostgreSql)
            {
                return $"date_trunc('month',{time})";
            }
            return $"LEFT({time},7)";
        }
        public string? MonthFull(string time)
        {
            return Concatenate(MonthTo(time), "'-01 00:00:00'");
        }
        public string YearTo(string time)
        {
            if (SqlType == SqlType.SqlServer)
            {
                return $"CONVERT(VARCHAR(4),{time} ,120)";
            }
            else if (SqlType == SqlType.SQLite)
            {
                return $"strftime('%Y-01-01 00:00:00', {time})";
            }
            else if (SqlType == SqlType.PostgreSql)
            {
                return $"date_trunc('year',{time})";
            }
            return $"LEFT({time},4)";
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
        public string Quarter(string time)
        {
            switch (SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    return $"DATEPART(QUARTER, {time})";
                case SqlType.MySql:
                    return $"QUARTER({time})";
                case SqlType.SQLite:
                    return @$"CASE
    WHEN strftime('%m', {time}) BETWEEN '01' AND '03' THEN 1
    WHEN strftime('%m', {time}) BETWEEN '04' AND '06' THEN 2
    WHEN strftime('%m', {time}) BETWEEN '07' AND '09' THEN 3
    ELSE 4
    END";
                case SqlType.PostgreSql:
                    return $"EXTRACT(QUARTER FROM {time})";
                case SqlType.Db2:
                case SqlType.Oracle:
                default:
                    return string.Empty;
            }
        }
        public string Week(string time)
        {
            switch (SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    return $"DATEPART(WEEK, {time})";
                case SqlType.MySql:
                    return $"WEEK({time})";
                case SqlType.SQLite:
                    return $"strftime('%W', {time})";
                case SqlType.PostgreSql:
                    return $"EXTRACT(WEEK FROM {time})";
                case SqlType.Db2:
                case SqlType.Oracle:
                default:
                    return string.Empty;
            }
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
                    return "strftime('%Y-%m-%d 00:00:00', 'now', 'localtime')";
                case SqlType.SqlServer:
                    return "CONVERT(DATETIME,CONVERT(VARCHAR(10),GETDATE(),120)+' 00:00:00',120)";
                case SqlType.PostgreSql:
                    return "CAST(CURRENT_DATE||' 00:00:00' AS TIMESTAMP)";
                case SqlType.MySql:
                default:
                    return "CAST(CONCAT(DATE_FORMAT(NOW(),'%Y-%m-%d'),' 00:00:00') AS DATETIME)";
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
