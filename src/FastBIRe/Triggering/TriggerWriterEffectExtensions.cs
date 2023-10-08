using DatabaseSchemaReader.DataSchema;
using FastBIRe.Timing;

namespace FastBIRe.Triggering
{
    public static class TriggerWriterEffectExtensions
    {
        public static IEnumerable<string> CreateTimeExpand(this ITriggerWriter triggerWriter,
            SqlType sqlType,
            string name,
            TriggerTypes type,
            DatabaseTable table,
            IEnumerable<string> expandFields,
            ITimeExpandHelper? timeExpandHelper = null,
            TimeTypes timeTypes = TimeTypes.ExceptSecond,
            bool hasIdentity = false)
        {
            if (expandFields == null)
            {
                throw new ArgumentNullException(nameof(expandFields));
            }

            if (triggerWriter == null)
            {
                timeExpandHelper = new TimeExpandHelper(sqlType);
            }

            var exps = expandFields.SelectMany(x => timeExpandHelper!.Create(x, timeTypes)).Cast<IExpandResult>().ToList();
            return CreateExpand(triggerWriter!, sqlType, name, type, table, exps,hasIdentity);
        }
        public static IEnumerable<string> CreateExpand(this ITriggerWriter triggerWriter,
            SqlType sqlType,
            string name,
            TriggerTypes type,
            DatabaseTable table,
            IEnumerable<IExpandResult> expandResults,
            bool hasIdentity = false,
            IEnumerable<string>? autoNumberColumns=null)
        {
            var body = string.Empty;
            var when = string.Empty;
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    {
                        var identitySet = string.Empty;
                        var identitySetRestore = string.Empty;
                        if (hasIdentity)
                        {
                            identitySet = $"SET IDENTITY_INSERT [{table.Name}] ON;";
                            identitySetRestore = $"SET IDENTITY_INSERT [{table.Name}] OFF;";
                        }
                        //https://www.sqlservertutorial.net/sql-server-triggers/sql-server-instead-of-trigger
                        if (type == TriggerTypes.InsteadOfInsert)
                        {
                            var allColumnExceptExpands = table.Columns.Select(x => x.Name).Except(expandResults.Select(x => x.Name)).Distinct().ToList();
                            //Make insert column expressions
                            var insertColumnExpression = allColumnExceptExpands.Concat(expandResults.Select(x => x.Name))
                                .Select(x => $"[{x}]")
                                .ToList();
                            var dbcc =string.Empty;
                            if (hasIdentity)
                            {
                                identitySet += $@"
DECLARE @ident bigint;
DECLARE @incr bigint;
SET @incr = IDENT_INCR('{table.Name}');
SET @ident = IDENT_CURRENT('{table.Name}');
";
                                identitySetRestore = $@"
DECLARE @count bigint;
SELECT @count = COUNT(*) FROM INSERTED;
SET @ident = @ident + @incr * @count;
DBCC CHECKIDENT ('{table.Name}', RESEED, @ident);
{identitySetRestore}
";
                            }
                            //The expand field was let it to end insert, now make insert expressions exception
                            for (int i = 0; i < allColumnExceptExpands.Count; i++)
                            {
                                if (autoNumberColumns != null && autoNumberColumns.Contains(allColumnExceptExpands[i]))
                                {
                                    allColumnExceptExpands[i] = "@ident+(ROW_NUMBER() OVER(ORDER BY (SELECT NULL)))*@incr";//Replace to quto
                                }
                                else
                                {
                                    allColumnExceptExpands[i] = $"[{allColumnExceptExpands[i]}]";//Replace to quto
                                }
                            }
                            //Add expressions
                            allColumnExceptExpands.AddRange(expandResults.Select(x => $"({x.FormatExpression($"[NEW].[{x.OriginName}]")}) AS [{x.Name}]"));
                            body = $@"
{identitySet}
INSERT INTO [{table.Name}]({string.Join(",", insertColumnExpression)}) SELECT {string.Join(",", allColumnExceptExpands)} FROM INSERTED AS [NEW];
{identitySetRestore}";
                        }
                        else if (type == TriggerTypes.InsteadOfUpdate)
                        {
                            string where = string.Empty;
                            if (table.PrimaryKey == null)
                            {
                                //No PK, all columns except expands
                                throw new InvalidOperationException($"Sqlserver update trigger must has PK for update");
                            }
                            var setList = table.Columns.Select(x => x.Name)
                                .Except(expandResults.Select(x => x.Name))
                                .Where(x=> table.PrimaryKey==null||!table.PrimaryKey.Columns.Contains(x))
                                .Distinct()
                                .Select(x => $"[{x}] = [NEW].[{x}]")
                                .ToList();
                            var pkWhereList = table.PrimaryKey.Columns.Select(x => $"[{table.Name}].[{x}] = [NEW].[{x}]");
                            body = @$"
{identitySet}
UPDATE [{table.Name}] SET {string.Join(",", setList)} FROM INSERTED AS [NEW] WHERE {string.Join(" AND ", pkWhereList)};
{identitySetRestore}";
                        }
                        else
                        {
                            throw new InvalidOperationException($"At sqlserver use INSTEAD OF to do that, only InsteadOfInsert/InsteadOfUpdate can be used");
                        }
                        break;
                    }
                case SqlType.MySql:
                    {
                        body = string.Join("\n", expandResults.Select(x => $"SET NEW.`{x.Name}` = CASE WHEN NEW.`{x.OriginName}` IS NULL THEN NULL ELSE {x.FormatExpression($"NEW.`{x.OriginName}`")} END;"));
                        break;
                    }
                case SqlType.SQLite:
                    {
                        body = $"UPDATE `{table}` SET {string.Join(", ", expandResults.Select(x => $"`{x.Name}` = (CASE WHEN NEW.`{x.OriginName}` IS NULL THEN NULL ELSE {x.FormatExpression($"NEW.`{x.OriginName}`")} END)"))} WHERE `ROWID` = NEW.`ROWID`;";
                        break;
                    }
                case SqlType.PostgreSql:
                    {
                        body = string.Join("\n", expandResults.Select(x => $"NEW.\"{x.Name}\" = CASE WHEN NEW.\"{x.OriginName}\" IS NULL THEN NULL ELSE {x.FormatExpression($"NEW.\"{x.OriginName}\"")} END;"));
                        break;
                    }
                case SqlType.Db2:
                case SqlType.Oracle:
                default:
                    yield break;
            }
            foreach (var item in triggerWriter.Create(sqlType, name, type, table.Name, body, when))
            {
                yield return item;
            }
        }
        public static IEnumerable<string> CreateEffect(this ITriggerWriter triggerWriter,
            SqlType sqlType,
            string name,
            TriggerTypes type,
            string sourceTable,
            string targetTable,
            IEnumerable<EffectTriggerSettingItem> settingItems,
            bool hasIdentity = false)
        {
            var body = string.Empty;
            var when = string.Empty;
            switch (sqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    {
                        var identitySet = string.Empty;
                        var identitySetRestore = string.Empty;
                        if (hasIdentity)
                        {
                            identitySet = $"SET IDENTITY_INSERT [{targetTable}] ON;";
                            identitySetRestore = $"SET IDENTITY_INSERT [{targetTable}] OFF;";
                        }
                        body = $@"
    SET NOCOUNT ON;
    {identitySet}
    INSERT INTO [{targetTable}] ({string.Join(",", settingItems.Select(x => $"[{x.Field}]"))})
    SELECT {string.Join(",", settingItems.Select(x => x.Raw))}
    FROM (
        SELECT {string.Join(",", settingItems.Select(x => $"[NEW].[{x.Field}] AS [{x.Field}]"))} 
        FROM INSERTED AS [NEW] 
        GROUP BY {string.Join(",", settingItems.Select(x => $"[NEW].[{x.Field}]"))}
        HAVING NOT EXISTS(
            SELECT 1 FROM [{targetTable}] AS [t] 
            WHERE {string.Join(" AND ", settingItems.Select(x => $"([t].[{x.Field}] = [NEW].[{x.Field}] OR ([t].[{x.Field}] IS NULL AND [NEW].[{x.Field}] IS NULL))"))}
        )
    ) AS [NEW];
    {identitySetRestore}
";
                        break;
                    }
                case SqlType.MySql:
                    {
                        body = $@"
DECLARE has_row INT;
SELECT 1 INTO has_row FROM `{targetTable}` WHERE {string.Join(" AND ", settingItems.Select(x => $"(`{x.Field}` = {x.Raw} OR (`{x.Field}` IS NULL AND {x.Raw} IS NULL))"))} LIMIT 1;
IF has_row IS NULL THEN
    INSERT INTO `{targetTable}`({string.Join(",", settingItems.Select(x => $"`{x.Field}`"))}) VALUES({string.Join(",", settingItems.Select(x => x.Raw))});
END IF;
";
                        break;
                    }
                case SqlType.SQLite:
                    {
                        //TODO: when check
                        when = $"(NOT EXISTS (SELECT 1 FROM `{targetTable}` WHERE {string.Join(" AND ", settingItems.Select(x => x.Field).Distinct().Select(x => $"(`{x}` = `NEW`.`{x}` OR (`{x}` IS NULL AND `NEW`.`{x}` IS NULL))"))}))";
                        body = $@"INSERT INTO [{targetTable}] ({string.Join(",", settingItems.Select(x => $"[{x.Field}]"))}) VALUES({string.Join(",", settingItems.Select(x => x.Raw))});";
                        break;
                    }
                case SqlType.PostgreSql:
                    {
                        body = $@"
IF NOT EXISTS (SELECT 1 FROM ""{targetTable}"" WHERE {string.Join(" AND ", settingItems.Select(x => $"(\"{x.Field}\" = {x.Raw} OR (\"{x.Field}\" IS NULL AND {x.Raw} IS NULL))"))}) THEN
INSERT INTO ""{targetTable}""({string.Join(", ", settingItems.Select(x => x.Field))}) VALUES ({string.Join(", ", settingItems.Select(x => x.Raw))});
END IF;
";
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
