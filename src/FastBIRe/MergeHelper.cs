using Ao.Stock.Querying;
using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public class MergeHelper
    {
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
        public static IMethodTranslator<object?> GetTranslator(SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.MySql:
                    return SqlMethodTranslatorHelpers<object?>.Mysql();
                case SqlType.SQLite:
                    return SqlMethodTranslatorHelpers<object?>.Sqlite();
                case SqlType.SqlServer:
                    return SqlMethodTranslatorHelpers<object?>.SqlServer();
                case SqlType.PostgreSql:
                    return SqlMethodTranslatorHelpers<object?>.PostgrSql();
                default:
                    throw new NotSupportedException(sqlType.ToString());
            }
        }

        public MergeHelper(SqlType sqlType, IMethodTranslator<object?> translator, IMethodWrapper methodWrapper)
        {
            SqlType = sqlType;
            Translator = translator;
            MethodWrapper = methodWrapper;
        }

        public MergeHelper(SqlType sqlType)
            : this(sqlType, GetTranslator(sqlType), GetMethodWrapper(sqlType))
        {
        }

        public SqlType SqlType { get; }

        public IMethodTranslator<object?> Translator { get; }

        public IMethodWrapper MethodWrapper { get; }

        public bool UseDerivedExists { get; set; }

        public IEnumerable<WhereItem>? WhereItems { get; set; }

        private SourceTableColumnDefine FastDistinctCount(SourceTableColumnDefine define, SourceTableDefine sourceTableDefine)
        {
            if (define.Method != ToRawMethod.DistinctCount || SqlType != SqlType.SqlServer)
            {
                return define;
            }
            var raw = $@"
(
        SELECT
            {string.Format(define.RawFormat, $"{Wrap("c")}.{Wrap(define.Field)}")}
        FROM
            {Wrap(sourceTableDefine.Table)} AS {Wrap("c")}
        WHERE {(string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => $"{string.Format(x.RawFormat, $"{Wrap("c")}.{Wrap(x.Field)}")} = {x.Raw}")))}
)
";
            return new SourceTableColumnDefine(define.Field, raw, define.IsGroup, define.DestColumn, define.Method, define.RawFormat);
        }
        private string GetTableRef(SourceTableDefine sourceTableDefine, CompileOptions? options = null)
        {
            if (options != null && options.IncludeEffectJoin && options.EffectTable != null)
            {
                return $@"{Wrap(options.EffectTable)} AS {Wrap("b")} INNER JOIN {Wrap(sourceTableDefine.Table)} AS {Wrap("a")} ON {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => $"{string.Format(x.RawFormat, $"{Wrap("b")}.{Wrap(x.Field)}")}={string.Format(x.RawFormat, $"{Wrap("a")}.{Wrap(x.Field)}")}"))}";
            }
           return $"{Wrap(sourceTableDefine.Table)} AS {Wrap("a")}";
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
    {string.Join(",\n", sourceTableDefine.Columns.Where(x => !x.IsGroup && !x.OnlySet).Select(x => $@"{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
FROM
	{Wrap(destTable)} AS {Wrap("a")}
	INNER JOIN (
		SELECT
            {string.Join(",\n", sourceTableDefine.Columns.Select(x => FastDistinctCount(x, sourceTableDefine)).Select(x => $"{x.Raw} AS {Wrap(x.DestColumn.Field)}"))}
		FROM {fromTable}
        {(WhereItems == null ? string.Empty : "WHERE " + string.Join(" AND ", WhereItems.Select(x => $"{x.Raw} = {x.Value}")))}
		GROUP BY
            {string.Join(",\n", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => x.Raw))}
	) AS  {Wrap("tmp")} ON {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => $" {Wrap("a")}.{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
AND(
    {string.Join(" OR ", sourceTableDefine.Columns.Where(x => !x.IsGroup && !x.OnlySet).Select(x => $" {Wrap("a")}.{Wrap(x.DestColumn.Field)} != {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
);
";
            }
            else if (SqlType == SqlType.PostgreSql)
            {
                return $@"
UPDATE
	{Wrap(destTable)} AS {Wrap("a")}
SET
    {string.Join(",\n", sourceTableDefine.Columns.Where(x => !x.IsGroup && !x.OnlySet).Select(x => $@"{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
	FROM (
		SELECT
            {string.Join(",\n", sourceTableDefine.Columns.Select(x => $"{x.Raw} AS \"{x.DestColumn.Field}\""))}
		FROM {fromTable}
		{(WhereItems == null ? string.Empty : "WHERE " + string.Join(" AND ", WhereItems.Select(x => $"{x.Raw} = {x.Value}")))}
        GROUP BY {string.Join(",\n", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => x.Raw))}

) AS {Wrap("tmp")}
    WHERE {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => $"{Wrap("a")}.{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
";
            }
            else if (SqlType == SqlType.SQLite)
            {
                return $@"
UPDATE
    {Wrap(destTable)}
SET
    {string.Join(",\n", sourceTableDefine.Columns.Where(x => !x.IsGroup && !x.OnlySet).Select(x => $@"{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.""{x.DestColumn.Field}"""))}
FROM (
        SELECT
            {string.Join(",\n", sourceTableDefine.Columns.Select(x => $"{x.Raw} AS \"{x.DestColumn.Field}\""))}
        FROM {fromTable}
		{(WhereItems == null ? string.Empty : "WHERE " + string.Join(" AND ", WhereItems.Select(x => $"{x.Raw} = {x.Value}")) + " AND ")}
        EXISTS(
            SELECT 1 AS {Wrap("___tmp")} FROM {Wrap(destTable)} AS {Wrap("c")} WHERE 
            {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => $"{Wrap("a")}.{Wrap(x.Field)} = {Wrap("c")}.{Wrap(x.DestColumn.Field)}"))}
        )
        GROUP BY
            {string.Join(",\n", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => x.Raw))}
    ) AS {Wrap("tmp")}
    WHERE {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => $"\"{destTable}\".{Wrap(x.DestColumn.Field)} ={Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
AND(
    {string.Join(" OR ", sourceTableDefine.Columns.Where(x => !x.IsGroup && !x.OnlySet).Select(x => $"{Wrap(destTable)}.{Wrap(x.DestColumn.Field)} != {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
)
";
            }
            return $@"
UPDATE {Wrap(destTable)} AS {Wrap("a")}
	INNER JOIN (
		SELECT
            {string.Join(",\n", sourceTableDefine.Columns.Select(x => $"{x.Raw} AS {Wrap(x.DestColumn.Field)}"))}
		FROM {fromTable}
		{(WhereItems == null ? string.Empty : "WHERE " + string.Join(" AND ", WhereItems.Select(x => $"{x.Raw} = {x.Value}")))}
		GROUP BY
            {string.Join(",\n", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => x.Raw))}
	) AS {Wrap("tmp")} ON {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup && !x.OnlySet).Select(x => $"{Wrap("a")}.{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
AND(
    {string.Join(" OR ", sourceTableDefine.Columns.Where(x => !x.IsGroup && !x.OnlySet).Select(x => $"{Wrap("a")}.{Wrap(x.DestColumn.Field)} != {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
)
SET
    {string.Join(",\n", sourceTableDefine.Columns.Where(x => !x.IsGroup).Select(x => $@"{Wrap("a")}.{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
";
        }
        public virtual string CompileInsert(string destTable, SourceTableDefine sourceTableDefine, CompileOptions? options = null)
        {
            var str = $"INSERT INTO {Wrap(destTable)}({string.Join(", ", sourceTableDefine.Columns.Select(x => Wrap(x.DestColumn.Field)))})\n";
            str += $"SELECT {string.Join(",", sourceTableDefine.Columns.Select(x => FastDistinctCount(x, sourceTableDefine)).Select(x => $"{x.Raw} AS {Wrap(x.DestColumn.Field)}"))}\n";
            str += $"FROM {GetTableRef(sourceTableDefine, options)}\n";
            str += @$"WHERE {(WhereItems == null ? string.Empty : "(" + string.Join(" AND ", WhereItems.Select(x => $"{x.Raw} = {x.Value}")) + ")")}
                {(WhereItems != null ? "AND" : string.Empty)}";
            str += @$"
                NOT EXISTS(
                    SELECT 1 AS {Wrap("tmp")} 
                    FROM {Wrap(destTable)} AS {Wrap("c")}
                    WHERE {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => $"{x.Raw} = {Wrap("c")}.{Wrap(x.DestColumn.Field)}"))}
                )";
            str += $"GROUP BY {string.Join(",", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => x.Raw))};";
            return str;
        }
        public static bool IsSQLSyntx(SqlType sqlType)
        {
            return sqlType == SqlType.MySql || sqlType == SqlType.SQLite;
        }
        protected IQueryMetadata GetFormatString(ToRawMethod method, string fieldName, bool quto)
        {
            switch (method)
            {
                case ToRawMethod.Day:
                    if (SqlType == SqlType.SqlServer)
                    {
                        return new RawMetadata($"CONVERT(VARCHAR(10), {(quto ? Wrap(fieldName) : fieldName)}, 120)");
                    }
                    return DataFroamt(KnowsMethods.DateFormat, fieldName, IsSQLSyntx(SqlType) ? "%Y-%m-%d" : "yyyy-MM-dd", quto);
                case ToRawMethod.Hour:
                    if (SqlType == SqlType.SqlServer)
                    {
                        return new RawMetadata($"CONVERT(VARCHAR(13), {(quto ? Wrap(fieldName) : fieldName)}, 120)");
                    }
                    return DataFroamt(KnowsMethods.DateFormat, fieldName, IsSQLSyntx(SqlType) ? "%Y-%m-%d %H" : "yyyy-MM-dd HH", quto);
                case ToRawMethod.Minute:
                    if (SqlType == SqlType.SqlServer)
                    {
                        return new RawMetadata($"CONVERT(VARCHAR(16), {(quto ? Wrap(fieldName) : fieldName)}, 120)");
                    }
                    if (SqlType == SqlType.SQLite)
                    {
                        return DataFroamt(KnowsMethods.DateFormat, fieldName, "%Y-%m-%d %H:%M", quto);
                    }
                    return DataFroamt(KnowsMethods.DateFormat, fieldName, IsSQLSyntx(SqlType) ? "%Y-%m-%d %H:%i" : "yyyy-MM-dd HH:mm", quto);
                case ToRawMethod.Second:
                    if (SqlType == SqlType.SqlServer)
                    {
                        return new RawMetadata($"CONVERT(VARCHAR(20), {(quto ? Wrap(fieldName) : fieldName)}, 120)");
                    }
                    if (SqlType == SqlType.SQLite)
                    {
                        return DataFroamt(KnowsMethods.DateFormat, fieldName, "%Y-%m-%d %H:%M:%S", quto);
                    }
                    return DataFroamt(KnowsMethods.DateFormat, fieldName, IsSQLSyntx(SqlType) ? "%Y-%m-%d %H:%i:%s" : "yyyy-MM-dd HH:mm:ss", quto);
                case ToRawMethod.Month:
                    if (SqlType == SqlType.SqlServer)
                    {
                        return new RawMetadata($"CONVERT(VARCHAR(7), {(quto ? Wrap(fieldName) : fieldName)}, 120)");
                    }
                    return DataFroamt(KnowsMethods.DateFormat, fieldName, IsSQLSyntx(SqlType) ? "%Y-%m" : "yyyy-MM", quto);
                default:
                    throw new NotSupportedException(SqlType.ToString());
            }
        }

        public static IQueryMetadata DataFroamt(string method, string fieldName, string format, bool quto)
        {
            return new MethodMetadata(method,
                GetRef(fieldName, quto),
                new ValueMetadata(format));
        }
        private static IQueryMetadata GetRef(string field, bool quto)
        {
            return quto ? new ValueMetadata(field, true) : new RawMetadata(field);
        }
        public virtual string? ToRaw(ToRawMethod method, string field, bool quto)
        {
            switch (method)
            {
                case ToRawMethod.Now:
                    {
                        switch (SqlType)
                        {
                            case SqlType.SQLite:
                                return "datetime('now')";
                            case SqlType.SqlServer:
                                return "GETDATE()";
                            case SqlType.PostgreSql:
                                return "CURRENT_TIMESTAMP";
                            case SqlType.MySql:
                            default:
                                return "NOW()";
                        }
                    }
                case ToRawMethod.Min:
                    return Translator.Translate(new MethodMetadata(KnowsMethods.Min, GetRef(field, quto)), null);
                case ToRawMethod.Max:
                    return Translator.Translate(new MethodMetadata(KnowsMethods.Max, GetRef(field, quto)), null);
                case ToRawMethod.Count:
                    return Translator.Translate(new MethodMetadata(KnowsMethods.Count, GetRef(field, quto)), null);
                case ToRawMethod.DistinctCount:
                    return Translator.Translate(new MethodMetadata(KnowsMethods.DistinctCount, GetRef(field, quto)), null);
                case ToRawMethod.Sum:
                    return Translator.Translate(new MethodMetadata(KnowsMethods.Sum, GetRef(field, quto)), null);
                case ToRawMethod.Avg:
                    return Translator.Translate(new MethodMetadata(KnowsMethods.Avg, GetRef(field, quto)), null);
                case ToRawMethod.Year:
                case ToRawMethod.Month:
                case ToRawMethod.Day:
                case ToRawMethod.Hour:
                case ToRawMethod.Minute:
                case ToRawMethod.Second:
                    {
                        var f = GetFormatString(method, field, quto);
                        if (f is IMethodMetadata methodMetadata)
                        {
                            return Translator.Translate(methodMetadata, null);
                        }
                        return f.ToString();
                    }
                case ToRawMethod.Quarter:
                    return Translator.Translate(new MethodMetadata(KnowsMethods.StrConcat,
                            new MethodMetadata(
                                KnowsMethods.Year,
                                GetRef(field, quto)),
                            new ValueMetadata("-"),
                            new MethodMetadata(
                                KnowsMethods.Quarter,
                                GetRef(field, quto))), null);
                case ToRawMethod.Weak:
                    return Translator.Translate(new MethodMetadata(KnowsMethods.StrConcat,
                            new MethodMetadata(
                                KnowsMethods.Year,
                                GetRef(field, quto)),
                            new ValueMetadata("-"),
                            new MethodMetadata(
                                KnowsMethods.Weak,
                                GetRef(field, quto),
                                new ValueMetadata(1))), null);
                default:
                    return quto ? MethodWrapper.WrapValue(field) : field;
            }
        }
        public virtual string Wrap(string obj)
        {
            return MethodWrapper.Quto(obj);
        }
    }
}
