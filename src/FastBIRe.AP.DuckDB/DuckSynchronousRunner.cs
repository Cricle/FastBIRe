using FastBIRe.Cdc;
using FastBIRe.Cdc.Checkpoints;
using FastBIRe.Cdc.Events;
using FastBIRe.Farm;

namespace FastBIRe.AP.DuckDB
{
    public class DuckSynchronousRunner : SynchronousRunner
    {
        public DuckSynchronousRunner(FarmWarehouse targetWareHouse, FarmWarehouse sourceWareHouse, string sourceTableName, ICheckpointStorage checkpointStorage, ICdcManager cdcManager) : base(targetWareHouse, sourceWareHouse, sourceTableName, checkpointStorage, cdcManager)
        {
        }

        public override IEventDispatcher<CdcEventArgs> CreateCdcDispatcher()
        {
            var handler = DuckDbCdcHandler.Create(TargetScriptExecuter, SourceTableName, new CheckpointIdentity(SourceConnection.Database, SourceTableName), CheckpointStorage); ;
            return new ChannelEventDispatcher<CdcEventArgs>(handler);
        }
    }
}
