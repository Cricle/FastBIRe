using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;

namespace FastBIRe
{
    public class TableHelper
    {
        public static readonly TableHelper MySql = new TableHelper(FSqlType.MySql);
        public static readonly TableHelper SqlServer = new TableHelper(FSqlType.SqlServer);
        public static readonly TableHelper MariaDB = MySql;
        public static readonly TableHelper Sqlite = new TableHelper(FSqlType.SQLite);
        public static readonly TableHelper Oracle = new TableHelper(FSqlType.Oracle);
        public static readonly TableHelper PostgreSql = new TableHelper(FSqlType.PostgreSql);
        public static readonly TableHelper DuckDB = new TableHelper(FSqlType.DuckDB);

        private static readonly char[] goLineTrimChars = new char[] { ' ', ';', '\r', '\n' };

        public static IList<string> SplitSqlServerByGo(string input)
        {
            var sps = input.Split('\n');
            var lst = new List<string>();
            var quto = false;
            var start = 0;
            for (int i = 0; i < sps.Length; i++)
            {
                var sp = sps[i];
                if (sp.TrimStart().StartsWith("--"))
                {
                    continue;
                }
                var q = sp.Count(x => x == '\'');
                if (q % 2 != 0)
                {
                    quto = !quto;
                }
                if (!quto)
                {
                    if (sp.Trim(goLineTrimChars).Equals("GO", StringComparison.OrdinalIgnoreCase))
                    {
                        lst.Add(string.Join(Environment.NewLine, sps.Skip(start).Take(i - start)));
                        start = i + 1;
                    }
                }
            }
            return lst;
        }

        public TableHelper(FSqlType sqlType)
        {
            SqlType = sqlType;
        }

        public FSqlType SqlType { get; }

        public string CreateIndex(string name, string table, string[] fields, bool[]? descs = null)
        {
            var fs = new List<string>();
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (descs == null || descs.Length <= i)
                {
                    fs.Add(SqlType.Wrap(field));
                }
                else
                {
                    fs.Add($"{SqlType.Wrap(field)} {(descs[i] ? "DESC" : "ASC")}");
                }
            }
            return $"CREATE INDEX {SqlType.Wrap(name)} ON {SqlType.Wrap(table)} ({string.Join(",", fs)});";
        }

