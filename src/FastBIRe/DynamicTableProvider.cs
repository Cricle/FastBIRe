using DatabaseSchemaReader.DataSchema;

namespace FastBIRe
{
    public class DynamicTableProvider : List<DatabaseTable>, ITableProvider
    {
        public bool IsDynamic => true;

        public SqlType SqlType { get; }

        public DynamicTableProvider(SqlType sqlType)
        {
            SqlType = sqlType;
        }

        public DatabaseTable? GetTable(string name)
        {
            return Find(x => x.Name == name);
        }

        public bool HasTable(string name)
        {
            return FindIndex(x => x.Name == name) != -1;
        }
    }
}
