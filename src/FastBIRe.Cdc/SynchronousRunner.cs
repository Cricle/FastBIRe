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
            return pkg?.CastCheckpoint<ICheckpoint>(cpMgr);
        }
        public async Task<ICheckpoint?> SyncAndGetCheckpointAsync(CancellationToken token = default)
        {
            var syncOk = await SyncAsync(token);
            var checkpoint = await GetCheckpointAsync(token);
            if (!syncOk && checkpoint == null)
            {
                await DeleteTargetDatasAsync(token);
                await SyncDataAsync(token);
            }
            return checkpoint;
        }

        public async Task<bool> SyncAsync(CancellationToken token = default)
        {
            var result = await FarmManager.SyncAsync(token: token);
            if (result == SyncResult.Modify)
            {
                await DeleteTargetDatasAsync(token);
                await SyncDataAsync(token);
                return true;
            }
            return false;
        }
        public abstract IEventDispatcheHandler<CdcEventArgs> CreateHandler();

        public virtual IEventDispatcher<CdcEventArgs> CreateCdcDispatcher()
        {
            return new ChannelEventDispatcher<CdcEventArgs>(CreateHandler());
        }

        public async Task EnsureDatabaseSupportAsync(CancellationToken token = default)
        {
            var isSupport = await CdcManager.IsDatabaseSupportAsync(token);
            if (!isSupport)
            {
                throw new InvalidOperationException($"The cdc manager {CdcManager.GetType()} report current database {SourceConnection.Database} is not support cdc!");
            }
        }
        public async Task<SynchronousRunDefaultResult> RunDefaultAsync(ICdcListenerOptionCreator optionCreator, IReadOnlyList<string>? tableNames = null, IProgress<TimeSpan?>? processReport = null, int reportFreqMS = 100, string memory = "1G",bool startNow=true, CancellationToken token = default)
        {
            await SetMemoryLimitAsync(memory, token);
            var checkpoint = await SyncDDLAndDataAsync(processReport, reportFreqMS: reportFreqMS);
            var eventDispatcher = CreateCdcDispatcher();
            await eventDispatcher.StartAsync(token);
            var listener = await optionCreator.CreateCdcListnerAsync(new CdcListenerOptionCreateInfo(this, checkpoint, tableNames), token: token);
            listener.AttachToDispatcher(eventDispatcher);
            if (startNow)
            {
                await listener.StartAsync(token);
            }
            if (checkpoint == null)
            {
                var lastCheckpoint = await CdcManager.GetLastCheckpointAsync(SourceConnection.Database, SourceTableName, token);
                if (lastCheckpoint != null && !lastCheckpoint.IsEmpty)
                {
                    await CheckpointStorage.SetAsync(new CheckpointPackage(new CheckpointIdentity(SourceConnection.Database, SourceTableName), lastCheckpoint.ToBytes()), token);
                }
            }
            return new SynchronousRunDefaultResult(checkpoint, eventDispatcher, listener);
        }
        private async Task<ICheckpoint?> SyncDDLAndDataAsync(IProgress<TimeSpan?>? processReport, int reportFreqMS = 100)
        {
            using (var tokenSource = new CancellationTokenSource())
            {
                var sw = Stopwatch.StartNew();
                var tsk = Task.Factory.StartNew(async () =>
                {
                    while (!tokenSource.IsCancellationRequested)
                    {
                        processReport?.Report(sw.Elapsed);
                        await Task.Delay(reportFreqMS);
                    }
                    tokenSource.Dispose();
                });
                var checkpoint = await SyncAndGetCheckpointAsync();
                tokenSource.Cancel();
                await tsk;
                processReport?.Report(null);
                return checkpoint;
            }
        }
    }
}