        public string DropIndex(string name, string table)
        {
            switch (SqlType)
            {
                case FSqlType.MySql:
                    return $"DROP INDEX {SqlType.Wrap(name)} ON {SqlType.Wrap(table)};";
                case FSqlType.SqlServer:
                    return $"DROP INDEX {SqlType.Wrap(table)}.{SqlType.Wrap(name)};";
                case FSqlType.SQLite:
                case FSqlType.PostgreSql:
                case FSqlType.DuckDB:
                    return $"DROP INDEX {SqlType.Wrap(name)};";
                default:
                    return string.Empty;
            }
        }
        public string Pagging(int? skip, int? take)
        {
            if (skip == null && take == null)
            {
                return string.Empty;
            }
            switch (SqlType)
            {
                case FSqlType.SqlServerCe:
                case FSqlType.SqlServer:
                    {
                        if (skip != null && take != null)
                        {
                            return $"OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY";
                        }
                        if (skip != null)
                        {
                            return $"OFFSET {skip} ROWS";
                        }
                        return $"OFFSET 0 ROWS FETCH NEXT {take} ROWS ONLY";
                    }
                case FSqlType.MySql:
                    {
                        if (skip != null && take != null)
                        {
                            return $"LIMIT {skip}, {take}";
                        }
                        if (skip != null)
                        {
                            return $"LIMIT {skip}";
                        }
                        return $"LIMIT 0, {take}";
                    }
                case FSqlType.SQLite:
                    {
                        if (skip != null && take != null)
                        {
                            return $"LIMIT {take} OFFSET {skip}";
                        }
                        if (skip != null)
                        {
                            return $"LIMIT -1 OFFSET {skip}";
                        }
                        return $"LIMIT {take} OFFSET 0";
                    }
                case FSqlType.DuckDB:
                case FSqlType.PostgreSql:
                    {
                        if (skip != null && take != null)
                        {
                            return $"OFFSET {skip} LIMIT {take}";
                        }
                        if (skip != null)
                        {
                            return $"OFFSET {skip}";
                        }
                        return $"LIMIT {take}";
                    }
                default:
                    throw new NotSupportedException(SqlType.ToString());
            }
        }
        private static readonly string PostgreSqlTableCreateFunctionScripts;
        private static readonly string SqlServerTableCreateProcedureScripts;
        static TableHelper()
        {
            using (var stream = typeof(TableHelper).Assembly.GetManifestResourceStream("FastBIRe.Resources.pg_get_tabledef.sql"))
            using (var sr = new StreamReader(stream!))
            {
                PostgreSqlTableCreateFunctionScripts = sr.ReadToEnd();
            }
            using (var stream = typeof(TableHelper).Assembly.GetManifestResourceStream("FastBIRe.Resources.sp_getddl.sql"))
            using (var sr = new StreamReader(stream!))
            {
                SqlServerTableCreateProcedureScripts = sr.ReadToEnd();
            }
        }
        public string? GetEnableCheck()
        {
            switch (SqlType)
            {
                case FSqlType.SqlServerCe:
                case FSqlType.SqlServer:
                    return "EXEC sp_msforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';";
                case FSqlType.MySql:
                    return "SET FOREIGN_KEY_CHECKS = 1;";
                case FSqlType.SQLite:
                    return "PRAGMA foreign_keys=on;";
                case FSqlType.PostgreSql:
                    return "SET session_replication_role = origin;";
                case FSqlType.DuckDB:
                case FSqlType.Oracle:
                case FSqlType.Db2:
                default:
                    return null;
            }
        }
        public string? GetIgnoreCheck()
        {
            switch (SqlType)
            {
                case FSqlType.SqlServerCe:
                case FSqlType.SqlServer:
                    return "EXEC sp_msforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';";
                case FSqlType.MySql:
                    return "SET FOREIGN_KEY_CHECKS=0;";
                case FSqlType.SQLite:
                    return "PRAGMA foreign_keys=off;";
                case FSqlType.PostgreSql:
                    return "SET session_replication_role = replica;";
                case FSqlType.DuckDB:
                case FSqlType.Oracle:
                case FSqlType.Db2:
                default:
                    return null;
            }
        }
        public static string GetPgDDLFunctionScripts()
        {
            return PostgreSqlTableCreateFunctionScripts;
        }
        public static string GetSqlServerDDLFunctionScripts()
        {
            return SqlServerTableCreateProcedureScripts;
        }
        public record class DumpTableResult
        {
            public DumpTableResult(string dropScript, string createScripts, TableRef tableRef)
            {
                DropScript = dropScript;
                CreateScripts = createScripts;
                TableRef = tableRef;
            }

            public string DropScript { get; }

            public string CreateScripts { get;}

