using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public class TimeDataMigrationHelper
    {
        public static string? Sql(MergeHelper helper, string name, ToRawMethod type)
        {
            switch (helper.SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    return SqlServer(helper, name, type);
                case SqlType.MySql:
                    return MySql(helper, name, type);
                case SqlType.SQLite:
                    return Sqlite(helper, name, type);
                case SqlType.PostgreSql:
                    return Postgresql(helper, name, type);
                default:
                    return null;
            }
        }
        public static string? GetMissDatePart(ToRawMethod type)
        {
            switch (type)
            {
                case ToRawMethod.Year:
                    return "'-01-01 00:00:00'";
                case ToRawMethod.Month:
                    return "'-01 00:00:00'";
                case ToRawMethod.Day:
                    return "' 00:00:00'";
                case ToRawMethod.Hour:
                    return "':00:00'";
                case ToRawMethod.Minute:
                    return "':00'";
                default:
                    return null;
            }
        }
        public static string NormalSupplement(MergeHelper helper, string name, string miss)
        {
            return $"LEFT({helper.JoinString(name, miss)},19)";
        }
        public static string? Postgresql(MergeHelper helper, string name, ToRawMethod type)
        {
            switch (type)
            {
                case ToRawMethod.Year:
                case ToRawMethod.Month:
                case ToRawMethod.Day:
                case ToRawMethod.Hour:
                case ToRawMethod.Minute:
                    return NormalSupplement(helper, name, GetMissDatePart(type)!);
                case ToRawMethod.Quarter:
                    return $@"
  LEFT({name},4)||
    '-'||
    CASE RIGHT({name}, 2)
      WHEN '01' THEN '01'
      WHEN '02' THEN '04'
      WHEN '03' THEN '07'
      WHEN '04' THEN '10'
    END||
    '-01'";
                case ToRawMethod.Week:
                    return $"(date_trunc('week', to_date('2022-01', 'YYYY-W')) + interval '1 week')::TIMESTAMP";
                default:
                    return null;
            }
        }
        public static string? Sqlite(MergeHelper helper, string name, ToRawMethod type)
        {
            switch (type)
            {
                case ToRawMethod.Year:
                case ToRawMethod.Month:
                case ToRawMethod.Day:
                case ToRawMethod.Hour:
                case ToRawMethod.Minute:
                    return NormalSupplement(helper, name, GetMissDatePart(type)!);
                case ToRawMethod.Quarter:
                    return $@"
  SUBSTR({name}, 1, 4)||
    '-'||
    CASE SUBSTR({name}, 6)
      WHEN '01' THEN '01'
      WHEN '02' THEN '04'
      WHEN '03' THEN '07'
      WHEN '04' THEN '10'
    END||
    '-01'";
                case ToRawMethod.Week:
                    return $"date(substr({name},1,4)||'-01-01', 'weekday 1', '+'||((substr({name},6)-1)*7)||' day')";
                default:
                    return null;
            }
        }
        public static string? SqlServer(MergeHelper helper, string name, ToRawMethod type)
        {
            switch (type)
            {
                case ToRawMethod.Year:
                case ToRawMethod.Month:
                case ToRawMethod.Day:
                case ToRawMethod.Hour:
                case ToRawMethod.Minute:
                    return NormalSupplement(helper, name, GetMissDatePart(type)!);
                case ToRawMethod.Quarter:
                    return $@"
  LEFT({name},4)+
    '-'+
    CASE RIGHT({name},2)
      WHEN '01' THEN '01'
      WHEN '02' THEN '04'
      WHEN '03' THEN '07'
      WHEN '04' THEN '10'
    END+
    '-01';";
                case ToRawMethod.Week:
                    return $"DATEADD(WEEK, CAST(RIGHT({name}, 2) AS int), DATEADD(YEAR, CAST(LEFT({name}, 4) AS int) - 1900, 0)) - DATEPART(WEEKDAY, DATEADD(YEAR, CAST(LEFT({name}, 4) AS int) - 1900, -1)) + 1";
                default:
                    return null;
            }
        }
        public static string? MySql(MergeHelper helper, string name, ToRawMethod type)
        {
            switch (type)
            {
                case ToRawMethod.Year:
                case ToRawMethod.Month:
                case ToRawMethod.Day:
                case ToRawMethod.Hour:
                case ToRawMethod.Minute:
                    return NormalSupplement(helper, name, GetMissDatePart(type)!);
                case ToRawMethod.Quarter:
                    return $@"
  CONCAT(
    SUBSTRING({name}, 1, 4),
    '-',
    CASE SUBSTRING({name}, 6)
      WHEN '01' THEN '01'
      WHEN '02' THEN '04'
      WHEN '03' THEN '07'
      WHEN '04' THEN '10'
    END,
    '-01'
  )";
                case ToRawMethod.Week:
                    return $"STR_TO_DATE(CONCAT(SUBSTRING({name}, 1, 4 ), '-W', SUBSTRING( {name}, 6 ), '-1' ), '%X-W%V-%w' )";
                default:
                    return null;
            }
        }
    }
}
