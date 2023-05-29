using Ao.Stock.Querying;
using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public class MergeHelper
    {
        public static string Wrap(SqlType sqlType,string field)
        {
            return GetMethodWrapper(sqlType).Quto(field);
        }
        public static string? WrapValue<T>(SqlType sqlType, T value)
        {
            return GetMethodWrapper(sqlType).WrapValue(value);
        }
        public static IMethodWrapper GetMethodWrapper(SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.MySql:
                    return DefaultMethodWrapper.MySql;
                case SqlType.SQLite:
                    return DefaultMethodWrapper.Sqlite;
                case SqlType.SqlServer:
                    return DefaultMethodWrapper.SqlServer;
                case SqlType.PostgreSql:
                    return DefaultMethodWrapper.PostgreSql;
                default:
                    throw new NotSupportedException(sqlType.ToString());
            }
        }
        public MergeHelper(SqlType sqlType, IMethodWrapper methodWrapper)
        {
            SqlType = sqlType;
            MethodWrapper = methodWrapper;
        }

        public MergeHelper(SqlType sqlType)
            : this(sqlType, GetMethodWrapper(sqlType))
        {
        }

        public SqlType SqlType { get; }

        public IMethodWrapper MethodWrapper { get; }

        public bool UseDerivedExists { get; set; }

        public IEnumerable<WhereItem>? WhereItems { get; set; }

        private string GetFormatter(SourceTableColumnDefine x)
        {
            var sourceField = DefaultDateTimePartNames.GetField(x.Method, x.Field, out var sourceOk);
            if (sourceOk)
            {
                return $"{Wrap("a")}.{Wrap(sourceField)} = {Wrap("b")}.{Wrap(x.Field)}";
            }
            var refSource = GetFormatter($"{Wrap("b")}.{Wrap(x.Field)}", x.Method);
            var refDest = GetFormatter($"{Wrap("a")}.{Wrap(x.Field)}", x.Method);
            return $"{refSource} = {refDest}";
        }
        private string GetFormatterSelect(SourceTableColumnDefine x)
        {
            if (x.ExpandDateTime)
            {
                var sourceField = DefaultDateTimePartNames.GetField(x.Method, x.Field, out var sourceOk);
                if (sourceOk)
                {
                    return $"{Wrap("a")}.{Wrap(sourceField)}";
                }
            }
            return x.Raw;
        }
        private string GetTableRef(SourceTableDefine sourceTableDefine, CompileOptions? options = null)
        {
            if (options != null && options.IncludeEffectJoin && options.EffectTable != null)
            {
                if (SqlType == SqlType.SqlServer || SqlType == SqlType.PostgreSql || SqlType == SqlType.SQLite)
                {
                    return $@"{Wrap(sourceTableDefine.Table)} AS {Wrap("a")} WHERE EXISTS( SELECT 1 FROM {Wrap(options.EffectTable)} AS {Wrap("b")} WHERE {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => GetFormatter(x)))})";
                }
                return $@"{Wrap(options.EffectTable)} AS {Wrap("b")} INNER JOIN {Wrap(sourceTableDefine.Table)} AS {Wrap("a")} ON {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => GetFormatter(x)))}";
            }
            return $"{Wrap(sourceTableDefine.Table)} AS {Wrap("a")}";
        }
        public string CompileUpdateSelect(string destTable, SourceTableDefine sourceTableDefine, CompileOptions? options = null)
        {
            var str = $"SELECT {Wrap("a")}.* ";
            if (SqlType == SqlType.MySql)
            {
                str += $"FROM {Wrap(destTable)} AS {Wrap("a")} ";
            }
            str += CompileUpdateSelectCore(destTable, sourceTableDefine, options);
            return str;
        }
        public string CompileUpdateInnerSelect(string destTable, SourceTableDefine sourceTableDefine, CompileOptions? options = null)
        {
            if (options!=null&&options.UseView)
            {
                return $"\nSELECT * FROM {Wrap(options.GetUpdateViewName(destTable, sourceTableDefine.Table))}\n";
            }
            var fromTable = GetTableRef(sourceTableDefine, options);
            if (SqlType == SqlType.SqlServer)
            {
                return $@"
		SELECT
            {string.Join(",\n", sourceTableDefine.Columns.Select(x => $"{GetFormatterSelect(x)} AS {Wrap(x.DestColumn.Field)}"))} 
		FROM {fromTable}
        {(WhereItems == null || !WhereItems.Any() ? string.Empty : "WHERE " + string.Join(" AND ", WhereItems.Select(x => $"{x.Raw} = {x.Value}")))}
		GROUP BY
            {string.Join(",\n", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => GetFormatterSelect(x)))}
";
            }
            else if (SqlType == SqlType.PostgreSql)
            {
                return $@"
SELECT
    {string.Join(",\n", sourceTableDefine.Columns.Select(x => $"{GetFormatterSelect(x)} AS \"{x.DestColumn.Field}\""))}
FROM {fromTable}
{(WhereItems == null || !WhereItems.Any() ? string.Empty : "WHERE " + string.Join(" AND ", WhereItems.Select(x => $"{x.Raw} = {x.Value}")))}
GROUP BY {string.Join(",\n", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => GetFormatterSelect(x)))}
";
            }
            else if (SqlType == SqlType.SQLite)
            {
                return $@"
SELECT
    {string.Join(",\n", sourceTableDefine.Columns.Select(x => $"{GetFormatterSelect(x)} AS \"{x.DestColumn.Field}\""))}
FROM {fromTable}
{(WhereItems == null || !WhereItems.Any() ? string.Empty : "WHERE " + string.Join(" AND ", WhereItems.Select(x => $"{x.Raw} = {x.Value}")))}
GROUP BY
    {string.Join(",\n", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => GetFormatterSelect(x)))}
