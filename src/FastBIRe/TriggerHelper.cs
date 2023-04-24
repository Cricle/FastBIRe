using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public record class TriggerField(string Field, string Raw,string Type,string RawFormat);
    public class ComputeTriggerHelper
    {
        public const string Update = "_update";
        public const string Insert = "_insert";

        public static readonly ComputeTriggerHelper Instance = new ComputeTriggerHelper();

        public string? DropRaw(string name,string sourceTable, SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return DropSqlServerRaw(name);
                case SqlType.MySql:
                    return DropMySqlRaw(name);
                case SqlType.SQLite:
                    return DropSqliteRaw(name);
                case SqlType.PostgreSql:
                    return DropPostgreSQLRaw(name, sourceTable);
                case SqlType.Oracle:
                case SqlType.Db2:
                default:
                    return null;
            }
        }
        public IEnumerable<string> Create(string name, string sourceTable, IEnumerable<TriggerField> columns,string idColumn, SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return CreateSqlServer(name, sourceTable, columns,idColumn);
                case SqlType.MySql:
                    return CreateMySql(name, sourceTable, columns);
                case SqlType.SQLite:
                    return CreateSqlite(name, sourceTable, columns,idColumn);
                case SqlType.PostgreSql:
                    return CreatePostgreSQL(name, sourceTable, columns,idColumn);
                case SqlType.Oracle:
                case SqlType.Db2:
                default:
                    return Enumerable.Empty<string>();
            }
        }
        public IEnumerable<string> CreateMySql(string name, string sourceTable, IEnumerable<TriggerField> columns)
        {
            yield return $@"CREATE TRIGGER `{name}{Insert}` BEFORE INSERT ON `{sourceTable}`
FOR EACH ROW
BEGIN
    {string.Join("\n", columns.Select(x => $"SET NEW.`{x.Field}` = {x.Raw};"))}
END;
";
            yield return $@"CREATE TRIGGER `{name}{Update}` BEFORE UPDATE ON `{sourceTable}`
FOR EACH ROW
BEGIN
    {string.Join("\n", columns.Select(x => $"SET NEW.`{x.Field}` = {x.Raw};"))}
END;
";
        }
        public IEnumerable<string> CreateSqlServer(string name, string sourceTable, IEnumerable<TriggerField> columns, string idColumn)
        {
            yield return $@"CREATE TRIGGER [{name}{Insert}] ON [{sourceTable}] AFTER INSERT
        AS
        BEGIN
            UPDATE [{sourceTable}] SET {string.Join(", ", columns.Select(x => $"[{x.Field}] = {x.Raw}"))}
            WHERE [{sourceTable}].[{idColumn}] IN(SELECT [{idColumn}] FROM INSERTED);
        END;
        ";
            yield return $@"CREATE TRIGGER [{name}{Update}] ON [{sourceTable}] AFTER UPDATE
        AS
        BEGIN
            UPDATE [{sourceTable}] SET {string.Join(", ", columns.Select(x => $"[{x.Field}] = {x.Raw}"))}
            WHERE [{sourceTable}].[{idColumn}] IN(SELECT [{idColumn}] FROM INSERTED);
        END;
        ";
        }
        public IEnumerable<string> CreateSqlite(string name, string sourceTable, IEnumerable<TriggerField> columns,string idColumn)
        {
            yield return $@"
        CREATE TRIGGER [{name}{Insert}] AFTER INSERT ON [{sourceTable}]
        BEGIN
            UPDATE [{sourceTable}] SET {string.Join(", ", columns.Select(x => $"`{x.Field}` = {x.Raw}"))}
            WHERE [{idColumn}]=NEW.[{idColumn}];
        END;
        ";
            yield return $@"
        CREATE TRIGGER [{name}{Update}] AFTER UPDATE ON [{sourceTable}]
        BEGIN
            UPDATE [{sourceTable}] SET {string.Join(", ", columns.Select(x => $"`{x.Field}` = {x.Raw}"))}
            WHERE [{idColumn}]=NEW.[{idColumn}];
        END;
    
        ";
        }
        private string GetNpgSqlFunName(string name)
        {
            return "fun_" + name.Replace('-', '_');
        }
        public IEnumerable<string> CreatePostgreSQL(string name, string sourceTable, IEnumerable<TriggerField> columns,string idColumn)
        {
            var funName = GetNpgSqlFunName(name);
            yield return $@"CREATE FUNCTION {funName}{Insert}() RETURNS TRIGGER AS $$
        BEGIN
              {string.Join("\n", columns.Select(x => $"NEW.\"{x.Field}\" = {x.Raw};"))}
        END;
        $$ LANGUAGE plpgsql;
        ";
            yield return $@"
        CREATE TRIGGER ""{name}{Insert}"" AFTER INSERT ON ""{sourceTable}""
        FOR EACH ROW
        EXECUTE FUNCTION {funName}{Insert}();
        ";
            yield return $@"CREATE FUNCTION {funName}{Update}() RETURNS TRIGGER AS $$
        BEGIN
            {string.Join("\n", columns.Select(x => $"NEW.\"{x.Field}\" = {x.Raw};"))}
        END;
        $$ LANGUAGE plpgsql;
        ";
            yield return $@"
        CREATE TRIGGER ""{name}{Update}"" AFTER INSERT ON ""{sourceTable}""
        FOR EACH ROW
        EXECUTE FUNCTION {funName}{Update}();
        ";
        }
        public string DropMySqlRaw(string name)
        {
            return $@"DROP TRIGGER IF EXISTS `{name}`;";
        }
        public string DropSqliteRaw(string name)
        {
            return $@"DROP TRIGGER IF EXISTS `{name}`;";
        }
        public string DropSqlServerRaw(string name)
        {
            return $@"DROP TRIGGER IF EXISTS [{name}];";
        }
        public string DropPostgreSQLRaw(string name, string sourceTable)
        {
            var funName = GetNpgSqlFunName(name);
            return $@"
        DROP TRIGGER IF EXISTS ""{name}"" ON ""{sourceTable}"";
        DROP FUNCTION IF EXISTS {funName}();
        ";
        }
    }
    public class EffectTriggerHelper
    {
        public static readonly EffectTriggerHelper Instance = new EffectTriggerHelper();

        public string? Drop(string name, string sourceTable, SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return DropSqlServer(name, sourceTable);
                case SqlType.MySql:
                    return DropMySql(name);
                case SqlType.SQLite:
                    return DropSqlite(name);
                case SqlType.PostgreSql:
                    return DropPostgreSQL(name, sourceTable);
                case SqlType.Oracle:
                case SqlType.Db2:
                default:
                    return null;
            }
        }
        public string? Create(string name, string sourceTable, string targetTable, IEnumerable<TriggerField> columns, SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return CreateSqlServer(name, sourceTable, targetTable, columns);
                case SqlType.MySql:
                    return CreateMySql(name, sourceTable, targetTable, columns);
                case SqlType.SQLite:
                    return CreateSqlite(name, sourceTable, targetTable, columns);
                case SqlType.PostgreSql:
                    return CreatePostgreSQL(name, sourceTable, targetTable, columns);
                case SqlType.Oracle:
                case SqlType.Db2:
                default:
                    return null;
            }
        }

        public string CreateMySql(string name, string sourceTable, string targetTable, IEnumerable<TriggerField> columns)
        {
            return $@"CREATE TRIGGER `{name}` AFTER INSERT ON `{sourceTable}`
FOR EACH ROW
BEGIN
	INSERT IGNORE INTO `{targetTable}`({string.Join(",", columns.Select(x => $"`{x.Field}`"))}) VALUES({string.Join(",", columns.Select(x => x.Raw))});
END;
";
        }
        public string CreateSqlServer(string name, string sourceTable, string targetTable, IEnumerable<TriggerField> columns)
        {
            return $@"CREATE TRIGGER [{name}] ON [{sourceTable}] AFTER INSERT
AS
BEGIN
    INSERT INTO [{targetTable}] ({string.Join(",", columns.Select(x => $"[{x.Field}]"))})
    SELECT {string.Join(",", columns.Select(x =>x.Raw))}
    FROM (SELECT {string.Join(",", columns.Select(x => $"[{x.Field}]"))} FROM INSERTED GROUP BY {string.Join(",", columns.Select(x => $"[{x.Field}]"))}) AS [NEW]
    WHERE NOT EXISTS(
        SELECT 1 FROM [{targetTable}] AS [t] WHERE {string.Join(" AND ", columns.Select(x => $"{string.Format(x.RawFormat,$"[t].[{x.Field}]")} = {x.Raw}"))}
    )
END;
";
        }
        public string CreateSqlite(string name, string sourceTable, string targetTable, IEnumerable<TriggerField> columns)
        {
            return $@"CREATE TRIGGER [{name}] INSERT ON [{sourceTable}]
BEGIN
    INSERT OR IGNORE INTO [{targetTable}] ({string.Join(",", columns.Select(x => $"[{x.Field}]"))}) VALUES({string.Join(",", columns.Select(x => x.Raw))});
END;
";
        }
        private string GetNpgSqlFunName(string name)
        {
            return "fun_" + name.Replace('-', '_');
        }
        public string CreatePostgreSQL(string name, string sourceTable, string targetTable, IEnumerable<TriggerField> columns)
        {
            var funName = GetNpgSqlFunName(name);
            return $@"CREATE FUNCTION {funName}() RETURNS TRIGGER AS $$
BEGIN
  INSERT INTO ""{targetTable}"" ({string.Join(",", columns.Select(x => $"\"{x.Field}\""))}) VALUES ({string.Join(",", columns.Select(x => x.Raw))})
    ON CONFLICT DO NOTHING;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER ""{name}"" AFTER INSERT ON ""{sourceTable}""
FOR EACH ROW
EXECUTE FUNCTION {funName}();
";
        }
        public string DropMySql(string name)
        {
            return $@"DROP TRIGGER IF EXISTS `{name}`;";
        }
        public string DropSqlServer(string name, string sourceTable)
        {
            return $@"DROP TRIGGER IF EXISTS [{name}];";
        }
        public string DropSqlite(string name)
        {
            return $@"DROP TRIGGER IF EXISTS [{name}];";
        }
        public string DropPostgreSQL(string name, string sourceTable)
        {
            var funName = GetNpgSqlFunName(name);
            return $@"
DROP TRIGGER IF EXISTS ""{name}"" ON ""{sourceTable}"";
DROP FUNCTION IF EXISTS {funName}();
";
        }
    }
}
