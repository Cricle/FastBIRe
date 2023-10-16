using DatabaseSchemaReader.DataSchema;
using System;
using System.Collections.Generic;
using System.Text;

namespace FastBIRe.Cdc.Mssql
{
    public class MssqlTableMapInfo : TableMapInfo
    {
        public MssqlTableMapInfo(object id, string databaseName, string tableName, DatabaseTable table)
            : base(id, databaseName, tableName)
        {
            Table = table;
            ColumnNameJoined = string.Join(",", table.Columns.Select(x => $"[{x.Name}]"));
        }

        public DatabaseTable Table { get; }

        public string ColumnNameJoined { get; }
    }
}