            public TableRef TableRef { get; }
        }
        public async Task DumpDatabaseCreateAsync(IDbScriptExecuter scriptExecuter, Action<DumpTableResult> scriptReceiver, Predicate<TableRef>? tableFilter = null, bool includeSchema = false,bool sortTheTableByRef=false)
        {
            var reader = new DatabaseReader(scriptExecuter.Connection) { Owner = scriptExecuter.Connection.Database };
            var tables = reader.AllTables();
            var refs = sortTheTableByRef ? DatabaseHelper.SortTable(tables) : TableRef.CreateRange(tables);
            var dbAdapter = SqlType.GetDatabaseCreateAdapter()!;
            var ddl = new DdlGeneratorFactory((SqlType)SqlType);
            for (int i = 0; i < refs.Count; i++)
            {
                var @ref = refs[i];
                if (tableFilter != null && !tableFilter(@ref))
                {
                    continue;
                }
                var dropScript = (dbAdapter.DropTableIfExists(@ref.Table.Name));
                var createScript = (await DumpTableCreateAsync(@ref.Table.Name,
                    scriptExecuter,
                    includeSchema: includeSchema,
                    ddlGeneratorFactory: ddl,
                    databaseReader: reader) + ";");
                scriptReceiver(new DumpTableResult(dropScript, createScript,@ref));
            }
        }
        public async Task DumpDatabaseCreateAsync(IDbScriptExecuter scriptExecuter, StreamWriter writer, Predicate<TableRef>? tableFilter = null, bool includeSchema = false, bool sortTheTableByRef = false)
        {
            await DumpDatabaseCreateAsync(scriptExecuter, s =>
            {
                writer.WriteLine(s.DropScript);
                writer.WriteLine(s.CreateScripts);
            }, tableFilter, includeSchema: includeSchema, sortTheTableByRef: sortTheTableByRef);
        }
        public async Task<IList<DumpTableResult>?> DumpDatabaseCreateAsync(IDbScriptExecuter scriptExecuter, Predicate<TableRef>? tableFilter = null, bool includeSchema = false, bool sortTheTableByRef = false)
        {
            var res = new List<DumpTableResult>();
            await DumpDatabaseCreateAsync(scriptExecuter, s => res.Add(s), tableFilter, includeSchema: includeSchema, sortTheTableByRef: sortTheTableByRef);
            return res;
        }
        public Task<string?> DumpTableCreateAsync(string table,
            IDbScriptExecuter scriptExecuter,
            bool includeSchema = false,
            DdlGeneratorFactory? ddlGeneratorFactory = null,
            DatabaseReader? databaseReader = null)
        {
            var sql = DumpTableCreateSql(table);
            if (sql == null)
            {
                databaseReader ??= new DatabaseReader(scriptExecuter.Connection) { Owner = scriptExecuter.Connection.Database };
                ddlGeneratorFactory ??= new DdlGeneratorFactory((SqlType)SqlType);
                var tableDef = databaseReader.Table(table);
                string? ddl = null;
                if (tableDef != null)
                {
                    var gen = ddlGeneratorFactory.TableGenerator(tableDef);
                    gen.IncludeSchema = includeSchema;
                    ddl = gen.Write();
                }
                return Task.FromResult(ddl);
            }
            return scriptExecuter.ReadResultAsync(sql, (o, e) =>
            {
                string? result = null;
                if (e.Reader.Read())
                {
                    switch (SqlType)
                    {
                        case FSqlType.MySql:
                            result = e.Reader.GetString(1);
                            break;
                        case FSqlType.DuckDB:
                        case FSqlType.SQLite:
                        case FSqlType.PostgreSql:
                            result = e.Reader.GetString(0);
                            break;
                        case FSqlType.SqlServer:
                        case FSqlType.SqlServerCe:
                            result = e.Reader.GetString(0);
                            break;
                        case FSqlType.Oracle:
                        case FSqlType.Db2:
                        default:
                            break;
                    }
                }
                return Task.FromResult(result);
            });
        }
        public static Task<bool> PgSqlHasDDLFunctionDefAsync(IScriptExecuter scriptExecuter, CancellationToken token = default)
        {
            return PgSqlHasFunctionDefAsync("pg_get_tabledef", scriptExecuter, token);
        }
        public static Task<bool> SqlServerHasDDLFunctionDefAsync(IScriptExecuter scriptExecuter, CancellationToken token = default)
        {
            return PgSqlHasFunctionDefAsync("sp_GetDDL", scriptExecuter, token);
        }
        public static Task<bool> PgSqlHasFunctionDefAsync(string funName, IScriptExecuter scriptExecuter, CancellationToken token = default)
        {
            return scriptExecuter.ExistsAsync($"SELECT 1 FROM pg_proc WHERE proname = '{funName}';", token: token);
        }

