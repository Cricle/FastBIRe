using Ao.Stock.Querying;
using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public class MergeHelper
    {
        public MergeHelper(SqlType sqlType, IMethodWrapper methodWrapper)
        {
            SqlType = sqlType;
            MethodWrapper = methodWrapper;
        }

        public MergeHelper(SqlType sqlType)
            : this(sqlType, sqlType.GetMethodWrapper())
        {
        }

        public SqlType SqlType { get; }

        public IMethodWrapper MethodWrapper { get; }

        public bool UseDerivedExists { get; set; }

        public IEnumerable<WhereItem>? WhereItems { get; set; }

        private string GetFormatter(SourceTableColumnDefine x)
        {
            if (DateTimePartNames.Default.TryGetField(x.Method, x.Field, out var sourceField))
            {
                return $"{Wrap("a")}.{Wrap(sourceField)} = {Wrap("b")}.{Wrap(sourceField)}";
            }
            var refSource = GetFormatter($"{Wrap("b")}.{Wrap(x.Field)}", x.Method);
            var refDest = GetFormatter($"{Wrap("a")}.{Wrap(x.Field)}", x.Method);
            return $"{refSource} = {refDest}";
        }
        private string GetFormatterSelect(SourceTableColumnDefine x)
        {
            if (x.ExpandDateTime)
            {
                if (DateTimePartNames.Default.TryGetField(x.Method, x.Field, out var sourceField))
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
            if (options != null && options.UseView)
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
		{CompileUpdateInnerSelect(destTable, sourceTableDefine, options)}
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
            var fromTable = GetTableRef(sourceTableDefine, options);
            if (SqlType == SqlType.SqlServer)
            {
                return $@"
UPDATE
	{Wrap(destTable)}
SET
    {string.Join(",\n", sourceTableDefine.Columns.Where(x => !x.IsGroup).Select(x => $@"{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
{CompileUpdateSelectCore(destTable, sourceTableDefine, options)};
";
            }
            else if (SqlType == SqlType.PostgreSql)
            {
                return $@"
UPDATE
	{Wrap(destTable)} AS {Wrap("a")}
SET
    {string.Join(",\n", sourceTableDefine.Columns.Where(x => !x.IsGroup && !x.OnlySet).Select(x => $@"{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
{CompileUpdateSelectCore(destTable, sourceTableDefine, options)};
";
            }
            else if (SqlType == SqlType.SQLite)
            {
                return $@"
UPDATE
    {Wrap(destTable)}
SET
    {string.Join(",\n", sourceTableDefine.Columns.Where(x => !x.IsGroup).Select(x => $@"{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.""{x.DestColumn.Field}"""))}
{CompileUpdateSelectCore(destTable, sourceTableDefine, options)};
";
            }
            return $@"
UPDATE {Wrap(destTable)} AS {Wrap("a")}
{CompileUpdateSelectCore(destTable, sourceTableDefine, options)}
SET
    {string.Join(",\n", sourceTableDefine.Columns.Where(x => !x.IsGroup).Select(x => $@"{Wrap("a")}.{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))};
";
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
            return str;
        }
        public string JoinString(string left, string right)
        {
            return new FunctionMapper(SqlType).Concatenate(left, right);
        }
        public string? GetFormatter(string @ref, ToRawMethod method)
        {
            var mapper = new FunctionMapper(SqlType);
            switch (method)
            {
                case ToRawMethod.Year:
                    return mapper.YearFull(@ref);
                case ToRawMethod.Month:
                    return mapper.MonthFull(@ref);
                case ToRawMethod.Day:
                    return mapper.DayFull(@ref);
                case ToRawMethod.Hour:
                    return mapper.HourFull(@ref);
                case ToRawMethod.Minute:
                    return mapper.MinuteFull(@ref);
                case ToRawMethod.Week:
                    return mapper.WeekFull(@ref);
                case ToRawMethod.Quarter:
                    return mapper.QuarterFull(@ref);
                default:
                    return @ref;
            }
        }

        public string GetRef(string field, bool quto)
        {
            return quto ? MethodWrapper.Quto(field) : field;
        }
        public virtual string? ToRaw(ToRawMethod method, string field, bool quto)
        {
            var mapper = new FunctionMapper(SqlType);
            switch (method)
            {
                case ToRawMethod.Now:
                    return mapper.Now();
                case ToRawMethod.Min:
                    return mapper.MinC(GetRef(field, quto));
                case ToRawMethod.Max:
                    return mapper.MaxC(GetRef(field, quto));
                case ToRawMethod.Count:
                    return mapper.CountC(GetRef(field, quto));
                case ToRawMethod.DistinctCount:
                    return mapper.DistinctCountC(GetRef(field, quto));
                case ToRawMethod.Sum:
                    return mapper.SumC(GetRef(field, quto));
                case ToRawMethod.Avg:
                    return mapper.AverageC(GetRef(field, quto));
                case ToRawMethod.Year:
                case ToRawMethod.Month:
                case ToRawMethod.Day:
                case ToRawMethod.Hour:
                case ToRawMethod.Minute:
                case ToRawMethod.Second:
                    return GetFormatter(GetRef(field, quto), method);
                case ToRawMethod.Quarter:
                    {
                        return GetFormatter(GetRef(field, quto), ToRawMethod.Quarter);
                    }
                case ToRawMethod.Week:
                    {
                        return GetFormatter(GetRef(field, quto), ToRawMethod.Week);
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
