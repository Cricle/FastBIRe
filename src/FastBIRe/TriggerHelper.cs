using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public record class TriggerField(string Field, string Raw, string Target, string Type, string RawFormat);
    public class ComputeTriggerHelper
    {
        public const string DefaultUpdateSuffix = "_update";
        public const string DefaultInsertSuffix = "_insert";

        public static readonly ComputeTriggerHelper Default = new ComputeTriggerHelper(DefaultUpdateSuffix,DefaultInsertSuffix);

        public ComputeTriggerHelper(string updateSuffix, string insertSuffix)
        {
            UpdateSuffix = updateSuffix;
            InsertSuffix = insertSuffix;
        }

        public string UpdateSuffix { get; }

        public string InsertSuffix { get; }

        public string? DropRaw(string name, string sourceTable, SqlType sqlType)
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
        public IEnumerable<string> Create(string name, string sourceTable, IEnumerable<TriggerField> columns, string idColumn, SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return CreateSqlServer(name, sourceTable, columns, idColumn);
                case SqlType.MySql:
                    return CreateMySql(name, sourceTable, columns);
                case SqlType.SQLite:
                    return CreateSqlite(name, sourceTable, columns, idColumn);
                case SqlType.PostgreSql:
                    return CreatePostgreSQL(name, sourceTable, columns);
                case SqlType.Oracle:
                case SqlType.Db2:
                default:
                    return Enumerable.Empty<string>();
            }
        }
        public IEnumerable<string> CreateMySql(string name, string sourceTable, IEnumerable<TriggerField> columns)
        {
            string GenSql(bool update)
            {
                return $@"CREATE TRIGGER `{name}{(update ? InsertSuffix : UpdateSuffix)}` BEFORE {(update ? "INSERT" : "UPDATE")} ON `{sourceTable}`
FOR EACH ROW
BEGIN
    {string.Join("\n", columns.Select(x => $"SET NEW.`{x.Field}` = CASE WHEN `NEW`.`{x.Target}` IS NULL THEN NULL ELSE {x.Raw} END;"))}
END;
";
            }
            yield return GenSql(false);
            yield return GenSql(true);
        }
        public IEnumerable<string> CreateSqlServer(string name, string sourceTable, IEnumerable<TriggerField> columns, string idColumn)
        {
            string GenSql(bool update)
            {
                return $@"CREATE TRIGGER [{name}{(update ? InsertSuffix : UpdateSuffix)}] ON [{sourceTable}] AFTER {(update ? "INSERT" : "UPDATE")}
        AS
        BEGIN
            SET NOCOUNT ON;
            UPDATE [{sourceTable}] SET {string.Join(", ", columns.Select(x => $"[{x.Field}] = {x.Raw}"))}
            WHERE [{sourceTable}].[{idColumn}] IN(SELECT [{idColumn}] FROM INSERTED WHERE {string.Join(" AND ", columns.Select(x => x.Target).Distinct().Select(x => $"[{x}] IS NOT NULL"))});
            UPDATE [{sourceTable}] SET {string.Join(", ", columns.Select(x => $"[{x.Field}] = NULL"))}
            WHERE [{sourceTable}].[{idColumn}] IN(SELECT [{idColumn}] FROM INSERTED WHERE {string.Join(" AND ", columns.Select(x => x.Target).Distinct().Select(x => $"[{x}] IS NULL"))});
        END;
";
            }
            yield return GenSql(false);
            yield return GenSql(true);
        }
        public IEnumerable<string> CreateSqlite(string name, string sourceTable, IEnumerable<TriggerField> columns, string idColumn)
        {
            string GenSql(bool update)
            {
                return $@"CREATE TRIGGER [{name}{(update ? InsertSuffix : UpdateSuffix)}] AFTER {(update ? "INSERT" : "UPDATE")} ON [{sourceTable}]
        BEGIN
            UPDATE [{sourceTable}] SET {string.Join(", ", columns.Select(x => $"[{x.Field}] = CASE WHEN NEW.[{x.Target}] IS NULL THEN NULL ELSE {x.Raw} END"))}
            WHERE [{idColumn}]=NEW.[{idColumn}];
        END;
";
            }
            yield return GenSql(false);
            yield return GenSql(true);
        }
        public IEnumerable<string> CreatePostgreSQL(string name, string sourceTable, IEnumerable<TriggerField> columns)
        {
            IEnumerable<string> GenSql(bool update)
            {
                var funName = PostgreSQLTriggerHelper.GetNpgSqlFunName(name);
                yield return $@"CREATE OR REPLACE FUNCTION {funName}{(update ? InsertSuffix : UpdateSuffix)}() RETURNS TRIGGER AS $$
        BEGIN
              {string.Join("\n", columns.Select(x => $"NEW.\"{x.Field}\" = CASE WHEN NEW.\"{x.Target}\" IS NULL THEN NULL ELSE {x.Raw} END;"))}
              RETURN NEW;
        END;
        $$ LANGUAGE plpgsql;
        ";
                yield return $@"
        CREATE OR REPLACE TRIGGER ""{name}{(update ? InsertSuffix : UpdateSuffix)}"" BEFORE {(update ? "INSERT" : "UPDATE")} ON ""{sourceTable}""
        FOR EACH ROW
        EXECUTE FUNCTION {funName}{(update ? InsertSuffix : UpdateSuffix)}();
        ";
            }
            foreach (var item in GenSql(false))
            {
                yield return item;
            }
            foreach (var item in GenSql(true))
            {
                yield return item;
            }
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
            var funName = PostgreSQLTriggerHelper.GetNpgSqlFunName(name);
            return $@"
        DROP TRIGGER IF EXISTS ""{name}"" ON ""{sourceTable}"";
        DROP FUNCTION IF EXISTS {funName}();
        ";
        }
    }
    public class EffectTriggerHelper
    {
        public const string Update = "_update";
        public const string Insert = "_insert";

        public static readonly EffectTriggerHelper Instance = new EffectTriggerHelper();

        public string[] Drop(string name, string sourceTable, SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return DropSqlServer(name);
                case SqlType.MySql:
                    return DropMySql(name);
                case SqlType.SQLite:
                    return DropSqlite(name);
                case SqlType.PostgreSql:
                    return DropPostgreSQL(name, sourceTable);
                case SqlType.Oracle:
                case SqlType.Db2:
                default:
                    return Array.Empty<string>();
            }
        }
        public string[] Create(string name, string sourceTable, string targetTable, IEnumerable<TriggerField> columns, SqlType sqlType)
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
                    return Array.Empty<string>();
            }
        }

        public string[] CreateMySql(string name, string sourceTable, string targetTable, IEnumerable<TriggerField> columns)
        {
            string GenSql(bool update)
            {
                return $@"CREATE TRIGGER `{name}{(update ? Update : Insert)}` AFTER {(update ? "UPDATE" : "INSERT")} ON `{sourceTable}`
FOR EACH ROW
BEGIN
    IF {string.Join(" AND ", columns.Select(x => x.Target).Distinct().Select(x => $"NEW.`{x}` IS NOT NULL"))} THEN
	INSERT IGNORE INTO `{targetTable}`({string.Join(",", columns.Select(x => $"`{x.Field}`"))}) VALUES({string.Join(",", columns.Select(x => x.Raw))});
    END IF;
END;
";
            }
            return new string[]
            {
                GenSql(false),
                GenSql(true)
            };

        }
        public string[] CreateSqlServer(string name, string sourceTable, string targetTable, IEnumerable<TriggerField> columns)
        {
            string GenSql(bool update)
            {
                return $@"CREATE TRIGGER [{name}{(update ? Update : Insert)}] ON [{sourceTable}] AFTER {(update ? "UPDATE" : "INSERT")}
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [{targetTable}] ({string.Join(",", columns.Select(x => $"[{x.Field}]"))})
    SELECT {string.Join(",", columns.Select(x => x.Raw))}
    FROM (
        SELECT {string.Join(",", columns.Select(x => $"{string.Format(x.RawFormat, $"[NEW].[{x.Field}]")} AS [{x.Field}]"))} 
        FROM INSERTED AS [NEW] 
        WHERE {string.Join(" AND ", columns.Select(x => x.Target).Distinct().Select(x => $"[NEW].[{x}] IS NOT NULL"))}
        GROUP BY {string.Join(",", columns.Select(x => string.Format(x.RawFormat, $"[NEW].[{x.Field}]")))}
        HAVING NOT EXISTS(
            SELECT 1 FROM [{targetTable}] AS [t] 
            WHERE {string.Join(" AND ", columns.Select(x => $"{string.Format(x.RawFormat, $"[t].[{x.Field}]")} = {string.Format(x.RawFormat, $"[NEW].[{x.Field}]")}"))}
        )
    ) AS [NEW];
END;
";
            }
            return new string[]
            {
                GenSql(false),
                GenSql(true)
            };
        }
        public string[] CreateSqlite(string name, string sourceTable, string targetTable, IEnumerable<TriggerField> columns)
        {
            string GenSql(bool update)
            {
                return $@"CREATE TRIGGER [{name}{(update ? Update : Insert)}] {(update ? "UPDATE" : "INSERT")} ON [{sourceTable}]
WHEN {string.Join(" AND ", columns.Select(x => x.Target).Distinct().Select(x => $"[NEW].[{x}] IS NOT NULL"))}
BEGIN
    INSERT OR IGNORE INTO [{targetTable}] ({string.Join(",", columns.Select(x => $"[{x.Field}]"))}) VALUES({string.Join(",", columns.Select(x => x.Raw))});
END;
";
            }
            return new string[]
            {
                GenSql(false),
                GenSql(true)
            };
        }
        public string[] CreatePostgreSQL(string name, string sourceTable, string targetTable, IEnumerable<TriggerField> columns)
        {
            string GenSql(bool update)
            {
                var funName = PostgreSQLTriggerHelper.GetNpgSqlFunName(name + (update ? Update : Insert));
                return $@"CREATE OR REPLACE FUNCTION {funName}() RETURNS TRIGGER AS $$
BEGIN
  IF {string.Join(" AND ", columns.Select(x => x.Target).Distinct().Select(x => $"NEW.\"{x}\" IS NOT NULL"))} THEN
  INSERT INTO ""{targetTable}"" ({string.Join(",", columns.Select(x => $"\"{x.Field}\""))}) VALUES ({string.Join(",", columns.Select(x => x.Raw))})
    ON CONFLICT DO NOTHING;
  END IF;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE TRIGGER ""{name + (update ? Update : Insert)}"" AFTER {(update ? "UPDATE" : "INSERT")} ON ""{sourceTable}""
FOR EACH ROW
EXECUTE FUNCTION {funName}();
";
            }
            var funName = PostgreSQLTriggerHelper.GetNpgSqlFunName(name);
            return new string[]
            {
                GenSql(false),
                GenSql(true)
            };
        }
        public string[] DropMySql(string name)
        {
            return new string[]
            {
                $@"DROP TRIGGER IF EXISTS `{name}{Insert}`;",
                $@"DROP TRIGGER IF EXISTS `{name}{Update}`;"
            };
        }
        public string[] DropSqlServer(string name)
        {
            return new string[]
            {
                $@"DROP TRIGGER IF EXISTS [{name}{Insert}];",
                $@"DROP TRIGGER IF EXISTS [{name}{Update}];"
            };
        }
        public string[] DropSqlite(string name)
        {
            return new string[]
            {
                $@"DROP TRIGGER IF EXISTS [{name}{Insert}];",
                $@"DROP TRIGGER IF EXISTS [{name}{Update}];"
            };
        }
        public string[] DropPostgreSQL(string name, string sourceTable)
        {
            return new string[]
            {
                $@"DROP TRIGGER IF EXISTS ""{name}{Insert}"" ON ""{sourceTable}"";",
                $@"DROP FUNCTION IF EXISTS {PostgreSQLTriggerHelper.GetNpgSqlFunName(name+Insert)}();",
                $@"DROP TRIGGER IF EXISTS ""{name}{Update}"" ON ""{sourceTable}"";",
                $@"DROP FUNCTION IF EXISTS {PostgreSQLTriggerHelper.GetNpgSqlFunName(name+Update)}();",
            };
        }
    }
    public static class PostgreSQLTriggerHelper
    {
        public static string GetNpgSqlFunName(string name)
        {
            return "fun_" + name.Replace('-', '_');
        }
    }
}
