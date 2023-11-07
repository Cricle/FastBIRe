using DatabaseSchemaReader.DataSchema;
using FastBIRe.Cdc.Checkpoints;
using FastBIRe.Cdc.Events;

namespace FastBIRe.AP.DuckDB
{
    public class DuckDbCdcHandler : IEventDispatcheHandler<CdcEventArgs>
    {
        public static DuckDbCdcHandler Create(IDbScriptExecuter connection,string tableName, CheckpointIdentity identity, ICheckpointStorage checkpointStorage)
        {
            var reader = connection.CreateReader();
            var table = reader.Table(tableName);
            if (table==null)
            {
                throw new ArgumentException($"Table {tableName} not found!");
            }
            return new DuckDbCdcHandler(connection, table, identity, checkpointStorage);
        }

        public DuckDbCdcHandler(IDbScriptExecuter connection, DatabaseTable table, CheckpointIdentity identity, ICheckpointStorage checkpointStorage)
        {
            ScriptExecuter = connection;
            Table = table;
            SqlType = connection.CreateReader().SqlType!.Value;
            TableWrapper = new TableWrapper(Table, SqlType, null);
            CheckpointStorage = checkpointStorage;
            Identity = identity;
        }
        public CheckpointIdentity Identity { get; }

        public IDbScriptExecuter ScriptExecuter { get; }

        public ICheckpointStorage CheckpointStorage { get; }

        public SqlType SqlType { get; }

        public DatabaseTable Table { get;}

        public TableWrapper TableWrapper { get; }

        public event EventHandler<ICheckpoint>? CheckpointUpdate;

        public async Task HandleAsync(CdcEventArgs input, CancellationToken token = default)
        {
            if (input is OperatorCdcEventArgs ea && ea.TableInfo != null && ea.TableInfo.TableName == Table.Name)
            {
                if (input is InsertEventArgs iea)
                {
                    foreach (var item in iea.Rows)
                    {
                        var script = TableWrapper.InsertOrUpdate(item);
                        if (!string.IsNullOrEmpty(script))
                        {
                            await ScriptExecuter.ExecuteAsync(script, token: token);
                        }
                    }
                }
                else if (input is UpdateEventArgs uea)
                {
                    foreach (var item in uea.Rows)
                    {
                        var script = TableWrapper.InsertOrUpdate(item.AfterRow);
                        if (!string.IsNullOrEmpty(script))
                        {
                            await ScriptExecuter.ExecuteAsync(script, token: token);
                        }
                    }
                }
                else if (input is DeleteEventArgs dea)
                {
                    foreach (var item in dea.Rows)
                    {
                        var script = TableWrapper.CreateDeleteByKeySql(item);
                        if (!string.IsNullOrEmpty(script)) 
                        {
                            await ScriptExecuter.ExecuteAsync(script, token: token);
                        }
                    }
                }
            }
            if (input.Checkpoint != null && !input.Checkpoint.IsEmpty)
            {
                CheckpointUpdate?.Invoke(this, input.Checkpoint);
                await CheckpointStorage.SetAsync(new CheckpointPackage(Identity, input.Checkpoint.ToBytes()), token);
            }
        }
    }
}
