using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using System.Data.Common;

namespace FastBIRe
{
    public class TableWrapper
    {
        class TableColumnSnapshot : ITableColumnSnapshot
        {
            public string Name { get; }

            public int Index { get; }

            public string WrapName { get; }

            public TableColumnSnapshot(string name, int index, string wrapName)
            {
                Name = name;
                Index = index;
                WrapName = wrapName;
            }
        }
        public static TableWrapper Create(DbConnection connection, string tableName, Predicate<DatabaseColumn>? columnMarsk=null)
        {
            var reader = new DatabaseReader(connection) { Owner = connection.Database };
            var table = reader.Table(tableName);
            if (table == null)
            {
                throw new ArgumentException($"Table {tableName} not exists");
            }
            return FromMarsk(table, reader.SqlType!.Value, columnMarsk);
        }
        public static TableWrapper FromMarsk(DatabaseTable table, SqlType sqlType, Predicate<DatabaseColumn>? columnMarsk = null)
        {
            if (columnMarsk == null)
            {
                return new TableWrapper(table, sqlType, null);
            }
            var selectmask = new List<int>();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (columnMarsk(table.Columns[i]))
                {
                    selectmask.Add(i);
                }
            }
            return new TableWrapper(table, sqlType, selectmask);
        }
        public TableWrapper(DatabaseTable table, SqlType sqlType, IReadOnlyList<int>? selectMask)
        {
            Table = table;
            SqlType = sqlType;
            var sn = new TableColumnSnapshot[table.Columns.Count];
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var col = table.Columns[i];
                sn[i] = new TableColumnSnapshot(col.Name, i, SqlType.Wrap(col.Name));
            }
            columnSnapshots = sn;
            if (selectMask == null)
            {
                SelectsMask = columnSnapshots;
                ColumnNames = table.Columns.Select(x => x.Name).ToArray();
            }
            else
            {
                var names = new string[selectMask.Count];
                var sm = new ITableColumnSnapshot[selectMask.Count];
                for (int i = 0; i < selectMask.Count; i++)
                {
                    names[i] = table.Columns[selectMask[i]].Name;
                    sm[i] = columnSnapshots[selectMask[i]];
                }
                ColumnNames = names;
                SelectsMask = sm;
            }
            ColumnNameJoined = string.Join(", ", ColumnNames);
            var keys = new List<ITableColumnSnapshot>();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var col = table.Columns[i];
                if (col.IsPrimaryKey)
                {
                    keys.Add(columnSnapshots[i]);
                }
            }
            KeysMask = keys;
            WrapTableName = SqlType.Wrap(Table.Name);
            keyIndexSet = new HashSet<int>(KeysMask.Select(x => x.Index));
            SelectsExceptKeyMask = SelectsMask.Where(x => !keyIndexSet.Contains(x.Index)).ToArray();
        }
        private readonly HashSet<int> keyIndexSet;
        private readonly ITableColumnSnapshot[] columnSnapshots;

        public IReadOnlyList<ITableColumnSnapshot> ColumnSnapshots => columnSnapshots;

        public DatabaseTable Table { get; }

        public string WrapTableName { get; }

        public SqlType SqlType { get; }

        public IReadOnlyList<ITableColumnSnapshot> KeysMask { get; }

        public IReadOnlyList<ITableColumnSnapshot> SelectsMask { get; }

        public IReadOnlyList<ITableColumnSnapshot> SelectsExceptKeyMask { get; }

        public IReadOnlyList<string> ColumnNames { get; }

        public string ColumnNameJoined { get; }

        public string? CreateInsertOrUpdate(IEnumerable<object?> values)
        {
            var valueJoined = GetSelectValueJoined(values);
            switch (SqlType)
            {
                case SqlType.SqlServer:
                case SqlType.SqlServerCe:
                    {
                        var idMatchs = string.Join(" AND ", KeysMask.Select(x => $"target.[{x.Name}] = source.[{x.Name}]"));
                        return $@"MERGE {WrapTableName} AS target
USING (VALUES ({valueJoined})) AS source ({ColumnNameJoined})
ON {idMatchs}
WHEN MATCHED THEN
    UPDATE SET {string.Join(", ", SelectsExceptKeyMask.Select((x) => $"target.{x.Name} = source.{x.Name}"))}
WHEN NOT MATCHED THEN
    INSERT ({ColumnNameJoined})
    VALUES ({string.Join(", ", SelectsExceptKeyMask.Select((x) => $"source.{x.Name}"))});";
                    }
                case SqlType.MySql:
                    {
                        return $@"INSERT INTO {WrapTableName} ({ColumnNameJoined})
VALUES ({valueJoined})
ON DUPLICATE KEY UPDATE {string.Join(", ", SelectsExceptKeyMask.Select((x) => $"{x.Name} = VALUES({x.Name})"))};";
                    }
                case SqlType.SQLite:
                    return $@"INSERT OR REPLACE INTO {WrapTableName} ({ColumnNameJoined}) VALUES ({valueJoined});";
                case SqlType.PostgreSql:
                case SqlType.DuckDB:
                    return $@"INSERT INTO {WrapTableName} ({ColumnNameJoined})
VALUES ({valueJoined})
ON CONFLICT ({string.Join(", ", KeysMask.Select(x => x.Name))})
DO UPDATE SET {string.Join(", ", SelectsExceptKeyMask.Select((x) => $"{x.Name}=EXCLUDED.{x.Name}"))};";
                case SqlType.Oracle:
                case SqlType.Db2:
                default:
                    return null;
            }
        }
        public string GetSelectValueJoined(IEnumerable<object?> values)
        {
            return string.Join(", ", GetSelectValues(values).Select(x => SqlType.WrapValue(x)));
        }

        public IEnumerable<object?> GetSelectValues(IEnumerable<object?> values)
        {
            return SelectsMask.Select(x => values.ElementAt(x.Index));
        }
        public IEnumerable<object?> GetKeyValues(IEnumerable<object?> values)
        {
            return KeysMask.Select(x => values.ElementAt(x.Index));
        }

        public string CreateInsertSql(IEnumerable<object?> values)
        {
            return $"INSERT INTO {WrapTableName}({ColumnNameJoined}) VALUES ({GetSelectValueJoined(values)})";
        }
        public string CreateUpdateByKeySql(IEnumerable<object?> values)
        {
            var keyWhere = string.Join(" AND ", KeysMask.Select(x => $"{x.WrapName} = {SqlType.WrapValue(values.ElementAt(x.Index))}"));
            var sets = string.Join(", ", SelectsExceptKeyMask.Select(x => $"{x.WrapName} = {SqlType.WrapValue(values.ElementAt(x.Index))}"));
            return $"UPDATE {WrapTableName} SET {sets} WHERE {keyWhere}";
        }
        public string CreateDeleteByKeySql(IEnumerable<object?> values)
        {
            var keyWhere = string.Join(" AND ", KeysMask.Select(x => $"{x.WrapName} = {SqlType.WrapValue(values.ElementAt(x.Index))}"));
            return $"DELETE FROM {WrapTableName} WHERE {keyWhere}";
        }
        public string CreateDeleteByKeySql(IEnumerable<IEnumerable<object?>> valuess)
        {
            var ors = string.Join(" OR ", valuess.Select(x => $"({string.Join(" AND ", KeysMask.Select(k => $"{k.WrapName} = {SqlType.WrapValue(x.ElementAt(k.Index))}"))})"));
            return $"DELETE FROM {WrapTableName} WHERE {ors}";
        }
    }
}
