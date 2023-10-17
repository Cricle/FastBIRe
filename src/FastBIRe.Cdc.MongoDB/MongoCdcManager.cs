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

        public Task<DbVariables> GetCdcVariablesAsync(CancellationToken token = default)
        {
            var command = new BsonDocument { { "replSetGetStatus", 1 } };
            var status = Client.GetDatabase("admin").RunCommand<BsonDocument>(command);
            var var = new MongoVariables();
            var[MongoVariables.MemberStateKey] = status["members"]["stateStr"].AsString;
            var[MongoVariables.MemberIdKey] = status["members"]["_id"].ToString()!;
            return Task.FromResult<DbVariables>(var);
        }

        public Task<bool> IsDatabaseCdcEnableAsync(string databaseName, CancellationToken token = default)
        {
            return taskTrue;

        }

        public Task<bool> IsTableCdcEnableAsync(string databaseName, string tableName, CancellationToken token = default)
        {
            return taskTrue;
        }
    }
}
