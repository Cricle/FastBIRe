﻿using Ao.Stock.Querying;
using Ao.Stock.Warehouse;
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
            DefaultMethodTranslator<object?>? helper;
            switch (sqlType)
            {
                case SqlType.MySql:
                    helper= SqlMethodTranslatorHelpers<object?>.Mysql();
                    break;
                case SqlType.SQLite:
                    helper = SqlMethodTranslatorHelpers<object?>.Sqlite();
                    break;
                case SqlType.SqlServer:
                    helper = SqlMethodTranslatorHelpers<object?>.SqlServer();
                    break;
                case SqlType.PostgreSql:
                    helper = SqlMethodTranslatorHelpers<object?>.PostgrSql();
                    break;
                default:
                    throw new NotSupportedException(sqlType.ToString());
            }
            helper[KnowsMethods.StrLeft] = "LEFT({1},{2})";
            helper[KnowsMethods.StrRight] = "RIGHT({1},{2})";
            return helper;
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

        private string GetTableRef(SourceTableDefine sourceTableDefine, CompileOptions? options = null)
        {
            if (options != null && options.IncludeEffectJoin && options.EffectTable != null)
            {
                if (SqlType== SqlType.SqlServer||SqlType== SqlType.PostgreSql)
                {
                    return $@"{Wrap(sourceTableDefine.Table)} AS {Wrap("a")} WHERE EXISTS( SELECT 1 FROM {Wrap(options.EffectTable)} AS {Wrap("b")} WHERE {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => $"{Wrap("b")}.{Wrap(x.Field)}.{Wrap(x.Field)}={Wrap("a")}.{Wrap(x.Field)}"))})";
                }
                return $@"(SELECT {Wrap("a")}.* FROM {Wrap(options.EffectTable)} AS {Wrap("b")} INNER JOIN {Wrap(sourceTableDefine.Table)} AS {Wrap("a")} ON {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => $"{Wrap("b")}.{Wrap(x.Field)}={Wrap("a")}.{Wrap(x.Field)}"))}) AS {Wrap("a")}";
            }
            return $"{Wrap(sourceTableDefine.Table)} AS {Wrap("a")}";
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
FROM
	{Wrap(destTable)} AS {Wrap("a")}
	INNER JOIN (
		SELECT
            {string.Join(",\n", sourceTableDefine.Columns.Select(x => $"{x.Raw} AS {Wrap(x.DestColumn.Field)}"))} 
		FROM {fromTable}
        {(WhereItems == null || !WhereItems.Any() ? string.Empty : "WHERE " + string.Join(" AND ", WhereItems.Select(x => $"{x.Raw} = {x.Value}")))}
		GROUP BY
            {string.Join(",\n", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => x.Raw))}
	) AS  {Wrap("tmp")} ON {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => $" {Wrap("a")}.{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
AND(
    {string.Join(" OR ", sourceTableDefine.Columns.Where(x => !x.IsGroup && !x.OnlySet).Select(x => $" {Wrap("a")}.{Wrap(x.DestColumn.Field)} != {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
);
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
	FROM (
		SELECT
            {string.Join(",\n", sourceTableDefine.Columns.Select(x => $"{x.Raw} AS \"{x.DestColumn.Field}\""))}
		FROM {fromTable}
		{(WhereItems == null || !WhereItems.Any() ? string.Empty : "WHERE " + string.Join(" AND ", WhereItems.Select(x => $"{x.Raw} = {x.Value}")))}
        GROUP BY {string.Join(",\n", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => x.Raw))}

) AS {Wrap("tmp")}
    WHERE {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => $"{Wrap("a")}.{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
AND(
    {string.Join(" OR ", sourceTableDefine.Columns.Where(x => !x.IsGroup && !x.OnlySet).Select(x => $" {Wrap("a")}.{Wrap(x.DestColumn.Field)} != {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
);
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
FROM (
        SELECT
            {string.Join(",\n", sourceTableDefine.Columns.Select(x => $"{x.Raw} AS \"{x.DestColumn.Field}\""))}
        FROM {fromTable}
		{(WhereItems == null || !WhereItems.Any() ? string.Empty : "WHERE "+ string.Join(" AND ", WhereItems.Select(x => $"{x.Raw} = {x.Value}")))}
        GROUP BY
            {string.Join(",\n", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => x.Raw))}
    ) AS {Wrap("tmp")}
    WHERE {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => $"\"{destTable}\".{Wrap(x.DestColumn.Field)} ={Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
AND(
    {string.Join(" OR ", sourceTableDefine.Columns.Where(x => !x.IsGroup && !x.OnlySet).Select(x => $"{Wrap(destTable)}.{Wrap(x.DestColumn.Field)} != {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
);
{noLockRestoreSql}
";
            }
            return $@"
{noLockSql}
UPDATE {Wrap(destTable)} AS {Wrap("a")}
	INNER JOIN (
		SELECT
            {string.Join(",\n", sourceTableDefine.Columns.Select(x => $"{x.Raw} AS {Wrap(x.DestColumn.Field)}"))}
		FROM {fromTable}
		{(WhereItems == null|| !WhereItems.Any() ? string.Empty : "WHERE " + string.Join(" AND ", WhereItems.Select(x => $"{x.Raw} = {x.Value}")))}
		GROUP BY
            {string.Join(",\n", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => x.Raw))}
	) AS {Wrap("tmp")} ON {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup && !x.OnlySet).Select(x => $"{Wrap("a")}.{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
AND(
    {string.Join(" OR ", sourceTableDefine.Columns.Where(x => !x.IsGroup && !x.OnlySet).Select(x => $"{Wrap("a")}.{Wrap(x.DestColumn.Field)} != {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))}
)
SET
    {string.Join(",\n", sourceTableDefine.Columns.Where(x => !x.IsGroup).Select(x => $@"{Wrap("a")}.{Wrap(x.DestColumn.Field)} = {Wrap("tmp")}.{Wrap(x.DestColumn.Field)}"))};
{noLockRestoreSql}
";
        }
        private string GetNoLockRestoreSql()
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
        private string GetNoLockSql()
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
        public virtual string CompileInsert(string destTable, SourceTableDefine sourceTableDefine, CompileOptions? options = null)
        {
            var str = $"INSERT INTO {Wrap(destTable)}({string.Join(", ", sourceTableDefine.Columns.Select(x => Wrap(x.DestColumn.Field)))})\n";
            str += $"SELECT {string.Join(",", sourceTableDefine.Columns.Select(x => $"{x.Raw} AS {Wrap(x.DestColumn.Field)}"))}\n";
            str += $"FROM {GetTableRef(sourceTableDefine, options)}\n";
            if (SqlType == SqlType.SQLite&&sourceTableDefine.Columns.Any(x=>x.IsGroup&&x.Method== ToRawMethod.None))
            {
                str += $@"LEFT JOIN {Wrap(destTable)} AS {Wrap("c")} ON {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => $"{x.Raw} = {Wrap("c")}.{Wrap(x.DestColumn.Field)}"))}
                          WHERE {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup&&x.Method== ToRawMethod.None).Select(x=>$"{Wrap("c")}.{Wrap(x.DestColumn.Field)} IS NULL"))}
";
            }
            else
            {
                str += @$"{((SqlType == SqlType.SqlServer || SqlType == SqlType.PostgreSql) && (options?.IncludeEffectJoin ?? false) ? "AND" : "WHERE")} {(WhereItems == null || !WhereItems.Any() ? string.Empty : ("(" + string.Join(" AND ", WhereItems.Select(x => $"{x.Raw} = {x.Value}")) + ")"))} {(WhereItems == null || !WhereItems.Any() ? string.Empty : "AND")}";
                str += @$"
                NOT EXISTS(
                    SELECT 1 AS {Wrap("tmp")} 
                    FROM {Wrap(destTable)} AS {Wrap("c")}
                    WHERE {string.Join(" AND ", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => $"{x.Raw} = {Wrap("c")}.{Wrap(x.DestColumn.Field)}"))}
                )";
            }
            str += $"GROUP BY {string.Join(",", sourceTableDefine.Columns.Where(x => x.IsGroup).Select(x => x.Raw))};\n";
            str += options?.NoLock ?? false ? GetNoLockRestoreSql() : string.Empty;
            return str;
        }
        protected IQueryMetadata GetFormatString(ToRawMethod method, string fieldName, bool quto)
        {
            switch (method)
            {
                case ToRawMethod.Year:
                    return new MethodMetadata(KnowsMethods.Year, GetRef(fieldName, quto), new ValueMetadata("-01-01 00:00:00"));
                case ToRawMethod.Day:
                    if (SqlType == SqlType.SqlServer)
                    {
                        return new MethodMetadata(KnowsMethods.StrConcat, new RawMetadata($"CONVERT(VARCHAR(10), {(quto ? Wrap(fieldName) : fieldName)}, 120)"), new ValueMetadata(" 00:00:00"));
                    }
                    if (SqlType == SqlType.SQLite)
                    {
                        return new RawMetadata($"strftime( '%Y-%m-%d', {(quto?MethodWrapper.Quto(fieldName):fieldName)}) || ' 00:00:00'");
                    }
                    return new MethodMetadata(KnowsMethods.StrConcat, DataFroamt(KnowsMethods.StrLeft, fieldName, 10, quto),new ValueMetadata(" 00:00:00"));
                case ToRawMethod.Hour:
                    if (SqlType == SqlType.SqlServer)
                    {
                        return new MethodMetadata(KnowsMethods.StrConcat, new RawMetadata($"CONVERT(VARCHAR(13), {(quto ? Wrap(fieldName) : fieldName)}, 120)"), new ValueMetadata(":00:00"));
                    }
                    if (SqlType == SqlType.SQLite)
                    {
                        return new MethodMetadata(KnowsMethods.StrConcat, DataFroamt(KnowsMethods.DateFormat, fieldName, "%Y-%m-%d %H", quto), new ValueMetadata(":00:00"));
                    }
                    return new MethodMetadata(KnowsMethods.StrConcat, DataFroamt(KnowsMethods.StrLeft, fieldName, 13, quto), new ValueMetadata(":00:00"));
                case ToRawMethod.Minute:
                    if (SqlType == SqlType.SqlServer)
                    {
                        return new MethodMetadata(KnowsMethods.StrConcat, new RawMetadata($"CONVERT(VARCHAR(16), {(quto ? Wrap(fieldName) : fieldName)}, 120)"), new ValueMetadata(":00"));
                    }
                    if (SqlType == SqlType.SQLite)
                    {
                        return new RawMetadata($"strftime( '%Y-%m-%d %H:%M', {(quto ? MethodWrapper.Quto(fieldName) : fieldName)}) || ':00'");
                    }
                    if (SqlType== SqlType.PostgreSql)
                    {
                        return new MethodMetadata(KnowsMethods.StrConcat, DataFroamt(KnowsMethods.DateFormat, fieldName, "yyyy-MM-dd HH:mm", quto), new ValueMetadata(":00"));
                    }
                    return new MethodMetadata(KnowsMethods.StrConcat, DataFroamt(KnowsMethods.StrLeft, fieldName, 16, quto), new ValueMetadata(":00"));
                case ToRawMethod.Second:
                    return new RawMetadata(quto ? Wrap(fieldName) : fieldName);
                case ToRawMethod.Month:
                    if (SqlType == SqlType.SqlServer)
                    {
                        return new MethodMetadata(KnowsMethods.StrConcat, new RawMetadata($"CONVERT(VARCHAR(7), {(quto ? Wrap(fieldName) : fieldName)}, 120)"), new ValueMetadata("-01 00:00:00"));
                    }
                    return new MethodMetadata(KnowsMethods.StrConcat, DataFroamt(KnowsMethods.StrLeft, fieldName, 7, quto), new ValueMetadata("-01 00:00:00"));
                default:
                    throw new NotSupportedException(method.ToString());
            }
        }

        public static IQueryMetadata DataFroamt(string method, string fieldName, object format, bool quto)
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
