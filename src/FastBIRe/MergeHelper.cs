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
        private string JoinString(string left, string right)
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
        protected string GetFormatString(ToRawMethod method, string fieldName, bool quto)
        {
            var @ref = GetRef(fieldName, quto);
            switch (method)
            {
                case ToRawMethod.Year:
                    return JoinString(@ref, "'-01-01 00:00:00'");
                case ToRawMethod.Day:
                    {
                        string forMatter;
                        if (SqlType == SqlType.SqlServer)
                        {
                            forMatter = $"CONVERT(VARCHAR(10),{@ref} ,120)";
                        }
                        else if (SqlType == SqlType.SQLite)
                        {
                            forMatter = $"strftime('%Y-%m-%d', {@ref})";
                        }
                        else
                        {
                            forMatter = $"LEFT({@ref},10)";
                        }
                        return JoinString(forMatter, " 00:00:00");
                    }
                case ToRawMethod.Hour:
                    {
                        string forMatter;
                        if (SqlType == SqlType.SqlServer)
                        {
                            forMatter = $"CONVERT(VARCHAR(13),{@ref} ,120)";
                        }
                        else if (SqlType == SqlType.SQLite)
                        {
                            forMatter = $"strftime('%Y-%m-%d %H', {@ref})";
                        }
                        else
                        {
                            forMatter = $"LEFT({@ref},13)";
                        }
                        return JoinString(forMatter, ":00:00");
                    }
                case ToRawMethod.Minute:
                    {
                        string forMatter;
                        if (SqlType == SqlType.SqlServer)
                        {
                            forMatter = $"CONVERT(VARCHAR(16),{@ref} ,120)";
                        }
                        else if (SqlType == SqlType.SQLite)
                        {
                            forMatter = $"strftime('%Y-%m-%d %H:%M', {@ref})";
                        }
                        else
                        {
                            forMatter = $"LEFT({@ref},16)";
                        }
                        return JoinString(forMatter, ":00:00");
                    }
                case ToRawMethod.Second:
                    return @ref;
                case ToRawMethod.Month:
                    {
                        string forMatter;
                        if (SqlType == SqlType.SqlServer)
                        {
                            forMatter = $"CONVERT(VARCHAR(7),{@ref} ,120)";
                        }
                        else if (SqlType == SqlType.SQLite)
                        {
                            forMatter = $"strftime('%Y-%m', {@ref})";
                        }
                        else
                        {
                            forMatter = $"LEFT({@ref},7)";
                        }
                        return JoinString(forMatter, "-01 00:00:00");
                    }
                default:
                    throw new NotSupportedException(method.ToString());
            }
        }

        private string GetRef(string field, bool quto)
        {
            return quto ? MethodWrapper.Quto(field) : field;
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
                    return $"MIN({GetRef(field,quto)})";
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
                        string quarter;
                        if (SqlType == SqlType.SQLite)
                        {
                            quarter = $"COALESCE(NULLIF((SUBSTR({GetRef(field, quto)}, 4, 2) - 1) / 3, 0), 4)";
                        }
                        else if (SqlType == SqlType.SqlServer)
                        {
                            quarter = $"DATEPART(QUARTER,{GetRef(field, quto)})";
                        }
                        else if (SqlType == SqlType.PostgreSql)
                        {
                            quarter = $"EXTRACT(QUARTER FROM {GetRef(field, quto)})";
                        }
                        else
                        {
                            quarter = $"QUARTER({GetRef(field, quto)})";
                        }
                        return JoinString(JoinString(ToRaw(ToRawMethod.Year, field, quto)!, "-"), quarter);
                    }
                case ToRawMethod.Weak:
                    {
                        string weak;
                        if (SqlType == SqlType.SQLite)
                        {
                            weak = $"strftime('%W',{GetRef(field, quto)})";
                        }
                        else if (SqlType == SqlType.SqlServer)
                        {
                            weak = $"DATEPART(WEEK,{GetRef(field, quto)})";
                        }
                        else if (SqlType == SqlType.PostgreSql)
                        {
                            weak = $"to_char({GetRef(field, quto)},'WW')";
                        }
                        else
                        {
                            weak = $"WEEK({GetRef(field, quto)})";
                        }
                        return JoinString(JoinString(ToRaw(ToRawMethod.Year, field, quto)!, "-"), weak);
                    }
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
