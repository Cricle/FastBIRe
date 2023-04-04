using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public class TableHelper
    {
        public TableHelper(SqlType sqlType)
        {
            SqlType = sqlType;
        }

        public SqlType SqlType { get; }

        public string CreateIndex(string name, string table, params string[] fields)
        {
            return $"CREATE INDEX {Wrap(name)} ON {Wrap(table)} ({string.Join(",", fields.Select(x => Wrap(x)))});";
        }

        private string Wrap(string input)
        {
            switch (SqlType)
            {
                case SqlType.MySql:
                    return $"`{input}`";
                case SqlType.PostgreSql:
                case SqlType.SQLite:
                    return $"\"{input}\"";
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    return $"[{input}]";
                default:
                    return input;
            }
        }

        public string DropIndex(string name, string table)
        {
            switch (SqlType)
            {
                case SqlType.MySql:
                    return $"DROP INDEX `{name}` ON `{table}`;";
                case SqlType.SqlServer:
                    return $"DROP INDEX [{table}].[{name}];";
                case SqlType.SQLite:
                case SqlType.PostgreSql:
                    return $"DROP INDEX \"{name}\";";
                default:
                    throw new NotSupportedException(SqlType.ToString());
            }
        }

        public string CreateDropTable(string table)
        {
            return $"DROP TABLE {Wrap(table)};";
        }

        public string CreateTable(string table, IEnumerable<SourceTableColumnDefine> columns)
        {
            var source = @$"
CREATE TABLE {Wrap(table)} (
    {string.Join(",\n    ", columns.Select(x => $"{Wrap(x.Field)} {x.Type}"))},
    PRIMARY KEY ({string.Join(",", columns.Select(x => Wrap(x.Field)))})
);";
            return source;
        }

        public string? AlterIdToAutoInc(string table, string idColumn, string dbType)
        {
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    var pkName = $"PK_{table}_{idColumn}";
                    return @$"
ALTER TABLE [{table}] DROP CONSTRAINT [{pkName}];
ALTER TABLE [{table}] DROP COLUMN [{idColumn}];
ALTER TABLE [{table}] ADD [{idColumn}] {dbType} IDENTITY(1,1) CONSTRAINT [{pkName}] PRIMARY KEY;";
                case SqlType.MySql:
                    return $"ALTER TABLE `{table}` MODIFY COLUMN `{idColumn}` {dbType} AUTO_INCREMENT;";
                case SqlType.PostgreSql:
                    return @$"
ALTER TABLE ""{table}"" DROP COLUMN ""{idColumn}"";
ALTER TABLE ""{table}"" ADD COLUMN ""{idColumn}"" SERIAL PRIMARY KEY;";
                case SqlType.Oracle:
                case SqlType.SQLite:
                default:
                    return null;
            }
        }
        public string? RenameColumn(string table,string oldName, string newName,string type) 
        {
            switch (SqlType)
            {
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                    return $"EXEC sp_rename '{Wrap(table)}.{Wrap(oldName)}', '{Wrap(newName)}', 'COLUMN';";
                case SqlType.MySql:
                    return $"ALTER TABLE {Wrap(table)} CHANGE COLUMN {Wrap(oldName)} {Wrap(newName)} {type};";
                case SqlType.Oracle:
                case SqlType.PostgreSql:
                case SqlType.SQLite:
                    return $"ALTER TABLE {Wrap(table)} RENAME COLUMN {Wrap(oldName)} TO {Wrap(newName)};";
                case SqlType.Db2:
                default:
                    return null;
            }
        }
        public string? AlterColumnType(string table,string column,string newType)
        {
            switch (SqlType)
            {
                case SqlType.Oracle:
                    return $"ALTER TABLE {Wrap(table)} MODIFY COLUMN {Wrap(column)} {newType};";
                case SqlType.SqlServerCe:
                case SqlType.SqlServer:
                case SqlType.MySql:
                    return $"ALTER TABLE {Wrap(table)} ALTER COLUMN {Wrap(column)} {newType};";
                case SqlType.SQLite:
                    return $@"
ALTER TABLE {Wrap(table)} RENAME TO temp_table_name;
CREATE TABLE {Wrap(table)} ({Wrap(column)} {newType});
INSERT INTO {Wrap(table)} SELECT * FROM temp_table_name;
DROP TABLE temp_table_name;";
                case SqlType.PostgreSql:
                    return $"ALTER TABLE {Wrap(table)} ALTER COLUMN {Wrap(column)} TYPE {newType} USING {Wrap(column)}::{newType};";
                case SqlType.Db2:
                default:
                    return null;
            }
        }
    }
}
