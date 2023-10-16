using System;
using System.Collections.Generic;
using System.Text;

namespace FastBIRe.Cdc
{
    public interface ITableMapInfo
    {
        object Id { get; }

        string DatabaseName { get; }

        string TableName { get; }
    }
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
