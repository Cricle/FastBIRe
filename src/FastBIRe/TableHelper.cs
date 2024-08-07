﻿using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public class TableHelper
    {
        public TableHelper(SqlType sqlType)
        {
            SqlType = sqlType;
        }

        public SqlType SqlType { get; }

        public string CreateIndex(string name, string table, string[] fields, bool[]? descs = null,bool unique=false)
        {
            var fs = new List<string>();
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (descs == null || descs.Length < i)
                {
                    fs.Add(Wrap(field));
                }
                else
                {
                    fs.Add($"{Wrap(field)} {(descs[i] ? "DESC" : "ASC")}");
                }
            }
            var uniqueStr = string.Empty;
            if (unique)
            {
                uniqueStr = "UNIQUE ";
            }
            return $"CREATE {uniqueStr}INDEX {Wrap(name)} ON {Wrap(table)} ({string.Join(",", fs)});";
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

    }
}
