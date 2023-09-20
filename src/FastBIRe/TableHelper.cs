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

        public string CreateIndex(string name, string table, string[] fields, bool[]? descs = null)
        {
            var fs = new List<string>();
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (descs == null || descs.Length <= i)
                {
                    fs.Add(Wrap(field));
                }
                else
                {
                    fs.Add($"{Wrap(field)} {(descs[i] ? "DESC" : "ASC")}");
                }
            }
            return $"CREATE INDEX {Wrap(name)} ON {Wrap(table)} ({string.Join(",", fs)});";
        }

        private string Wrap(string input)
        {
            return MergeHelper.Wrap(SqlType, input);
        }

        public string DropIndex(string name, string table)
        {
            switch (SqlType)
            {
                case SqlType.MySql:
                    return $"DROP INDEX {Wrap(name)} ON {Wrap(table)};";
                case SqlType.SqlServer:
                    return $"DROP INDEX {Wrap(table)}.{Wrap(name)};";
                case SqlType.SQLite:
                case SqlType.PostgreSql:
                    return $"DROP INDEX {Wrap(name)};";
                default:
                    return string.Empty;
            }
        }
        public string CreateDropTable(string table)
        {
            return $"DROP TABLE {Wrap(table)};";
        }
    }
}
