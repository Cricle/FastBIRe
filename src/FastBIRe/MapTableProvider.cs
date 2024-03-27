using DatabaseSchemaReader.DataSchema;
using System.Collections;

namespace FastBIRe
{
    public class MapTableProvider : ITableProvider
    {
        public MapTableProvider(IReadOnlyDictionary<string, DatabaseTable> tableMap, SqlType sqlType)
        {
            TableMap = tableMap;
            SqlType = sqlType;
        }

        public IReadOnlyDictionary<string, DatabaseTable> TableMap { get; }

        public bool IsDynamic => false;

        public SqlType SqlType { get; }

        public IEnumerator<DatabaseTable> GetEnumerator()
        {
            return TableMap.Values.GetEnumerator();
        }

        public DatabaseTable? GetTable(string name)
        {
            TableMap.TryGetValue(name, out var table);
            return table;
        }

        public bool HasTable(string name)
        {
            return TableMap.ContainsKey(name);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
