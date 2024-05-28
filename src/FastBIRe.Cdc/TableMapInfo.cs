namespace FastBIRe.Cdc
{
    public class TableMapInfo : ITableMapInfo
    {
        public TableMapInfo(object id, string databaseName, string tableName)
        {
            Id = id;
            DatabaseName = databaseName;
            TableName = tableName;
        }

        public object Id { get; }

        public string DatabaseName { get; }

        public string TableName { get; }
    }
}
