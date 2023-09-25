using DatabaseSchemaReader.DataSchema;
using FastBIRe.AAMode;
using FastBIRe.Naming;

namespace FastBIRe.Triggering
{
    public class TriggerWriter : ITriggerWriter
    {
        public static readonly INameGenerator DefaultPostgresqlFunctionNameGenerator = new RegexNameGenerator("fun_{0}");

        public static readonly TriggerWriter Default = new TriggerWriter(DefaultPostgresqlFunctionNameGenerator);

        public TriggerWriter(INameGenerator postgresqlFunctionNameGenerator)
        {
            PostgresqlFunctionNameGenerator = postgresqlFunctionNameGenerator ?? throw new ArgumentNullException(nameof(postgresqlFunctionNameGenerator));
        }

        public INameGenerator PostgresqlFunctionNameGenerator { get; }

        public virtual string GetTriggerName(TriggerTypes type, SqlType sqlType)
        {
            switch (type)
            {
                case TriggerTypes.BeforeInsert:
                    if (sqlType == SqlType.SQLite)
                        return "INSERT";
                    return "BEFORE INSERT";
                case TriggerTypes.AfterInsert:
                    if (sqlType == SqlType.SQLite)
                        return "INSERT";
                    return "AFTER INSERT";
                case TriggerTypes.BeforeUpdate:
                    if (sqlType == SqlType.SQLite)
                        return "UPDATE";
                    return "BEFORE UPDATE";
                case TriggerTypes.AfterUpdate:
                    if (sqlType == SqlType.SQLite)
                        return "UPDATE";
                    return "AFTER UPDATE";
                case TriggerTypes.BeforeMerge:
                    if (sqlType == SqlType.SQLite)
                        return "MERGE";
                    return "BEFORE MERGE";
                case TriggerTypes.AfterMerge:
                    if (sqlType == SqlType.SQLite)
                        return "MERGE";
                    return "AFTER MERGE";
                case TriggerTypes.BeforeDelete:
                    if (sqlType == SqlType.SQLite)
                        return "DELETE";
                    return "BEFORE DELETE";
                case TriggerTypes.AfterDelete:
                    if (sqlType == SqlType.SQLite)
                        return "DELETE";
                    return "AFTER DELETE";
                case TriggerTypes.InsteadOfInsert:
                    return "INSTEAD OF INSERT";
                case TriggerTypes.InsteadOfUpdate:
                    return "INSTEAD OF UPDATE";
                case TriggerTypes.InsteadOfDelete:
                    return "INSTEAD OF DELETE";
                case TriggerTypes.None:
                default:
                    return string.Empty;
            }
        }
        public virtual IEnumerable<string> Drop(SqlType sqlType, string name)
        {
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    yield return $"DROP TRIGGER IF EXISTS [{name}];";
                    break;
                case SqlType.MySql:
                    yield return $"DROP TRIGGER IF EXISTS `{name}`;";
                    break;
                case SqlType.SQLite:
                    yield return $"DROP TRIGGER IF EXISTS `{name}`;";
                    break;
                case SqlType.PostgreSql:
                    var pgFunName = PostgresqlFunctionNameGenerator.Create(new[] { name });
                    yield return $"DROP TRIGGER IF EXISTS \"{name}\";";
                    yield return $"DROP FUNCTION IF EXISTS \"{pgFunName}\";";
                    break;
                case SqlType.Db2:
                case SqlType.Oracle:
                default:
                    yield break;
            }
        }
        public virtual IEnumerable<string> Create(SqlType sqlType, string name, TriggerTypes type, string table, string body, string? when)
        {
            var hasWhen = !string.IsNullOrWhiteSpace(when);
            switch (sqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    {
                        if (hasWhen)
                        {
                            body = $@"
IF ({when})
BEGIN
{body}
END
";
                        }
                        yield return $@"
CREATE TRIGGER [{name}] ON [{table}] {GetTriggerName(type, sqlType)}
AS
BEGIN
{body}
END;
";
                        break;
                    }
                case SqlType.MySql:
                    {
                        if (hasWhen)
                        {
                            body = $@"
IF {when} THEN
{body}
END IF;
";
                        }
                        yield return $@"
CREATE TRIGGER `{name}` {GetTriggerName(type, sqlType)} ON `{table}`
FOR EACH ROW
BEGIN
{body}
END;
";
                        break;
                    }
                case SqlType.SQLite:
                    {
                        var whenStr = string.Empty;
                        if (hasWhen)
                        {
                            whenStr = $"WHEN {when}";
                        }
                        yield return $@"
CREATE TRIGGER `{name}` {GetTriggerName(type, sqlType)} ON [{table}]
{whenStr}
BEGIN
{body}
END;
";
                        break;
                    }
                case SqlType.PostgreSql:
                    {
                        var funName = PostgresqlFunctionNameGenerator.Create(new object[] { name });
                        string whenStr;
                        if (hasWhen)
                        {
                            whenStr = $@"
IF {when} THEN
{when}
END IF;
RETURN NEW;";
                        }
                        else
                        {
                            whenStr = "RETURN NEW;";
                        }
                        yield return $@"
CREATE OR REPLACE FUNCTION {funName}() RETURNS TRIGGER AS $$
BEGIN
{whenStr}
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE TRIGGER ""{name}"" {GetTriggerName(type, sqlType)} ON ""{table}""
FOR EACH ROW
EXECUTE FUNCTION {funName}();
";
                        break;
                    }
                case SqlType.Db2:
                case SqlType.Oracle:
                default:
                    yield break;
            }
        }
    }
}