";
            }
            return $@"
SELECT
    {string.Join(",\n", sourceTableDefine.Columns.Select(x => $"{GetFormatterSelect(x)} AS {Wrap(x.DestColumn.Field)}"))}
FROM {fromTable}
{(WhereItems == null || !WhereItems.Any() ? string.Empty : "WHERE " + string.Join(" AND ", WhereItems.Select(x => $"{x.Raw} = {x.Value}")))}
GROUP BY
    {string.Join(",\n", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => GetFormatterSelect(x)))}
";
        }
        public string CompileUpdateSelectCore(string destTable, SourceTableDefine sourceTableDefine, CompileOptions? options = null)
        {
            var fromTable = GetTableRef(sourceTableDefine, options);
            if (SqlType == SqlType.SqlServer)
            {
                return $@"
FROM
	{Wrap(destTable)} AS {Wrap("a")}
	INNER JOIN (
		{CompileUpdateInnerSelect(destTable,sourceTableDefine,options)}
	) AS  {Wrap("tmp")} ON {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => $" {Wrap("a")}.{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
AND(
    {string.Join(" OR ", sourceTableDefine.Columns.Where(x => !x.IsGroup && !x.OnlySet).Select(x => $" {Wrap("a")}.{Wrap(x.DestColumn.Field)} != {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
)
";
            }
            else if (SqlType == SqlType.PostgreSql)
            {
                string GetConvertMethod(ToRawMethod method)
                {
                    switch (method)
                    {
                        case ToRawMethod.Count:
                        case ToRawMethod.DistinctCount:
                        case ToRawMethod.Sum:
                        case ToRawMethod.Avg:
                            return "::bigint";
                        default:
                            return string.Empty;
                    }
                }
                return $@"
	FROM (
		{CompileUpdateInnerSelect(destTable, sourceTableDefine, options)}

) AS {Wrap("tmp")}
    WHERE {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => $"{Wrap("a")}.{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
AND(
    {string.Join(" OR ", sourceTableDefine.Columns.Where(x => !x.IsGroup && !x.OnlySet).Select(x => $" {Wrap("a")}.{Wrap(x.DestColumn.Field)}{GetConvertMethod(x.Method)} != {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}{GetConvertMethod(x.Method)}"))}
)
";
            }
            else if (SqlType == SqlType.SQLite)
            {
                return $@"
FROM (
		{CompileUpdateInnerSelect(destTable, sourceTableDefine, options)}
    ) AS {Wrap("tmp")}
    WHERE {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => $"\"{destTable}\".{Wrap(x.DestColumn.Field)} ={Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
AND(
    {string.Join(" OR ", sourceTableDefine.Columns.Where(x => !x.IsGroup && !x.OnlySet).Select(x => $"{Wrap(destTable)}.{Wrap(x.DestColumn.Field)} != {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
)
";
            }
            return $@"
	INNER JOIN (
		{CompileUpdateInnerSelect(destTable, sourceTableDefine, options)}
	) AS {Wrap("tmp")} ON {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup && !x.OnlySet).Select(x => $"{Wrap("a")}.{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
AND(
    {string.Join(" OR ", sourceTableDefine.Columns.Where(x => !x.IsGroup && !x.OnlySet).Select(x => $"{Wrap("a")}.{Wrap(x.DestColumn.Field)} != {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
)
";
        }
        public virtual string CompileUpdate(string destTable, SourceTableDefine sourceTableDefine, CompileOptions? options = null)
        {
            var noLockSql = options?.NoLock ?? false ? GetNoLockSql() : string.Empty;
            var noLockRestoreSql = options?.NoLock ?? false ? GetNoLockRestoreSql() : string.Empty;
            var fromTable = GetTableRef(sourceTableDefine, options);
            if (SqlType == SqlType.SqlServer)
            {
                return $@"
{noLockSql}
UPDATE
	{Wrap(destTable)}
SET
    {string.Join(",\n", sourceTableDefine.Columns.Where(x => !x.IsGroup).Select(x => $@"{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
{CompileUpdateSelectCore(destTable, sourceTableDefine, options)};
{noLockRestoreSql}
";
            }
            else if (SqlType == SqlType.PostgreSql)
            {
                return $@"
{noLockSql}
UPDATE
	{Wrap(destTable)} AS {Wrap("a")}
SET
    {string.Join(",\n", sourceTableDefine.Columns.Where(x => !x.IsGroup && !x.OnlySet).Select(x => $@"{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
{CompileUpdateSelectCore(destTable, sourceTableDefine, options)};
{noLockRestoreSql}
";
            }
            else if (SqlType == SqlType.SQLite)
            {
                return $@"
{noLockSql}
UPDATE
    {Wrap(destTable)}
SET
    {string.Join(",\n", sourceTableDefine.Columns.Where(x => !x.IsGroup).Select(x => $@"{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.""{x.DestColumn.Field}"""))}
{CompileUpdateSelectCore(destTable, sourceTableDefine, options)};
{noLockRestoreSql}
";
            }
            return $@"
{noLockSql}
UPDATE {Wrap(destTable)} AS {Wrap("a")}
{CompileUpdateSelectCore(destTable, sourceTableDefine, options)}
SET
    {string.Join(",\n", sourceTableDefine.Columns.Where(x => !x.IsGroup).Select(x => $@"{Wrap("a")}.{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))};
{noLockRestoreSql}
";
        }
        public string GetNoLockRestoreSql()
        {
            switch (SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    return "SET TRANSACTION ISOLATION LEVEL READ COMMITTED;\n";
                case SqlType.MySql:
                    return "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;\n";
                case SqlType.SQLite:
                    return "PRAGMA read_uncommitted = 0;\n";
                case SqlType.PostgreSql:
                    return "SET SESSION CHARACTERISTICS AS TRANSACTION ISOLATION LEVEL READ COMMITTED;\n";
                case SqlType.Oracle:
                case SqlType.Db2:
                default:
                    return string.Empty;
            }
        }
        public string GetNoLockSql()
        {
            switch (SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    return "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;\n";
                case SqlType.MySql:
                    return "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;\n";
                case SqlType.SQLite:
                    return "PRAGMA read_uncommitted = 1;\n";
                case SqlType.PostgreSql:
                    return "SET SESSION CHARACTERISTICS AS TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;\n";
                case SqlType.Oracle:
                case SqlType.Db2:
                default:
                    return string.Empty;
            }
        }
        public string CompileInsertSelect(string destTable, SourceTableDefine sourceTableDefine, CompileOptions? options = null)
        {
            var str = $"SELECT {string.Join(",", sourceTableDefine.Columns.Select(x => $"{GetFormatterSelect(x)} AS {Wrap(x.DestColumn.Field)}"))}\n";
            var optSqlite = SqlType == SqlType.SQLite && sourceTableDefine.Columns.Any(x => x.IsGroup && x.Method == ToRawMethod.None);
            if (optSqlite)
            {
                str += "FROM (SELECT * ";
            }
            str += $"FROM {GetTableRef(sourceTableDefine, options)}\n";
            if (optSqlite)
            {
                str += $@") AS {Wrap("a")} LEFT JOIN {Wrap(destTable)} AS {Wrap("c")} ON {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => $"{x.Raw} = {Wrap("c")}.{Wrap(x.DestColumn.Field)}"))}
                          WHERE {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup && x.Method == ToRawMethod.None).Select(x => $"{Wrap("c")}.{Wrap(x.DestColumn.Field)} IS NULL"))}
";
            }
            else
            {
                str += @$"{((SqlType == SqlType.SqlServer || SqlType == SqlType.PostgreSql || SqlType == SqlType.SQLite) && (options?.IncludeEffectJoin ?? false) ? "AND" : "WHERE")} {(WhereItems == null || !WhereItems.Any() ? string.Empty : ("(" + string.Join(" AND ", WhereItems.Select(x => $"{x.Raw} = {x.Value}")) + ")"))} {(WhereItems == null || !WhereItems.Any() ? string.Empty : "AND")}";
                str += @$"
                NOT EXISTS(
                    SELECT 1 AS {Wrap("tmp")} 
                    FROM {Wrap(destTable)} AS {Wrap("c")}
                    WHERE {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => $"{GetFormatterSelect(x)} = {Wrap("c")}.{Wrap(x.DestColumn.Field)}"))}
                )";
            }
            str += $"GROUP BY {string.Join(",", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => GetFormatterSelect(x)))};\n";
            return str;
        }
        public virtual string CompileInsert(string destTable, SourceTableDefine sourceTableDefine, CompileOptions? options = null)
        {
            var str = $"INSERT INTO {Wrap(destTable)}({string.Join(", ", sourceTableDefine.Columns.Select(x => Wrap(x.DestColumn.Field)))})\n";
            if (options != null && options.UseView)
            {
                str += $"SELECT * FROM {Wrap(options.GetInsertViewName(destTable, sourceTableDefine.Table))}\n";
            }
            else
            {
                str += CompileInsertSelect(destTable, sourceTableDefine, options);
            }
            str += options?.NoLock ?? false ? GetNoLockRestoreSql() : string.Empty;
            return str;
        }
        public string JoinString(string left, string right)
        {
            if (SqlType == SqlType.SQLite || SqlType == SqlType.PostgreSql)
            {
                return $"{left} || {right}";
            }
            if (SqlType == SqlType.SqlServer)
            {
                return $"{left} + {right}";
            }
            return $"CONCAT({left},{right})";
        }
        public string GetFormatter(string @ref,ToRawMethod method)
        {
            switch (method)
            {
                case ToRawMethod.Year:
                    return GetYearFormatter(@ref);
                case ToRawMethod.Month:
                    return GetMonthFormatter(@ref);
                case ToRawMethod.Day:
                    return GetDayFormatter(@ref);
                case ToRawMethod.Hour:
                    return GetHourFormatter(@ref);
                case ToRawMethod.Minute:
                    return GetMinuteFormatter(@ref);
                case ToRawMethod.Week:
                    return GetWeekFormatter(@ref);
                case ToRawMethod.Quarter:
                    return GetQuarterFormatter(@ref);
                default: 
                    return @ref;
            }
        }
        public string GetQuarterFormatter(string @ref)
        {
            if (SqlType == SqlType.SQLite)
            {
                return $@"
    STRFTIME('%Y', {@ref})||'-'||(CASE 
        WHEN COALESCE(NULLIF((SUBSTR({@ref}, 4, 2) - 1) / 3, 0), 4) < 10 
        THEN '0' || COALESCE(NULLIF((SUBSTR({@ref}, 4, 2) - 1) / 3, 0), 4)
        ELSE COALESCE(NULLIF((SUBSTR({@ref}, 4, 2) - 1) / 3, 0), 4)
    END)||'-01 00:00:00'
";
            }
            else if (SqlType == SqlType.SqlServer)
            {
                return $"DATEADD(qq, DATEDIFF(qq, 0, {@ref}), 0)";
            }
            else if (SqlType == SqlType.PostgreSql)
            {
                return $"date_trunc('quarter', {@ref}::TIMESTAMP)";
            }
            return $"CONCAT(DATE_FORMAT(LAST_DAY(MAKEDATE(EXTRACT(YEAR FROM {@ref}),1) + interval QUARTER({@ref})*3-3 month),'%Y-%m-'),'01')";
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
        public string GetWeekFormatter(string @ref)
        {
            if (SqlType == SqlType.SQLite)
            {
                return $"date({@ref}, 'weekday 0', '-6 day')||' 00:00:00'";
            }
            else if (SqlType == SqlType.SqlServer)
            {
                return $"DATEADD(WEEK, DATEDIFF(WEEK, 0, CONVERT(DATETIME, {@ref}, 120) - 1), 0)";
            }
            else if (SqlType == SqlType.PostgreSql)
            {
                return $"date_trunc('day',{@ref}::timestamp) - ((EXTRACT(DOW FROM {@ref}::TIMESTAMP)::INTEGER+6)%7 || ' days')::INTERVAL";
            }
            return $"DATE_FORMAT(DATE_SUB({@ref}, INTERVAL WEEKDAY({@ref}) DAY),'%Y-%m-%d')";

#if false
SELECT DATE_FORMAT(DATE_SUB(NOW(), INTERVAL WEEKDAY(NOW()) DAY),'%Y-%m-%d')--mysql
SELECT DATEADD(WEEK, DATEDIFF(WEEK, 0, CONVERT(DATETIME, '2023-04-24', 120) - 1), 0);--sqlserver
SELECT date('now', 'weekday 0', '-6 day');--sqlite
SELECT '2022-01-30 00:00:00'::timestamp - ((EXTRACT(DOW FROM '2022-01-30 00:00:00'::TIMESTAMP)::INTEGER+6)%7 || ' days')::INTERVAL;--pgsql
#endif
        }
        public string GetYearFormatter(string @ref)
        {
            if (SqlType == SqlType.SqlServer)
            {
                return $"CONVERT(VARCHAR(4),{@ref} ,120)";
            }
            else if (SqlType == SqlType.SQLite)
            {
                return $"strftime('%Y-01-01 00:00:00', {@ref})";
            }
            else if (SqlType== SqlType.PostgreSql)
            {
                return $"date_trunc('year',{@ref})";
            }
            return $"LEFT({@ref},4)";
        }
        public string GetMonthFormatter(string @ref)
        {
            if (SqlType == SqlType.SqlServer)
            {
                return $"CONVERT(VARCHAR(7),{@ref} ,120)";
            }
            else if (SqlType == SqlType.SQLite)
            {
                return $"strftime('%Y-%m-01 00:00:00', {@ref})";
            }
            else if (SqlType == SqlType.PostgreSql)
            {
                return $"date_trunc('month',{@ref})";
            }
            else
            {
                return $"LEFT({@ref},7)";
            }
        }
        public string GetDayFormatter(string @ref)
        {
            if (SqlType == SqlType.SqlServer)
            {
                return $"CONVERT(VARCHAR(10),{@ref} ,120)";
            }
            else if (SqlType == SqlType.SQLite)
            {
                return $"strftime('%Y-%m-%d 00:00:00', {@ref})";
            }
            else if (SqlType == SqlType.PostgreSql)
            {
                return $"date_trunc('day',{@ref})";
            }
            else
            {
                return $"LEFT({@ref},10)";
            }
        }
        public string GetHourFormatter(string @ref)
        {
            if (SqlType == SqlType.SqlServer)
            {
                return $"CONVERT(VARCHAR(13),{@ref} ,120)";
            }
            else if (SqlType == SqlType.SQLite)
            {
                return $"strftime('%Y-%m-%d %H:00:00', {@ref})";
            }
            else if (SqlType == SqlType.PostgreSql)
            {
                return $"date_trunc('hour',{@ref})";
            }
            else
            {
                return $"LEFT({@ref},13)";
            }
        }
        public string GetMinuteFormatter(string @ref)
        {
            if (SqlType == SqlType.SqlServer)
            {
                return $"CONVERT(VARCHAR(16),{@ref} ,120)";
            }
            else if (SqlType == SqlType.SQLite)
            {
                return $"strftime('%Y-%m-%d %H:%M:00', {@ref})";
            }
            else if (SqlType == SqlType.PostgreSql)
            {
                return $"date_trunc('minute',{@ref})";
            }
            else
            {
                return $"LEFT({@ref},16)";
            }
        }
        protected string GetFormatString(ToRawMethod method, string fieldName, bool quto)
        {
            var @ref = GetRef(fieldName, quto);
            switch (method)
            {
                case ToRawMethod.Year:
                    {
                        var forMatter = GetYearFormatter(@ref);
                        if (SqlType == SqlType.PostgreSql|| SqlType == SqlType.SQLite)
                        {
                            return forMatter;
                        }
                        return JoinString(forMatter, "'-01-01 00:00:00'");
                    }
                case ToRawMethod.Day:
                    {
                        var forMatter=GetDayFormatter(@ref);
                        if (SqlType== SqlType.SQLite||SqlType== SqlType.PostgreSql)
                        {
                            return forMatter;
                        }
                        return JoinString(forMatter, "' 00:00:00'");
                    }
                case ToRawMethod.Hour:
                    {
                        var forMatter= GetHourFormatter(@ref);
                        if (SqlType == SqlType.PostgreSql||SqlType == SqlType.SQLite)
                        {
                            return forMatter;
                        }
                        return JoinString(forMatter, "':00:00'");
                    }
                case ToRawMethod.Minute:
                    {
                        var forMatter= GetMinuteFormatter(@ref);
                        if (SqlType == SqlType.PostgreSql || SqlType == SqlType.SQLite)
                        {
                            return forMatter;
                        }
                        return JoinString(forMatter, "':00'");
                    }
                case ToRawMethod.Second:
                    return @ref;
                case ToRawMethod.Month:
                    {
                        var forMatter=GetMonthFormatter(@ref);
                        if (SqlType == SqlType.PostgreSql || SqlType == SqlType.SQLite)
                        {
                            return forMatter;
                        }
                        return JoinString(forMatter, "'-01 00:00:00'");
                    }
                default:
                    throw new NotSupportedException(method.ToString());
            }
        }

        public string GetRef(string field, bool quto)
        {
            return quto ? MethodWrapper.Quto(field) : field;
        }
        public virtual string ToRaw(ToRawMethod method, string field, bool quto)
        {
            switch (method)
            {
                case ToRawMethod.Now:
                    return new FunctionMapper(SqlType).Now();
                case ToRawMethod.Min:
                    return $"MIN({GetRef(field, quto)})";
                case ToRawMethod.Max:
                    return $"MAX({GetRef(field, quto)})";
                case ToRawMethod.Count:
                    return $"COUNT({GetRef(field, quto)})";
                case ToRawMethod.DistinctCount:
                    return $"COUNT(DISTINCT {GetRef(field, quto)})";
                case ToRawMethod.Sum:
                    return $"SUM({GetRef(field, quto)})";
                case ToRawMethod.Avg:
                    return $"AVG({GetRef(field, quto)})";
                case ToRawMethod.Year:
                case ToRawMethod.Month:
                case ToRawMethod.Day:
                case ToRawMethod.Hour:
                case ToRawMethod.Minute:
                case ToRawMethod.Second:
                    return GetFormatString(method, field, quto);
                case ToRawMethod.Quarter:
                    {
                        return GetQuarterFormatter(GetRef(field, quto));
                    }
                case ToRawMethod.Week:
                    {
                        return GetWeekFormatter(GetRef(field, quto));
                    }
                default:
                    return quto ? MethodWrapper.WrapValue(field)! : field;
            }
        }
        public virtual string Wrap(string obj)
        {
            return MethodWrapper.Quto(obj);
        }
    }
}
