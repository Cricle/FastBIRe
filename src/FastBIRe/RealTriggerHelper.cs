using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public class RealTriggerHelper
    {
        public static readonly RealTriggerHelper Instance = new RealTriggerHelper();

        public const string InsertTail = "_insert";
        public const string UpdateTail = "_update";

        public string? Drop(string name, string destTable, SqlType sqlType)
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
            var updateName = name + InsertTail;
            var insertName = name + UpdateTail;
            var helper = new MergeHelper(sqlType);
            helper.WhereItems = table.Columns.Where(x => !x.OnlySet && x.IsGroup)
                .Select(x => new WhereItem(x.Field, x.Raw, helper.ToRaw(x.Method, $"NEW.{helper.Wrap(x.Field)}", false)));
            var inserts = helper.CompileInsert(destTable, table);
            var updates=helper.CompileUpdate(destTable, table);
            return $@"
CREATE TRIGGER `{updateName}` AFTER UPDATE ON `{table.Table}`
FOR EACH ROW
BEGIN
    {inserts}
    {updates}
END;
CREATE TRIGGER `{insertName}` AFTER INSERT ON `{table.Table}`
FOR EACH ROW
BEGIN
    {inserts}
    {updates}
END;
";
        }
        public string CreateSqlServer(string name, string destTable, SourceTableDefine table, SqlType sqlType)
        {
            var updateName = name + InsertTail;
            var insertName = name + UpdateTail;
            var helper = new MergeHelper(sqlType);
            var opt=new CompileOptions { EffectTable= "INSERTED", IncludeEffectJoin=true };
            var inserts = helper.CompileInsert(destTable, table, opt);
            var updates = helper.CompileUpdate(destTable, table, opt);
            return $@"
CREATE TRIGGER [{insertName}] ON [{table.Table}] AFTER INSERT
AS
BEGIN
    {inserts}
    {updates}
END;
CREATE TRIGGER [{updateName}] ON [{table.Table}] AFTER UPDATE
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
            helper.WhereItems = table.Columns.Where(x => x.IsGroup)
                .Select(x => new WhereItem(x.Field, x.Raw, helper.ToRaw(x.Method, $"NEW.{helper.Wrap(x.Field)}", false)));
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
        
        private bool IsTimePartMethod(ToRawMethod method)
        {
            switch (method)
            {
                case ToRawMethod.Year:
                case ToRawMethod.Month:
                case ToRawMethod.Day:
                case ToRawMethod.Hour:
                case ToRawMethod.Minute:
                case ToRawMethod.Second:
                case ToRawMethod.Quarter:
                case ToRawMethod.Weak:
                    return true;
                default:
                    return false;
            }
        }
        public string CreatePostgreSQL(string name, string destTable, SourceTableDefine table, SqlType sqlType)
        {
            var updateName = name +UpdateTail;
            var insertName = name + InsertTail;
            var funName = PostgresqlHelper.GetFunName(name);
            var helper = new MergeHelper(sqlType);
            helper.WhereItems = table.Columns.Where(x => !x.OnlySet && x.IsGroup)
                .Select(x => new WhereItem(x.Field, x.Raw, helper.ToRaw(x.Method, $"NEW.{helper.Wrap(x.Field)}", false)));
            var inserts = helper.CompileInsert(destTable, table);
            var updates = helper.CompileUpdate(destTable, table);
            return $@"
CREATE FUNCTION {funName}() RETURNS TRIGGER AS $$
BEGIN
    {string.Join("\n",table.Columns.Where(x=>x.IsGroup&& IsTimePartMethod(x.Method)).Select(x=>$"NEW.\"{x.Field}\" := TO_CHAR(NEW.\"{x.Field}\", 'yyyy-MM-dd HH24:MI');"))}
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
            return $@"
DROP TRIGGER IF EXISTS `{name}{InsertTail}`;
DROP TRIGGER IF EXISTS `{name}{UpdateTail}`;";
        }
        public string DropSqlServer(string name)
        {
            return $@"
DROP TRIGGER IF EXISTS [{name}{InsertTail}];
DROP TRIGGER IF EXISTS [{name}{UpdateTail}];";
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
            var funName = PostgresqlHelper.GetFunName(name);
            return $@"
DROP TRIGGER IF EXISTS ""{name}{InsertTail}"" ON ""{sourceTable}"";
DROP TRIGGER IF EXISTS ""{name}{UpdateTail}"" ON ""{sourceTable}"";
DROP FUNCTION IF EXISTS {funName}();
";
        }

    }
}
