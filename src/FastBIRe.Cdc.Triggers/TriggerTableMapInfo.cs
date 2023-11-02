using DatabaseSchemaReader.DataSchema;
using FastBIRe.Cdc.Triggers;

namespace FastBIRe.Cdc.Mssql
{
    public class TriggerTableMapInfo:TableMapInfo
    {
        public TriggerTableMapInfo(object id, string databaseName, string tableName, DatabaseTable table,SqlType sqlType)
            : base(id, databaseName, tableName)
        {
            Table = table;
            ColumnNameJoined = string.Join(",", table.Columns.Select(x=>x.Name).Except(TriggerCdcManager.systemColumns).Select(x => sqlType.Wrap(x)));
        }

        public DatabaseTable Table { get; }

        public string ColumnNameJoined { get; }
    }
}
