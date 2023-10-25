using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public static class DatabaseHelper
    {
        public static IList<TableRef> SortTable(IList<DatabaseTable> tables)
        {
            var res = TableRef.CreateRange(tables);

            var creating = new List<TableRef>();
            var lastCreating = 0;
            while (res.Count != creating.Count)
            {
                //I have created all the referenced tables
                var canCreat = res.Except(creating)
                    .Where(x => x.TableRefs.Count == 0 || x.TableRefs.All(y => creating.Contains(y)));
                creating.AddRange(canCreat);
                if (lastCreating == creating.Count)
                {
                    throw new InvalidOperationException($"The ref collection has any **Can Not Ref** tables, leaving {string.Join(",", string.Join(",", res.Except(creating).Select(x => x.Table.Name)))}");
                }
                lastCreating = creating.Count;
            }

            return creating;
        }

    }
}
