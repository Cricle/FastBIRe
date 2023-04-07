using DatabaseSchemaReader.DataSchema;
using System;
using System.Collections.Generic;
using System.Text;

namespace FastBIRe
{
    public class RealTriggerHelper
    {
        public const string InsertTail = "_insert";
        public const string UpdateTail = "_update";

        public string? Drop(string name, string destTable, SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return DropSqlServer(name, destTable);
                case SqlType.MySql:
                    return DropMySql(name);
                case SqlType.SQLite:
                    return DropSqlite(name);
                case SqlType.PostgreSql:
                    return DropPostgreSQL(name, destTable);
                case SqlType.Oracle:
                case SqlType.Db2:
                default:
                    return null;
            }
        }
        public string? Create(string name, string destTable, SourceTableDefine table, SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return CreateSqlServer(name, destTable,table,sqlType);
                case SqlType.MySql:
                    return CreateMySql(name, destTable, table, sqlType);
                case SqlType.SQLite:
                    return CreateSqlite(name, destTable, table, sqlType);
                case SqlType.PostgreSql:
                    return CreatePostgreSQL(name, destTable, table  , sqlType);
                case SqlType.Oracle:
                case SqlType.Db2:
                default:
                    return null;
            }
        }

        public string CreateMySql(string name,string destTable,SourceTableDefine table, SqlType sqlType)
        {
            var helper = new MergeHelper(sqlType);
            helper.WhereItems = table.Columns.Where(x=>!x.OnlySet&&x.IsGroup).Select(x => new WhereItem(x.Field, x.Raw, $"NEW.{helper.Wrap(x.Field)}"));
            var inserts = helper.CompileInsert(destTable, table);
            var updates=helper.CompileUpdate(destTable, table);
            return $@"
CREATE TRIGGER `{name}` AFTER INSERT, UPDATE ON `{table.Table}`
FOR EACH ROW
BEGIN
    {inserts}
    {updates}
END;
";
        }
        public string CreateSqlServer(string name, string destTable, SourceTableDefine table, SqlType sqlType)
        {
            var helper = new MergeHelper(sqlType);
            var opt=new CompileOptions { EffectTable= "INSERTED", IncludeEffectJoin=true };
            var inserts = helper.CompileInsert(destTable, table, opt);
            var updates = helper.CompileUpdate(destTable, table, opt);
            return $@"
CREATE TRIGGER [{name}] ON [{table.Table}] AFTER INSERT, UPDATE
AS
BEGIN
    {inserts}
    {updates}
END;
";
        }
        public string CreateSqlite(string name, string destTable, SourceTableDefine table, SqlType sqlType)
        {
            var updateName = name + InsertTail;
            var insertName = name + UpdateTail;
            var helper = new MergeHelper(sqlType);
            helper.WhereItems = table.Columns.Where(x => !x.OnlySet && x.IsGroup).Select(x => new WhereItem(x.Field, x.Raw, $"NEW.{helper.Wrap(x.Field)}"));
            var inserts = helper.CompileInsert(destTable, table);
            var updates = helper.CompileUpdate(destTable, table);
            return $@"
CREATE TRIGGER [{insertName}] AFTER INSERT ON [{table.Table}]
BEGIN
    {inserts}
    {updates}
END;

CREATE TRIGGER [{updateName}] AFTER UPDATE ON [{table.Table}]
BEGIN
    {inserts}
    {updates}
END;
";
        }
        private string GetNpgSqlFunName(string name)
        {
            return "fun_" + name.Replace('-', '_');
        }
        public string CreatePostgreSQL(string name, string destTable, SourceTableDefine table, SqlType sqlType)
        {
            var updateName = name +UpdateTail;
            var insertName = name + InsertTail;
            var funName = GetNpgSqlFunName(name);
            var helper = new MergeHelper(sqlType);
            helper.WhereItems = table.Columns.Where(x => !x.OnlySet && x.IsGroup).Select(x => new WhereItem(x.Field, x.Raw, $"NEW.{helper.Wrap(x.Field)}"));
            var inserts = helper.CompileInsert(destTable, table);
            var updates = helper.CompileUpdate(destTable, table);
            return $@"
CREATE FUNCTION {funName}() RETURNS TRIGGER AS $$
BEGIN
  {inserts}
  {updates}
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER ""{insertName}"" AFTER INSERT ON ""{table.Table}""
FOR EACH ROW
EXECUTE FUNCTION {funName}();

CREATE TRIGGER ""{updateName}"" AFTER UPDATE ON ""{table.Table}""
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
            return $@"DROP TRIGGER IF EXISTS [{name}] ON [{sourceTable}];";
        }
        public string DropSqlite(string name)
        {
            return $@"
DROP TRIGGER IF EXISTS [{name}{InsertTail}];
DROP TRIGGER IF EXISTS [{name}{UpdateTail}];
";
        }
        public string DropPostgreSQL(string name, string sourceTable)
        {
            var funName = GetNpgSqlFunName(name);
            return $@"
DROP TRIGGER IF EXISTS ""{name}{InsertTail}"" ON ""{sourceTable}"";
DROP TRIGGER IF EXISTS ""{name}{UpdateTail}"" ON ""{sourceTable}"";
DROP FUNCTION IF EXISTS {funName}();
";
        }

    }
    public class TriggerHelper
    {
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
        public string? Create(string name, string sourceTable, string targetTable, IEnumerable<string> columns,SqlType sqlType)
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

        public string CreateMySql(string name,string sourceTable,string targetTable,IEnumerable<string> columns)
        {
            return $@"CREATE TRIGGER `{name}` AFTER INSERT ON `{sourceTable}`
FOR EACH ROW
BEGIN
	INSERT IGNORE INTO `{targetTable}`({string.Join(",", columns.Select(x=>$"`{x}`"))}) VALUES({string.Join(",", columns.Select(x => $"NEW.`{x}`"))});
END;
";
        }
        public string CreateSqlServer(string name, string sourceTable, string targetTable, IEnumerable<string> columns)
        {
            return $@"CREATE TRIGGER [{name}] ON [{sourceTable}] AFTER INSERT
AS
BEGIN
    INSERT INTO [{targetTable}] ({string.Join(",", columns.Select(x => $"[{x}]"))})
    SELECT {string.Join(",", columns.Select(x => $"new.[{x}]"))}
    FROM INSERTED AS [new]
    LEFT JOIN [{targetTable}] AS [t] ON {string.Join(" AND ",columns.Select(x=>$"[new].[{x}] = [t].[{x}]"))}
    WHERE {string.Join(" AND ", columns.Select(x => $"[t].[{x}] IS NULL"))}
END;
";
        }
        public string CreateSqlite(string name, string sourceTable, string targetTable, IEnumerable<string> columns)
        {
            return $@"CREATE TRIGGER [{name}] INSERT ON [{sourceTable}]
BEGIN
    INSERT OR IGNORE INTO [{targetTable}] ({string.Join(",", columns.Select(x => $"[{x}]"))}) VALUES({string.Join(",", columns.Select(x => $"NEW.[{x}]"))});
END;
";
        }
        private string GetNpgSqlFunName(string name)
        {
            return "fun_" + name.Replace('-', '_');
        }
        public string CreatePostgreSQL(string name, string sourceTable, string targetTable, IEnumerable<string> columns)
        {
            var funName = GetNpgSqlFunName(name);
            return $@"CREATE FUNCTION {funName}() RETURNS TRIGGER AS $$
BEGIN
  INSERT INTO ""{targetTable}"" ({string.Join(",", columns.Select(x => $"\"{x}\""))}) VALUES ({string.Join(",", columns.Select(x => $"NEW.\"{x}\""))})
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
            return $@"DROP TRIGGER IF EXISTS [{name}] ON [{sourceTable}];";
        }
        public string DropSqlite(string name)
        {
            return $@"DROP TRIGGER IF EXISTS [{name}];";
        }
        public string DropPostgreSQL(string name, string sourceTable)
        {
            var funName=GetNpgSqlFunName(name);
            return $@"
DROP TRIGGER IF EXISTS ""{name}"" ON ""{sourceTable}"";
DROP FUNCTION IF EXISTS {funName}();
";
        }
    }
}
