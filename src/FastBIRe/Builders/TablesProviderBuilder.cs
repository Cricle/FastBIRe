using DatabaseSchemaReader.DataSchema;

namespace FastBIRe.Builders
{
    public class TablesProviderBuilder : ITablesProviderBuilder
    {
        public TablesProviderBuilder(SqlType sqlType)
        {
            Tables = new List<DatabaseTable>();
            SqlType = sqlType;
        }

        public IList<DatabaseTable> Tables { get; }

        public SqlType SqlType { get; }

        public ITableBuilder GetTableBuilder(string name)
        {
            var table = Tables.FirstOrDefault(x => x.Name == name);
            if (table != null)
            {
                return new TableBuilder(table, SqlType);
            }
            table = new DatabaseTable { Name = name };
            Tables.Add(table);
            return new TableBuilder(table, SqlType);
        }
        public ITableProvider Build()
        {
            var dupNames = Tables.GroupBy(x => x.Name).Where(x => x.Skip(1).Any()).Select(x => x.Key).Distinct().ToList();
            if (dupNames.Count != 0)
            {
                throw new InvalidOperationException($"The table names {string.Join(",", dupNames)} was duplicated");
            }
            return new MapTableProvider(Tables.ToDictionary(x => x.Name), SqlType);
        }
    }
}
