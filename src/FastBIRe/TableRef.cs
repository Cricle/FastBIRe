using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public class TableRef
    {
        public TableRef(DatabaseTable table)
        {
            Table = table;
            TableRefs = new List<TableRef>();
        }

        public DatabaseTable Table { get; }

        public List<TableRef> TableRefs { get; }

        public bool RefsNoRefs()
        {
            return TableRefs.All(x => x.TableRefs.Count == 0);
        }

        public static List<TableRef> CreateRange(IList<DatabaseTable> tables)
        {
            var res = tables.Select(x => new TableRef(x)).ToList();
            foreach (var item in res)
            {
                Create(item, res);
            }
            return res;
        }

        public static void Create(TableRef refs, List<TableRef> tableRefs)
        {
            //(A->B)->C
            foreach (var item in refs.Table.ForeignKeys)
            {
                var tableRef = tableRefs.FirstOrDefault(x => x.Table.Name == item.RefersToTable);
                if (tableRef != null)
                {
                    refs.TableRefs.Add(tableRef);
                }
            }
        }
        public override string ToString()
        {
            return $"{Table.Name}[{string.Join(",", Table.ForeignKeys.Select(x => $"{x.Name}({string.Join(",", x.Columns.Select(y => $"{x.RefersToTable}.{y}"))})"))}]";
        }
    }
}