        public static Task<bool> SqlServerHasProcedureDefAsync(string funName, IScriptExecuter scriptExecuter, CancellationToken token = default)
        {
            return scriptExecuter.ExistsAsync($"SELECT 1 FROM sys.procedures WHERE name = '{funName}';", token: token);
        }

        public string? DumpTableCreateSql(string table)
        {
            switch (SqlType)
            {
                case FSqlType.MySql:
                    return $"SHOW CREATE TABLE `{table}`;";
                case FSqlType.DuckDB:
                case FSqlType.SQLite:
                    return $"SELECT sql FROM sqlite_master WHERE type = 'table' AND name = '{table}';";
                case FSqlType.PostgreSql:
                    return $"SELECT pg_get_tabledef('public','{table}',false)";
                case FSqlType.SqlServerCe:
                case FSqlType.SqlServer:
                    return $"EXEC sp_GetDDL '{table}'";
                case FSqlType.Db2:
                case FSqlType.Oracle:
                default:
                    break;
            }
            return null;
        }

        public string Opimize(string table)
        {
            switch (SqlType)
            {
                case FSqlType.SqlServerCe:
                case FSqlType.SqlServer:
                    return $"ALTER INDEX ALL ON [{table}] REBUILD;";
                case FSqlType.MySql:
                    return $"OPTIMIZE TABLE `{table}`;";
                case FSqlType.SQLite:
                    return "VACUUM;";
                case FSqlType.DuckDB:
                case FSqlType.PostgreSql:
                    return $"VACUUM FULL \"{table}\";";
                case FSqlType.Oracle:
                    return $"ALTER TABLE TRUNCATE TABLE \"{table}\" MOVE;";
                case FSqlType.Db2:
                    return $"REORG TABLE \"{table}\";";
                default:
                    throw new NotSupportedException(SqlType.ToString());
            }
        }
        public string InsertUnionValues(string tableName, IEnumerable<string> names, IEnumerable<object> values, string? where)
        {
            var sql = $@"INSERT INTO {SqlType.Wrap(tableName)}({string.Join(",", names.Select(x => SqlType.Wrap(x)))}) {UnionValues(values)} ";
            if (!string.IsNullOrEmpty(where))
            {
                sql += "WHERE " + where;
            }
            return sql;
        }
        public string UnionValues(IEnumerable<object> values)
        {
            return $"SELECT {string.Join(",", values.Select(x => $"{SqlType.WrapValue(x)}"))}";
        }
        public string Truncate(string table)
        {
            switch (SqlType)
            {
                case FSqlType.SqlServerCe:
                case FSqlType.SqlServer:
                    return $"TRUNCATE TABLE [{table}];";
                case FSqlType.MySql:
                    return $"DELETE FROM `{table}`;";
                case FSqlType.SQLite:
                    return $"DELETE FROM `{table}`;";
                case FSqlType.PostgreSql:
                case FSqlType.DuckDB:
                    return $"TRUNCATE TABLE \"{table}\";";
                case FSqlType.Oracle:
                    return $"TRUNCATE TABLE \"{table}\";";
                case FSqlType.Db2:
                    return $"TRUNCATE TABLE \"{table}\";";
                default:
                    throw new NotSupportedException(SqlType.ToString());
            }
        }
        public string CreateView(string viewName, string script)
        {
            var qutoViewName = SqlType.Wrap(viewName);
            return $"CREATE VIEW {qutoViewName} AS {script};";
        }
        public string DropView(string viewName)
        {
            if (SqlType == FSqlType.Db2 || SqlType == FSqlType.Oracle)
            {
                return string.Empty;
            }
            var qutoViewName = SqlType.Wrap(viewName);
            switch (SqlType)
            {
                case FSqlType.SqlServerCe:
                case FSqlType.SqlServer:
                    return $"IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'{viewName}')) DROP VIEW {qutoViewName};";
                case FSqlType.MySql:
                case FSqlType.SQLite:
                case FSqlType.PostgreSql:
                    return $"DROP VIEW IF EXISTS {qutoViewName};";
                default:
                    return string.Empty;
            }
        }
    }
}
