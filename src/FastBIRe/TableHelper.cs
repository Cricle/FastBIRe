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
                    return $"DROP INDEX {SqlType.Wrap(name)};";
                default:
                    return string.Empty;
            }
        }
        public string CreateDropTable(string table)
        {
            return $"DROP TABLE {SqlType.Wrap(table)};";
        }
    }
}
