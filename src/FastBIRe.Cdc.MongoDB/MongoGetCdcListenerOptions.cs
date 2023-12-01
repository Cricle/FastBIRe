using FastBIRe.Cdc.Checkpoints;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FastBIRe.Cdc.MongoDB
{
    public class MongoGetCdcListenerOptions : GetCdcListenerOptions
    {
        public MongoGetCdcListenerOptions(IChangeStreamCursor<ChangeStreamDocument<BsonDocument>> changeStreamCursor, ICheckpoint? checkpoint)
            : base(checkpoint)
        {
            ChangeStreamCursor = changeStreamCursor;
        }

        public IChangeStreamCursor<ChangeStreamDocument<BsonDocument>> ChangeStreamCursor { get; }

        public static MongoGetCdcListenerOptions FromUpdateLookup(IMongoDatabase database, ICheckpoint? checkpoint)
        {
            var cursor = database.Watch(new ChangeStreamOptions
            {
                FullDocument = ChangeStreamFullDocumentOption.UpdateLookup
            });
            return new MongoGetCdcListenerOptions(cursor, checkpoint);
        }
    }
}
