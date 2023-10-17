using MongoDB.Bson;
using MongoDB.Driver;

namespace FastBIRe.Cdc.MongoDB
{
    public class MongoGetCdcListenerOptions : IGetCdcListenerOptions
    {
        public MongoGetCdcListenerOptions(IReadOnlyList<string>? tableNames, IChangeStreamCursor<ChangeStreamDocument<BsonDocument>> changeStreamCursor)
        {
            TableNames = tableNames;
            ChangeStreamCursor = changeStreamCursor;
        }

        public IReadOnlyList<string>? TableNames { get; }

        public IChangeStreamCursor<ChangeStreamDocument<BsonDocument>> ChangeStreamCursor { get; }

        public static MongoGetCdcListenerOptions FromUpdateLookup(IReadOnlyList<string>? tableNames,IMongoDatabase database)
        {
            var cursor= database.Watch(new ChangeStreamOptions
            {
                FullDocument= ChangeStreamFullDocumentOption.UpdateLookup
            });
            return new MongoGetCdcListenerOptions(tableNames,cursor);   
        }
    }
}
