using DatabaseSchemaReader.DataSchema;
using FastBIRe.Timing;

namespace FastBIRe.Triggering
{
    public static class TriggerWriterEffectExtensions
    {
        public static IEnumerable<string> CreateExpand(this ITriggerWriter triggerWriter,SqlType sqlType,string name,TriggerTypes type,string table,string field, ITimeExpandHelper timeExpandHelper)
        {
            var body = string.Empty;
            var when = string.Empty;
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    {
                        break;
                    }
                case SqlType.MySql:
                    break;
                case SqlType.SQLite:
                    break;
                case SqlType.PostgreSql:
                    break;
                case SqlType.Db2:
                case SqlType.Oracle:
                default:
                    yield break;
            }
        }
        public static IEnumerable<string> CreateEffect(this ITriggerWriter triggerWriter,SqlType sqlType, string name, TriggerTypes type, string sourceTable,string targetTable, IEnumerable<EffectTriggerSettingItem> settingItems)
        {
            var body = string.Empty;
            var when = string.Empty;
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    {
                        yield return $@"
    SET NOCOUNT ON;
    INSERT INTO [{targetTable}] ({string.Join(",", settingItems.Select(x => $"[{x.Field}]"))})
    SELECT {string.Join(",", settingItems.Select(x => x.Raw))}
    FROM (
        SELECT {string.Join(",", settingItems.Select(x => $"{string.Format(x.RawFormat, $"[NEW].[{x.Field}]")} AS [{x.Field}]"))} 
        FROM INSERTED AS [NEW] 
        WHERE {string.Join(" AND ", settingItems.Select(x => x.Field).Distinct().Select(x => $"[NEW].[{x}] IS NOT NULL"))}
        GROUP BY {string.Join(",", settingItems.Select(x => string.Format(x.RawFormat, $"[NEW].[{x.Field}]")))}
        HAVING NOT EXISTS(
            SELECT 1 FROM [{targetTable}] AS [t] 
            WHERE {string.Join(" AND ", settingItems.Select(x => $"{string.Format(x.RawFormat, $"[t].[{x.Field}]")} = {string.Format(x.RawFormat, $"[NEW].[{x.Field}]")}"))}
        )
    ) AS [NEW];
";
                        break;
                    }
                case SqlType.MySql:
                    {
                        body = $@"
DECLARE has_row INT;
SELECT 1 INTO has_row FROM `{targetTable}` WHERE {string.Join(" AND ", settingItems.Select(x => $"(NEW.`{x.Field}` = {x.Raw} OR (NEW.`{x.Field}` IS NULL AND {x.Raw} IS NULL))"))} LIMIT 1;
IF has_row IS NULL THEN
    INSERT INTO `{targetTable}`({string.Join(",", settingItems.Select(x => $"`{x.Field}`"))}) VALUES({string.Join(",", settingItems.Select(x => x.Raw))});
END IF;
";
                        break;
                    }
                case SqlType.SQLite:
                    {
                        when = string.Join(" AND ", settingItems.Select(x => x.Field).Distinct().Select(x => $"[NEW].[{x}] IS NOT NULL"));
                        body = $@"INSERT OR IGNORE INTO [{targetTable}] ({string.Join(",", settingItems.Select(x => $"[{x.Field}]"))}) VALUES({string.Join(",", settingItems.Select(x => x.Raw))});";
                        break;
                    }
                case SqlType.PostgreSql:
                    {
                        when = string.Join(" AND ", settingItems.Select(x => x.Field).Distinct().Select(x => $"NEW.\"{x}\" IS NOT NULL"));
                        body = $@"INSERT INTO ""{targetTable}"" ({string.Join(",", settingItems.Select(x => $"\"{x.Field}\""))}) VALUES ({string.Join(",", settingItems.Select(x => x.Raw))})
    ON CONFLICT DO NOTHING;";
                        break;
                    }
                case SqlType.Db2:
                case SqlType.Oracle:
                default:
                    yield break;
            }
            foreach (var item in triggerWriter.Create(sqlType, name, type, sourceTable, body, when))
            {
                yield return item;
            }
        }
    }
}
