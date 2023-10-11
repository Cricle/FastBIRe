using DatabaseSchemaReader.DataSchema;
using System.Data;

namespace FastBIRe.Farm
{
    public class FarmManager : IFarmManager
    {
        public FarmManager(DatabaseTable sourceTable, FarmWarehouse sourceFarmWarehouse, FarmWarehouse destFarmWarehouse)
        {
            SourceTable = sourceTable;
            SourceFarmWarehouse = sourceFarmWarehouse;
            DestFarmWarehouse = destFarmWarehouse;
        }

        public DatabaseTable SourceTable { get; }

        public string TableName => SourceTable.Name;

        public IEnumerable<string> Columns=>SourceTable.Columns.Select(x=>x.Name);

        public FarmWarehouse SourceFarmWarehouse { get; }
        
        public FarmWarehouse DestFarmWarehouse { get; }

        public async Task SyncAsync(CancellationToken token = default)
        {
            await DestFarmWarehouse.SyncAsync(SourceTable,token);
            await DestFarmWarehouse.AddIfSeqNothingAsync();
        }

        public virtual async Task InsertAsync(IEnumerable<IEnumerable<object>> values, CancellationToken token = default)
        {
            await DestFarmWarehouse.InsertAsync(TableName, Columns, values, token);
        }
        public virtual async Task InsertAsync(IEnumerable<object> values, CancellationToken token = default)
        {
            await DestFarmWarehouse.InsertAsync(TableName, Columns, values, token);
        }

        public async Task CheckPointAsync(CancellationToken token = default)
        {
            await DestFarmWarehouse.CheckPointAsync(TableName, null, token);
        }
    }
}
