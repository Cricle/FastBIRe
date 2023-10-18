using DatabaseSchemaReader.DataSchema;

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

        public async Task SyncAsync(IEnumerable<int>? maskColumns = null,CancellationToken token = default)
        {
            await DestFarmWarehouse.SyncAsync(SourceTable, maskColumns, token);
            if (DestFarmWarehouse.AttackId)
            {
                await DestFarmWarehouse.AddIfSeqNothingAsync();
            }
        }

        public virtual async Task InsertAsync(IEnumerable<IEnumerable<object?>> values, CancellationToken token = default)
        {
            await DestFarmWarehouse.InsertAsync(TableName, Columns, values, token);
        }
        public virtual async Task InsertAsync(IEnumerable<object?> values, CancellationToken token = default)
        {
            await DestFarmWarehouse.InsertAsync(TableName, Columns, values, token);
        }

        public Task<IList<ICursorRowHandlerResult>> CheckPointAsync(CancellationToken token = default)
        {
            return DestFarmWarehouse.CheckPointAsync(TableName, null, token);
        }

        public void Dispose()
        {
            SourceFarmWarehouse.Dispose();
            DestFarmWarehouse.Dispose();
        }
    }
}
