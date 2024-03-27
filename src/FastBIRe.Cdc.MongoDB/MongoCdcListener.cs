using FastBIRe.Cdc.Events;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FastBIRe.Cdc.MongoDB
{
    public class MongoCdcListener : CdcListenerBase
    {
        private Task? task;
        private readonly MongoGetCdcListenerOptions options;
        private readonly Dictionary<string, ITableMapInfo> tableMapInfos = new Dictionary<string, ITableMapInfo>();

        public MongoCdcListener(MongoGetCdcListenerOptions options)
            : base(options)
        {
            this.options = options;
        }

        public ITableMapInfo? GetTableMapInfo(string id)
        {
            return tableMapInfos.TryGetValue(id, out var ifo) ? ifo : null;
        }
        public override ITableMapInfo? GetTableMapInfo(object id)
        {
            if (id != null)
            {
                return GetTableMapInfo(id.ToString()!);
            }
            return null;
        }
        protected override Task OnStartAsync(CancellationToken token = default)
        {
            task = Task.Factory.StartNew(Handler, this, token, TaskCreationOptions.LongRunning, TaskScheduler.Current)
                .Unwrap();
            return Task.CompletedTask;
        }

        protected override Task OnStopAsync(CancellationToken token = default)
        {
            tableMapInfos.Clear();
            task = null;
            return Task.CompletedTask;
        }
        private static CdcDataRow CreateRow(BsonDocument doc)
        {
            var row = new CdcDataRow(doc.ElementCount);
            foreach (var col in doc)
            {
                row.Add(BsonTypeMapper.MapToDotNetValue(col.Value));
            }
            return row;
        }
        private static CdcDataRow CreateRow(ChangeStreamDocument<BsonDocument> item)
        {
            var fullDoc = item.BackingDocument["fullDocument"];
            var doc = fullDoc.AsBsonDocument;
            // By bson object, can object in object
            return CreateRow(doc);
        }
        private async Task Handler(object? state)
        {
            var listener = (MongoCdcListener)state!;
            var source = listener.TokenSource;
            var cursor = listener.options!.ChangeStreamCursor;
            while (source!.IsCancellationRequested)
            {
                while (await cursor.MoveNextAsync(source.Token))
                {
                    try
                    {
                        foreach (var item in cursor.Current)
                        {
                            var tableInfo = new TableMapInfo(item.CollectionNamespace.FullName, item.CollectionNamespace.DatabaseNamespace.DatabaseName, item.CollectionNamespace.CollectionName);
                            switch (item.OperationType)
                            {
                                case ChangeStreamOperationType.Insert:
                                    {
                                        var insertEvent = new InsertEventArgs(item, item.CollectionNamespace.CollectionName, tableInfo, new ICdcDataRow[]
                                        {
                                            CreateRow(item)
                                        }, null);
                                        RaiseEvent(insertEvent);
                                    }
                                    break;
                                case ChangeStreamOperationType.Update:
                                    {
                                        var insertEvent = new UpdateEventArgs(item, item.CollectionNamespace.CollectionName, tableInfo, new ICdcUpdateRow[]
                                        {
                                            new CdcUpdateRow(null, CreateRow(item))
                                        }, null);
                                        RaiseEvent(insertEvent);
                                    }
                                    break;
                                case ChangeStreamOperationType.Delete:
                                    {
                                        var row = CreateRow(item.DocumentKey);
                                        var insertEvent = new DeleteEventArgs(item, item.CollectionNamespace.CollectionName, tableInfo, new ICdcDataRow[]
                                        {
                                            row
                                        }, null);
                                        RaiseEvent(insertEvent);
                                    }
                                    break;
                                default:
                                    RaiseEvent(new CdcEventArgs(item, null));
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        RaiseError(new CdcErrorEventArgs(ex));
                    }
                }
            }
        }
    }
}
