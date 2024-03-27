using MongoDB.Driver;

namespace FastBIRe.Cdc.MongoDB
{
    public class MongoDBCdcListenerOptionCreator : ICdcListenerOptionCreator
    {
        public MongoDBCdcListenerOptionCreator(IMongoDatabase mongoDatabase)
        {
            MongoDatabase = mongoDatabase;
        }

        public IMongoDatabase MongoDatabase { get; }

        public Task<ICdcListener> CreateCdcListnerAsync(CdcListenerOptionCreateInfo info, CancellationToken token = default)
        {
            return info.Runner.CdcManager.GetCdcListenerAsync(MongoGetCdcListenerOptions.FromUpdateLookup(MongoDatabase,
                info.CheckPoint),
                token);
        }
    }
}
