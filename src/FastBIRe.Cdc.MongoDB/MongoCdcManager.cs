using FastBIRe.Cdc.Checkpoints;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FastBIRe.Cdc.MongoDB
{
    public class MongoCdcManager : ICdcManager
    {
        private static readonly Task<bool> taskTrue= Task.FromResult(true);

        public MongoCdcManager(IMongoClient client)
        {
            Client = client;
        }

        public IMongoClient Client { get; }

        public CdcOperators SupportCdcOperators => CdcOperators.WithoutEnableDisable;

        public Task<ICheckPointManager> GetCdcCheckPointManagerAsync(CancellationToken token = default)
        {
            return Task.FromResult<ICheckPointManager>(EmptyCheckpointManager.Instance);
        }
        public Task<ICdcListener> GetCdcListenerAsync(MongoGetCdcListenerOptions options, CancellationToken token = default)
        {
            return Task.FromResult<ICdcListener>(new MongoCdcListener(options));
        }
        Task<ICdcListener> ICdcManager.GetCdcListenerAsync(IGetCdcListenerOptions options, CancellationToken token)
        {
            return GetCdcListenerAsync((MongoGetCdcListenerOptions)options, token);
        }

        public Task<ICdcLogService> GetCdcLogServiceAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public async Task<DbVariables> GetCdcVariablesAsync(CancellationToken token = default)
        {
            var command = new BsonDocument { { "replSetGetStatus", 1 } };
            var status =await Client.GetDatabase("admin").RunCommandAsync<BsonDocument>(command);
            var var = new MongoVariables();
            var[MongoVariables.ConfigsvrKey] = status[MongoVariables.ConfigsvrKey].ToString();
            var[MongoVariables.MemberStateKey] = status["members"][0]["stateStr"].AsString;
            var[MongoVariables.MemberIdKey] = status["members"][0]["_id"].ToString()!;
            return var;
        }

        public Task<bool> IsDatabaseCdcEnableAsync(string databaseName, CancellationToken token = default)
        {
            return taskTrue;

        }

        public Task<bool> IsTableCdcEnableAsync(string databaseName, string tableName, CancellationToken token = default)
        {
            return taskTrue;
        }

        public async Task<bool> IsDatabaseSupportAsync(CancellationToken token = default)
        {
            var command = new BsonDocument { { "replSetGetStatus", 1 } };
            var status =await Client.GetDatabase("admin").RunCommandAsync<BsonDocument>(command);
            return status[MongoVariables.ConfigsvrKey].AsBoolean;
        }

        public Task<bool?> TryEnableDatabaseCdcAsync(string databaseName, CancellationToken token = default)
        {
            return Task.FromResult<bool?>(null);
        }

        public Task<bool?> TryEnableTableCdcAsync(string databaseName, string tableName, CancellationToken token = default)
        {
            return Task.FromResult<bool?>(null);
        }

        public Task<bool?> TryDisableDatabaseCdcAsync(string databaseName, CancellationToken token = default)
        {
            return Task.FromResult<bool?>(null);
        }

        public Task<bool?> TryDisableTableCdcAsync(string databaseName, string tableName, CancellationToken token = default)
        {
            return Task.FromResult<bool?>(null);
        }
    }
}
