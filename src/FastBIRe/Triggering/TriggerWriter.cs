using DatabaseSchemaReader.DataSchema;
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

        public virtual string GetTriggerName(TriggerTypes type, FSqlType sqlType)
        {
            switch (type)
            {
                case TriggerTypes.BeforeInsert:
                    return "BEFORE INSERT";
                case TriggerTypes.AfterInsert:
                    return "AFTER INSERT";
                case TriggerTypes.BeforeUpdate:
                    return "BEFORE UPDATE";
                case TriggerTypes.AfterUpdate:
                    return "AFTER UPDATE";
                case TriggerTypes.BeforeMerge:
                    return "BEFORE MERGE";
                case TriggerTypes.AfterMerge:
                    return "AFTER MERGE";
                case TriggerTypes.BeforeDelete:
                    return "BEFORE DELETE";
                case TriggerTypes.AfterDelete:
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
        public virtual IEnumerable<string> Drop(FSqlType sqlType, string name, string table)
        {
            switch (sqlType)
            {
                case FSqlType.SqlServerCe:
                case FSqlType.SqlServer:
                    yield return $"DROP TRIGGER IF EXISTS [{name}];";
                    break;
                case FSqlType.MySql:
                    yield return $"DROP TRIGGER IF EXISTS `{name}`;";
                    break;
                case FSqlType.SQLite:
                    yield return $"DROP TRIGGER IF EXISTS `{name}`;";
                    break;
                case FSqlType.PostgreSql:
                    var pgFunName = PostgresqlFunctionNameGenerator.Create(new[] { name });
                    yield return $@"DO $$ 
BEGIN 
  IF EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = '{name}') THEN 
    DROP TRIGGER ""{name}"" ON ""{table}""; 
  END IF;
END $$;";
                    yield return $"DROP FUNCTION IF EXISTS \"{pgFunName}\";";
                    break;
                case FSqlType.Db2:
                case FSqlType.Oracle:
                default:
                    yield break;
            }
        }
        public virtual IEnumerable<string> Create(FSqlType sqlType, string name, TriggerTypes type, string table, string body, string? when)
        {
            var hasWhen = !string.IsNullOrWhiteSpace(when);
            switch (sqlType)
            {
                case FSqlType.SqlServer:
                case FSqlType.SqlServerCe:
                    {
                        if (hasWhen)
                        {
                            body = $@"IF ({when})
BEGIN
{body}
END";
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
                case FSqlType.MySql:
                    {
                        if (hasWhen)
                        {
                            body = $@"IF {when} THEN
{body}
END IF;";
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
                case FSqlType.SQLite:
                    {
                        var whenStr = string.Empty;
                        if (hasWhen)
                        {
                            whenStr = $"WHEN {when}";
                        }
                        yield return $@"
CREATE TRIGGER `{name}` {GetTriggerName(type, sqlType)} ON `{table}`
{whenStr}
BEGIN
{body}
END;
";
                        break;
                    }
                case FSqlType.PostgreSql:
                    {
                        var funName = PostgresqlFunctionNameGenerator.Create(new object[] { name });
                        string whenStr;
                        if (hasWhen)
                        {
                            whenStr = $@"IF {when} THEN
{body}
END IF;
RETURN NEW;";
                        }
                        else
                        {
                            whenStr = $@"{body}
RETURN NEW;";
                        }
                        yield return $@"
CREATE OR REPLACE FUNCTION {funName}() RETURNS TRIGGER AS $$
BEGIN
{whenStr}
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER ""{name}"" {GetTriggerName(type, sqlType)} ON ""{table}""
FOR EACH ROW
EXECUTE FUNCTION {funName}();
";
                        break;
                    }
                case FSqlType.Db2:
                case FSqlType.Oracle:
                default:
                    yield break;
            }
        }
    }
}
