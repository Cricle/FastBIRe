using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;

namespace FastBIRe
{
    public class TableHelper
    {
        public static readonly TableHelper MySql = new TableHelper(SqlType.MySql);
        public static readonly TableHelper SqlServer = new TableHelper(SqlType.SqlServer);
        public static readonly TableHelper MariaDB = MySql;
        public static readonly TableHelper Sqlite = new TableHelper(SqlType.SQLite);
        public static readonly TableHelper Oracle = new TableHelper(SqlType.Oracle);
        public static readonly TableHelper PostgreSql = new TableHelper(SqlType.PostgreSql);
        public static readonly TableHelper DuckDB = new TableHelper(SqlType.DuckDB);

        public TableHelper(SqlType sqlType)
        {
            SqlType = sqlType;
        }

        public SqlType SqlType { get; }

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
                case SqlType.MySql:
                    return $"DROP INDEX {SqlType.Wrap(name)} ON {SqlType.Wrap(table)};";
                case SqlType.SqlServer:
                    return $"DROP INDEX {SqlType.Wrap(table)}.{SqlType.Wrap(name)};";
                case SqlType.SQLite:
                case SqlType.PostgreSql:
                case SqlType.DuckDB:
                    return $"DROP INDEX {SqlType.Wrap(name)};";
                default:
                    return string.Empty;
            }
        }
        public string CreateDropTable(string table)
        {
            return $"DROP TABLE {SqlType.Wrap(table)};";
        }
        public string Pagging(int? skip, int? take)
        {
            if (skip == null && take == null)
            {
                return string.Empty;
            }
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
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
                case SqlType.MySql:
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
                case SqlType.SQLite:
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
                case SqlType.DuckDB:
                case SqlType.PostgreSql:
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
        static TableHelper()
        {
            using (var stream = typeof(TableHelper).Assembly.GetManifestResourceStream("FastBIRe.Resources.pg_get_tabledef.sql"))
            using (var sr = new StreamReader(stream!))
            {
                PostgreSqlTableCreateFunctionScripts = sr.ReadToEnd();
            }
        }
        public string? GetEnableCheck()
        {
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return "EXEC sp_msforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';";
                case SqlType.MySql:
                    return "SET FOREIGN_KEY_CHECKS = 1;";
                case SqlType.SQLite:
                    return "PRAGMA foreign_keys=on;";
                case SqlType.PostgreSql:
                    return "SET session_replication_role = replica;";
                case SqlType.DuckDB:
                case SqlType.Oracle:
                case SqlType.Db2:
                default:
                    return null;
            }
        }
        public string? GetIgnoreCheck()
        {
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return "EXEC sp_msforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';";
                case SqlType.MySql:
                    return "SET FOREIGN_KEY_CHECKS=0;";
                case SqlType.SQLite:
                    return "PRAGMA foreign_keys=off;";
                case SqlType.PostgreSql:
                    return "SET session_replication_role = origin;";
                case SqlType.DuckDB:
                case SqlType.Oracle:
                case SqlType.Db2:
                default:
                    return null;
            }
        }
        public static string GetPgDDLFunctionScripts()
        {
            return PostgreSqlTableCreateFunctionScripts;
        }
        public Task<string?> DumpTableCreateAsync(string table, IDbScriptExecuter scriptExecuter)
        {
            var sql = DumpTableCreateSql(table);
            if (sql == null)
            {
                var reader = new DatabaseReader(scriptExecuter.Connection) { Owner = scriptExecuter.Connection.Database };
                var tableDef = reader.Table(table);
                string? ddl = null;
                if (tableDef != null)
                {
                    ddl = new DdlGeneratorFactory(SqlType).TableGenerator(tableDef).Write();
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
                        case SqlType.MySql:
                            result= e.Reader.GetString(1);
                            break;
                        case SqlType.DuckDB:
                        case SqlType.SQLite:
                        case SqlType.PostgreSql:
                            result= e.Reader.GetString(0);
                            break;
                        case SqlType.SqlServer:
                        case SqlType.SqlServerCe:
                        case SqlType.Oracle:
                        case SqlType.Db2:
                        default:
                            break;
                    }
                }
                return Task.FromResult(result);
            });
        }
        public string? DumpTableCreateSql(string table)
        {
            switch (SqlType)
            {
                case SqlType.MySql:
                    return $"SHOW CREATE TABLE `{table}`;";
                case SqlType.DuckDB:
                case SqlType.SQLite:
                    return $"SELECT sql FROM sqlite_master WHERE type = 'table' AND name = '{table}';";
                case SqlType.PostgreSql:
                    return $"SELECT pg_get_tabledef('public','{table}',false)";
                case SqlType.Db2:
                case SqlType.Oracle:
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                default:
                    break;
            }
            return null;
        }

        public string Opimize(string table)
        {
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return $"ALTER INDEX ALL ON [{table}] REBUILD;";
                case SqlType.MySql:
                    return $"OPTIMIZE TABLE `{table}`;";
                case SqlType.SQLite:
                    return "VACUUM;";
                case SqlType.PostgreSql:
                    return $"VACUUM FULL \"{table}\";";
                case SqlType.Oracle:
                    return $"ALTER TABLE TRUNCATE TABLE \"{table}\" MOVE;";
                case SqlType.Db2:
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
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return $"TRUNCATE TABLE [{table}];";
                case SqlType.MySql:
                    return $"DELETE FROM `{table}`;";
                case SqlType.SQLite:
                    return $"DELETE FROM `{table}`;";
                case SqlType.PostgreSql:
                case SqlType.DuckDB:
                    return $"TRUNCATE TABLE \"{table}\";";
                case SqlType.Oracle:
                    return $"TRUNCATE TABLE \"{table}\";";
                case SqlType.Db2:
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
            if (SqlType == SqlType.Db2 || SqlType == SqlType.Oracle)
            {
                return string.Empty;
            }
            var qutoViewName = SqlType.Wrap(viewName);
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return $"IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'{viewName}')) DROP VIEW {qutoViewName};";
                case SqlType.MySql:
                case SqlType.SQLite:
                case SqlType.PostgreSql:
                    return $"DROP VIEW IF EXISTS {qutoViewName};";
                default:
                    return string.Empty;
            }
        }
    }
}
