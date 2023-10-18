using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FastBIRe
{
    public class TableWrapper
    {
        class TableColumnSnapshot: ITableColumnSnapshot
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
        public static TableWrapper FromMarsk(DatabaseTable table, SqlType sqlType,Predicate<DatabaseColumn> predicate)
        {
            var selectmask = new List<int>();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (predicate(table.Columns[i]))
                {
                    selectmask.Add(i);
                }
            }
            return new TableWrapper(table,sqlType, selectmask);
        }
        public TableWrapper(DatabaseTable table, SqlType sqlType, IReadOnlyList<int>? selectMask)
        {
            Table = table;
            SqlType = sqlType;
            var sn = new TableColumnSnapshot[table.Columns.Count];
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var col = table.Columns[i];
                sn[i] = new TableColumnSnapshot(col.Name, i,SqlType.Wrap(col.Name));
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
        }
        private readonly HashSet<int> keyIndexSet;
        private readonly ITableColumnSnapshot[] columnSnapshots;

        public IReadOnlyList<ITableColumnSnapshot> ColumnSnapshots => columnSnapshots;

        public DatabaseTable Table { get; }

        public string WrapTableName { get; }

        public SqlType SqlType { get; }

        public IReadOnlyList<ITableColumnSnapshot> KeysMask { get; }

        public IReadOnlyList<ITableColumnSnapshot> SelectsMask { get; }

        public IReadOnlyList<string> ColumnNames { get; }

        public string ColumnNameJoined { get; }

        public string CreateInsertSql(IEnumerable<object> values) 
        {
            return $"INSERT INTO {WrapTableName}({ColumnNameJoined}) VALUES ({string.Join(", ", values.Select(x => SqlType.WrapValue(x)))})";
        }
        public string CreateUpdateByKeySql(IEnumerable<object> values)
        {
            var keyWhere = string.Join(" AND ", KeysMask.Select(x => $"{x.WrapName} = {SqlType.WrapValue(values.ElementAt(x.Index))}"));
            var sets = string.Join(", ", SelectsMask.Where(x => !keyIndexSet.Contains(x.Index)).Select(x => $"{x.WrapName} = {SqlType.WrapValue(values.ElementAt(x.Index))}"));
            return $"UPDATE {WrapTableName} SET {sets} WHERE {keyWhere}";
        }
        public string CreateDeleteByKeySql(IEnumerable<object> values)
        {
            var keyWhere = string.Join(" AND ", KeysMask.Select(x => $"{x.WrapName} = {SqlType.WrapValue(values.ElementAt(x.Index))}"));
            return $"DELETE {WrapTableName} WHERE {keyWhere}";
        }
        public string CreateDeleteByKeySql(IEnumerable<IEnumerable<object>> valuess)
        {
            var ors = string.Join(" OR ", valuess.Select(x => $"({string.Join(" AND ", KeysMask.Select(k => $"{k.WrapName} = {SqlType.WrapValue(x.ElementAt(k.Index))}"))})"));
            return $"DELETE FROM {WrapTableName} WHERE {ors}";
        }
    }
}
