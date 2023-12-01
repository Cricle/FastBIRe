using FastBIRe.Cdc.Checkpoints;
using FastBIRe.Cdc.Events;
using FastBIRe.Farm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FastBIRe.Cdc
{
    public abstract class SynchronousRunner
    {
        public SynchronousRunner(FarmWarehouse targetWareHouse, FarmWarehouse sourceWareHouse, string sourceTableName, ICheckpointStorage checkpointStorage, ICdcManager cdcManager)
        {
            TargetWarehouse = targetWareHouse;
            SourceWarehouse = sourceWareHouse;

            FarmManager = FarmManager.Create(SourceWarehouse, TargetWarehouse, sourceTableName);

            SourceTableName = sourceTableName;
            CheckpointStorage = checkpointStorage;

            if (TargetConnection.State != ConnectionState.Open)
            {
                TargetConnection.Open();
            }
            if (SourceConnection.State != ConnectionState.Open)
            {
                SourceConnection.Open();
            }
            CdcManager = cdcManager;
        }

        public string SourceTableName { get; }

        public ICheckpointStorage CheckpointStorage { get; }

        public DbConnection TargetConnection => TargetWarehouse.Connection;

        public IDbScriptExecuter TargetScriptExecuter => TargetWarehouse.ScriptExecuter;

        public FarmWarehouse TargetWarehouse { get; }

        public DbConnection SourceConnection => SourceWarehouse.Connection;

        public IDbScriptExecuter SourceScriptExecuter => SourceWarehouse.ScriptExecuter;

        public FarmWarehouse SourceWarehouse { get; }

        public FarmManager FarmManager { get; }

        public ICdcManager CdcManager { get; }

        public Task SetMemoryLimitAsync(string memory, CancellationToken token = default)
        {
            return TargetScriptExecuter.ExecuteAsync($"SET memory_limit='{memory}';", token: token);
        }
        public Task DeleteTargetDatasAsync(CancellationToken token = default)
        {
            return TargetWarehouse.DeleteAsync(SourceTableName, token);
        }
        public Task SyncDataAsync(CancellationToken token = default)
        {
            return FarmManager.SyncDataAsync(SourceTableName, token: token);
        }
        public async Task<ICheckpoint?> GetCheckpointAsync(CancellationToken token = default)
        {
            var pkg = await CheckpointStorage.GetAsync(SourceConnection.Database, SourceTableName, token: token);
            var cpMgr = await CdcManager.GetCdcCheckPointManagerAsync(token);
            return pkg?.CastCheckpoint<ICheckpoint>(cpMgr,throwException:false);
        }
        public async Task<ICheckpoint?> SyncAndGetCheckpointAsync(IProgress<SyncReport>? progress=null,bool forceSyncData=false, CancellationToken token = default)
        {
            var sw = Stopwatch.StartNew();
            progress?.Report(new SyncReport(SyncStages.SyncingStruct, null));
            var syncOk = await SyncAsync(token);
            progress?.Report(new SyncReport(SyncStages.SyncedStruct, sw.Elapsed));
            sw.Reset();
            progress?.Report(new SyncReport(SyncStages.FetchingCheckpoint, null));
            var checkpoint = await GetCheckpointAsync(token);
            progress?.Report(new SyncReport(SyncStages.FetchedCheckpoint, sw.Elapsed));
            sw.Reset();
            if (forceSyncData ||( syncOk == SyncResult.NoModify && checkpoint == null))
            {
                progress?.Report(new SyncReport(SyncStages.DeletingTargetDatas, null));
                await DeleteTargetDatasAsync(token);
                progress?.Report(new SyncReport(SyncStages.DeletedTargetDatas, sw.Elapsed));
                sw.Reset();
                progress?.Report(new SyncReport(SyncStages.SyncingData, null));
                await SyncDataAsync(token);
                progress?.Report(new SyncReport(SyncStages.SyncedData, null));
            }
            return checkpoint;
        }

        public async Task<SyncResult> SyncAsync(CancellationToken token = default)
        {
            var result = await FarmManager.SyncAsync(token: token);
            if (result == SyncResult.Modify)
            {
                await DeleteTargetDatasAsync(token);
                await SyncDataAsync(token);
            }
            return result;
        }
        public abstract IEventDispatcher<CdcEventArgs> CreateCdcDispatcher();

        public async Task EnsureDatabaseSupportAsync(CancellationToken token = default)
        {
            var isSupport = await CdcManager.IsDatabaseSupportAsync(token);
            if (!isSupport)
            {
                throw new InvalidOperationException($"The cdc manager {CdcManager.GetType()} report current database {SourceConnection.Database} is not support cdc!");
            }
        }

        public async Task<SynchronousRunDefaultResult> RunDefaultAsync(ICdcListenerOptionCreator optionCreator,IProgress<SyncReport>? progress=null, string memory = "1G",bool startNow=true,bool forceSyncData=false, CancellationToken token = default)
        {
            await SetMemoryLimitAsync(memory, token);
            var checkpoint = await SyncAndGetCheckpointAsync(progress,forceSyncData, token);
            var eventDispatcher = CreateCdcDispatcher();
            await eventDispatcher.StartAsync(token);
            var listener = await optionCreator.CreateCdcListnerAsync(new CdcListenerOptionCreateInfo(this, checkpoint), token: token);
            listener.AttachToDispatcher(eventDispatcher);
            if (checkpoint == null)
            {
                checkpoint = await CdcManager.GetLastCheckpointAsync(SourceConnection.Database, SourceTableName, token);
                if (checkpoint != null && !checkpoint.IsEmpty)
                {
                    await CheckpointStorage.SetAsync(new CheckpointPackage(new CheckpointIdentity(SourceConnection.Database, SourceTableName), checkpoint.ToBytes()), token);
                }
            }
            if (startNow)
            {
                await listener.StartAsync(token);
            }
            return new SynchronousRunDefaultResult(checkpoint, eventDispatcher, listener);
        }
    }
}
