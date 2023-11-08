using DatabaseSchemaReader;
using DuckDB.NET.Data;
using FastBIRe.Farm;
using System.Data;

namespace FastBIRe.AP.DuckDB
{
    public class DuckFarmWarehouse : FarmWarehouse
    {
        private readonly DuckDBConnection duckDBConnection;
        public DuckFarmWarehouse(IDbScriptExecuter scriptExecuter)
            : base(scriptExecuter)
        {
            duckDBConnection = (DuckDBConnection)scriptExecuter.Connection;
        }

        public DuckFarmWarehouse(IDbScriptExecuter scriptExecuter, DatabaseReader databaseReader)
            : base(scriptExecuter, databaseReader)
        {
            duckDBConnection = (DuckDBConnection)scriptExecuter.Connection;
        }
        public override Task<int> InsertAsync(string tableName, IEnumerable<string> columnNames, IEnumerable<IEnumerable<object?>> values, CancellationToken token = default)
        {
            var count = 0;
            using (var appender = duckDBConnection.CreateAppender(tableName))
            {
                foreach (var row in values)
                {
                    var rowAppender = appender.CreateRow();
                    foreach (var item in row)
                    {
                        DuckAppendHelper.Add(rowAppender, item);
                    }
                    rowAppender.EndRow();
                    count++;
                }
            }
            return Task.FromResult(count);
        }
        public override Task InsertAsync(string tableName, IEnumerable<string> columnNames, IEnumerable<object?> values, CancellationToken token = default)
        {
            using (var appender = duckDBConnection.CreateAppender(tableName))
            {
                var row = appender.CreateRow();
                foreach (var item in values)
                {
                    DuckAppendHelper.Add(row, item);
                }
                row.EndRow();
            }
            return Task.CompletedTask;
        }
        public override Task<int> SyncDataAsync(string tableName, IEnumerable<string> columnNames, IDataReader reader, int batchSize, CancellationToken token = default)
        {
            var res = 0;
            var fetcher = DuckAppendHelper.BuildFetcher(reader);
            using (var appender = duckDBConnection.CreateAppender(tableName))
            {
                var fieldCount = reader.FieldCount;
                while (reader.Read())
                {
                    var row = appender.CreateRow();
                    for (int i = 0; i < fieldCount; i++)
                    {
                        fetcher[i](row, reader);
                    }
                    row.EndRow();
                    res++;
                }
            }
            return Task.FromResult(res);
        }

    }
}