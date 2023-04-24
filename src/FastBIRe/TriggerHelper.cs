﻿using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public record class TriggerField(string Field, string Raw);
    public class ComputeTriggerHelper
    {
        public const string Update = "_update";
        public const string Insert = "_insert";

        public static readonly ComputeTriggerHelper Instance = new ComputeTriggerHelper();

        public string? DropRaw(string name, SqlType sqlType)
        {
            switch (sqlType)
            {
                //case SqlType.SqlServerCe:
                //case SqlType.SqlServer:
                //    return DropSqlServer(name, sourceTable);
                case SqlType.MySql:
                    return DropMySqlRaw(name);
                case SqlType.SQLite:
                    return DropSqliteRaw(name);
                //case SqlType.PostgreSql:
                //    return DropPostgreSQL(name, sourceTable);
                //case SqlType.Oracle:
                //case SqlType.Db2:
                default:
                    return null;
            }
        }
        public string? Create(string name, string sourceTable, IEnumerable<TriggerField> columns,string idColumn, SqlType sqlType)
        {
            switch (sqlType)
            {
                //case SqlType.SqlServerCe:
                //case SqlType.SqlServer:
                //    return CreateSqlServer(name, sourceTable, targetTable, columns);
                case SqlType.MySql:
                    return CreateMySql(name, sourceTable, columns);
                case SqlType.SQLite:
                    return CreateSqlite(name, sourceTable, columns,idColumn);
                //case SqlType.PostgreSql:
                //    return CreatePostgreSQL(name, sourceTable, targetTable, columns);
                //case SqlType.Oracle:
                //case SqlType.Db2:
                default:
                    return null;
            }
        }
        public string CreateMySql(string name, string sourceTable, IEnumerable<TriggerField> columns)
        {
            return $@"CREATE TRIGGER `{name}{Insert}` BEFORE INSERT ON `{sourceTable}`
FOR EACH ROW
BEGIN
    {string.Join("\n",columns.Select(x=>$"SET NEW.`{x.Field}` = {x.Raw};"))}
END;
CREATE TRIGGER `{name}{Update}` BEFORE UPDATE ON `{sourceTable}`
FOR EACH ROW
BEGIN
    {string.Join("\n", columns.Select(x => $"SET NEW.`{x.Field}` = {x.Raw};"))}
END;
";
        }
        //        public string CreateSqlServer(string name, string sourceTable, string targetTable, IEnumerable<TriggerField> columns)
        //        {
        //            return $@"CREATE TRIGGER [{name}] ON [{sourceTable}] AFTER INSERT
        //AS
        //BEGIN
        //    INSERT INTO [{targetTable}] ({string.Join(",", columns.Select(x => $"[{x.Field}]"))})
        //    SELECT {string.Join(",", columns.Select(x => $"[NEW].[{x.Field}]"))}
        //    FROM INSERTED AS [NEW]
        //    WHERE NOT EXISTS(
        //        SELECT 1 FROM [{targetTable}] AS [t] WHERE {string.Join(" AND ", columns.Select(x => $"[t].[{x.Field}] = [NEW].[{x.Field}]"))}
        //    )
        //END;
        //";
        //        }
        public string CreateSqlite(string name, string sourceTable, IEnumerable<TriggerField> columns,string idColumn)
        {
            return $@"
        CREATE TRIGGER [{name}{Insert}] AFTER INSERT ON [{sourceTable}]
        BEGIN
            UPDATE [{sourceTable}] SET {string.Join(", ", columns.Select(x => $"`{x.Field}` = {x.Raw}"))}
            WHERE [{idColumn}]=NEW.[{idColumn}];
        END;
        CREATE TRIGGER [{name}{Update}] AFTER UPDATE ON [{sourceTable}]
        BEGIN
            UPDATE [{sourceTable}] SET {string.Join(", ", columns.Select(x => $"`{x.Field}` = {x.Raw}"))}
            WHERE [{idColumn}]=NEW.[{idColumn}];
        END;
    
        ";
        }
        //        private string GetNpgSqlFunName(string name)
        //        {
        //            return "fun_" + name.Replace('-', '_');
        //        }
        //        public string CreatePostgreSQL(string name, string sourceTable, string targetTable, IEnumerable<TriggerField> columns)
        //        {
        //            var funName = GetNpgSqlFunName(name);
        //            return $@"CREATE FUNCTION {funName}() RETURNS TRIGGER AS $$
        //BEGIN
        //  INSERT INTO ""{targetTable}"" ({string.Join(",", columns.Select(x => $"\"{x.Field}\""))}) VALUES ({string.Join(",", columns.Select(x => x.Raw))})
        //    ON CONFLICT DO NOTHING;
        //  RETURN NEW;
        //END;
        //$$ LANGUAGE plpgsql;

        //CREATE TRIGGER ""{name}"" AFTER INSERT ON ""{sourceTable}""
        //FOR EACH ROW
        //EXECUTE FUNCTION {funName}();
        //";
        //        }
        public string DropMySqlRaw(string name)
        {
            return $@"DROP TRIGGER IF EXISTS `{name}`;";
        }
        public string DropSqliteRaw(string name)
        {
            return $@"DROP TRIGGER IF EXISTS `{name}`;";
        }
        //        public string DropSqlServer(string name, string sourceTable)
        //        {
        //            return $@"DROP TRIGGER IF EXISTS [{name}];";
        //        }
        //        public string DropSqlite(string name)
        //        {
        //            return $@"DROP TRIGGER IF EXISTS [{name}];";
        //        }
        //        public string DropPostgreSQL(string name, string sourceTable)
        //        {
        //            var funName = GetNpgSqlFunName(name);
        //            return $@"
        //DROP TRIGGER IF EXISTS ""{name}"" ON ""{sourceTable}"";
        //DROP FUNCTION IF EXISTS {funName}();
        //";
        //        }
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
    SELECT {string.Join(",", columns.Select(x => $"[NEW].[{x.Field}]"))}
    FROM INSERTED AS [NEW]
    WHERE NOT EXISTS(
        SELECT 1 FROM [{targetTable}] AS [t] WHERE {string.Join(" AND ", columns.Select(x => $"[t].[{x.Field}] = [NEW].[{x.Field}]"))}
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
