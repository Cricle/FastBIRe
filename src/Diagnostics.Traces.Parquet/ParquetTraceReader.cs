using Diagnostics.Traces.Models;
using Microsoft.Extensions.Logging;
using ParquetSharp;
using System.Buffers;
using System.Diagnostics;
using System.Text.Json;

namespace Diagnostics.Traces.Parquet
{
    public class ParquetTraceReader : ITraceReader
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            TypeInfoResolverChain =
            {
                DictionaryStringStringJsonSerializerContext.Default,
                ActivityEventEntitysJsonSerializerContext.Default,
                ActivityLinkEntitysJsonSerializerContext.Default,
                ActivityLinkContextEntityJsonSerializerContext.Default
            },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ParquetTraceReader(ParquetFileReader reader)
        {
            Reader = reader;
        }

        public ParquetFileReader Reader { get; }

        class ColumnData<T> : IDisposable
        {
            public readonly RowGroupReader Reader;

            public readonly int Count;

            public T[]? Datas;

            public ColumnData(RowGroupReader reader, int count)
            {
                Reader = reader;
                Count = count;
            }

            public T[] Read(int col)
            {
                if (Datas == null)
                {
                    Datas = ArrayPool<T>.Shared.Rent(Count);
                    Reader.Column(col).LogicalReader<T>().ReadBatch(Datas);
                }
                return Datas!;
            }

            public void Dispose()
            {
                if (Datas != null)
                {
                    ArrayPool<T>.Shared.Return(Datas!);
                }
            }
        }
        public IEnumerable<AcvtityEntity> ReadActivities(IEnumerable<string>? traceIds = null)
        {
            for (int i = 0; i < Reader.FileMetaData.NumRowGroups; i++)
            {
                using var group = Reader.RowGroup(i);
                var nodes = new ColumnDescriptor[group.MetaData.Schema.NumColumns];
                var rowCount = (int)group.MetaData.NumRows;
                using var idReader = new ColumnData<string?>(group, rowCount);
                using var statusReader = new ColumnData<int>(group, rowCount);
                using var statusDescriptionReader = new ColumnData<string?>(group, rowCount);
                using var hasRemoteParentReader = new ColumnData<bool>(group, rowCount);
                using var kindReader = new ColumnData<int>(group, rowCount);
                using var operationNameReader = new ColumnData<string?>(group, rowCount);
                using var displayNameReader = new ColumnData<string?>(group, rowCount);
                using var sourceNameReader = new ColumnData<string?>(group, rowCount);
                using var sourceVersionReader = new ColumnData<string?>(group, rowCount);
                using var durationReader = new ColumnData<double>(group, rowCount);
                using var startTimeUtcReader = new ColumnData<DateTime>(group, rowCount);
                using var parentIdReader = new ColumnData<string?>(group, rowCount);
                using var rootIdReader = new ColumnData<string?>(group, rowCount);
                using var tagsReader = new ColumnData<string?>(group, rowCount);
                using var eventsReader = new ColumnData<string?>(group, rowCount);
                using var linksReader = new ColumnData<string?>(group, rowCount);
                using var baggageReader = new ColumnData<string?>(group, rowCount);
                using var contextReader = new ColumnData<string?>(group, rowCount);
                using var traceStateStringReader = new ColumnData<string?>(group, rowCount);
                using var spanIdReader = new ColumnData<string?>(group, rowCount);
                using var traceIdReader = new ColumnData<string?>(group, rowCount);
                using var recordedReader = new ColumnData<bool>(group, rowCount);
                using var activityTraceFlagsReader = new ColumnData<int>(group, rowCount);
                using var parentSpanIdReader = new ColumnData<string?>(group, rowCount);

                for (int j = 0; j < nodes.Length; j++)
                {
                    nodes[j] = group.MetaData.Schema.Column(j);
                    switch (nodes[j].Name)
                    {
                        case "id": idReader.Read(j); break;
                        case "status": statusReader.Read(j); break;
                        case "statusDescription": statusDescriptionReader.Read(j); break;
                        case "hasRemoteParent": hasRemoteParentReader.Read(j); break;
                        case "kind": kindReader.Read(j); break;
                        case "operationName": operationNameReader.Read(j); break;
                        case "displayName": displayNameReader.Read(j); break;
                        case "sourceName": sourceNameReader.Read(j); break;
                        case "sourceVersion": sourceVersionReader.Read(j); break;
                        case "duration": durationReader.Read(j); break;
                        case "startTimeUtc": startTimeUtcReader.Read(j); break;
                        case "parentId": parentIdReader.Read(j); break;
                        case "rootId": rootIdReader.Read(j); break;
                        case "tags": tagsReader.Read(j); break;
                        case "events": eventsReader.Read(j); break;
                        case "links": linksReader.Read(j); break;
                        case "baggage": baggageReader.Read(j); break;
                        case "context": contextReader.Read(j); break;
                        case "traceStateString": traceStateStringReader.Read(j); break;
                        case "spanId": spanIdReader.Read(j); break;
                        case "traceId": traceIdReader.Read(j); break;
                        case "recorded": recordedReader.Read(j); break;
                        case "activityTraceFlags": activityTraceFlagsReader.Read(j); break;
                        case "parentSpanId": parentSpanIdReader.Read(j); break;
                        default:
                            break;
                    }
                }
                for (long j = 0; j < group.MetaData.NumRows; j++)
                {
                    var entity = new AcvtityEntity();
                    for (int q = 0; q < nodes.Length; q++)
                    {
                        var name = nodes[q].Name;
                        switch (name)
                        {
                            case "id": entity.Id = idReader.Datas![j]; break;
                            case "status": entity.Status = (ActivityStatusCode)statusReader.Datas![j]; break;
                            case "statusDescription": entity.StatusDescription = statusDescriptionReader.Datas![j]; break;
                            case "hasRemoteParent": entity.HasRemoteParent = hasRemoteParentReader.Datas![j]; break;
                            case "kind": entity.Kind = (ActivityKind)kindReader.Datas![j]; break;
                            case "operationName": entity.OperationName = operationNameReader.Datas![j]; break;
                            case "displayName": entity.DisplayName = displayNameReader.Datas![j]; break;
                            case "sourceName": entity.SourceName = sourceNameReader.Datas![j]; break;
                            case "sourceVersion": entity.SourceVersion = sourceVersionReader.Datas![j]; break;
                            case "duration": entity.Duration = durationReader.Datas![j]; break;
                            case "startTimeUtc": entity.StartTimeUtc = startTimeUtcReader.Datas![j]; break;
                            case "parentId": entity.ParentId = parentIdReader.Datas![j]; break;
                            case "rootId": entity.RootId = rootIdReader.Datas![j]; break;
                            case "tags": entity.Tags = JsonSerializer.Deserialize<Dictionary<string, string?>>(tagsReader.Datas![j] ?? "{}", jsonSerializerOptions); break;
                            case "events": entity.Events = JsonSerializer.Deserialize<List<ActivityEventEntity>>(eventsReader.Datas![j] ?? "[]", jsonSerializerOptions); break;
                            case "links": entity.Links = JsonSerializer.Deserialize<List<ActivityLinkEntity>>(linksReader.Datas![j] ?? "[]", jsonSerializerOptions); break;
                            case "baggage": entity.Baggage = JsonSerializer.Deserialize<Dictionary<string, string?>>(baggageReader.Datas![j] ?? "{}", jsonSerializerOptions); break;
                            case "context": entity.Context = JsonSerializer.Deserialize<ActivityLinkContextEntity?>(contextReader.Datas![j] ?? "{}", jsonSerializerOptions); break;
                            case "traceStateString": entity.TraceStateString = traceStateStringReader.Datas![j]; break;
                            case "spanId": entity.SpanId = spanIdReader.Datas![j]; break;
                            case "traceId": entity.TraceId = traceIdReader.Datas![j]; break;
                            case "recorded": entity.Recorded = recordedReader.Datas![j]; break;
                            case "activityTraceFlags": entity.ActivityTraceFlags = (ActivityTraceFlags)activityTraceFlagsReader.Datas![j]; break;
                            case "parentSpanId": entity.ParentSpanId = parentSpanIdReader.Datas![j]; break;
                            default:
                                break;
                        }
                    }
                    yield return entity;
                }
            }
        }

        public IEnumerable<ExceptionEntity> ReadExceptions(IEnumerable<string>? traceIds = null)
        {
            for (int i = 0; i < Reader.FileMetaData.NumRowGroups; i++)
            {
                using var group = Reader.RowGroup(i);
                var nodes = new ColumnDescriptor[group.MetaData.Schema.NumColumns];
                var rowCount = (int)group.MetaData.NumRows;

                using var traceId = new ColumnData<string?>(group, rowCount);
                using var spanId = new ColumnData<string?>(group, rowCount);
                using var createTime = new ColumnData<DateTime>(group, rowCount);
                using var typeName = new ColumnData<string?>(group, rowCount);
                using var message = new ColumnData<string?>(group, rowCount);
                using var helpLink = new ColumnData<string?>(group, rowCount);
                using var data = new ColumnData<string?>(group, rowCount);
                using var stackTrace = new ColumnData<string?>(group, rowCount);
                using var innerException = new ColumnData<string?>(group, rowCount);


                for (int j = 0; j < nodes.Length; j++)
                {
                    nodes[j] = group.MetaData.Schema.Column(j);
                    switch (nodes[j].Name)
                    {
                        case "traceId": traceId.Read(j); break;
                        case "spanId": spanId.Read(j); break;
                        case "createTime": createTime.Read(j); break;
                        case "typeName": typeName.Read(j); break;
                        case "message": message.Read(j); break;
                        case "helpLink": helpLink.Read(j); break;
                        case "data": data.Read(j); break;
                        case "stackTrace": stackTrace.Read(j); break;
                        case "innerException": innerException.Read(j); break;
                        default:
                            break;
                    }
                }

                for (long j = 0; j < group.MetaData.NumRows; j++)
                {
                    var entity = new ExceptionEntity();
                    for (int q = 0; q < nodes.Length; q++)
                    {
                        var name = nodes[q].Name;
                        switch (name)
                        {
                            case "traceId": entity.TraceId = traceId.Datas![j]; break;
                            case "spanId": entity.SpanId = spanId.Datas![j]; break;
                            case "createTime": entity.CreateTime = createTime.Datas![j]; break;
                            case "typeName": entity.TypeName = typeName.Datas![j]; break;
                            case "message": entity.Message = message.Datas![j]; break;
                            case "helpLink": entity.HelpLink= helpLink.Datas![j]; break;
                            case "data": entity.Data= JsonSerializer.Deserialize<Dictionary<string, string?>>(data.Datas![j] ?? "{}", jsonSerializerOptions); break;
                            case "stackTrace": entity.StackTrace = stackTrace.Datas![j]; break;
                            case "innerException": entity.InnerException = innerException.Datas![j]; break;
                            default:
                                break;
                        }
                    }
                    yield return entity;
                }
            }
        }

        public IEnumerable<LogEntity> ReadLogs(IEnumerable<string>? traceIds = null)
        {
            for (int i = 0; i < Reader.FileMetaData.NumRowGroups; i++)
            {
                using var group = Reader.RowGroup(i);
                var nodes = new ColumnDescriptor[group.MetaData.Schema.NumColumns];
                var rowCount = (int)group.MetaData.NumRows;

                using var timestamp = new ColumnData<DateTime>(group, rowCount);
                using var logLevel = new ColumnData<int>(group, rowCount);
                using var categoryName = new ColumnData<string?>(group, rowCount);
                using var traceId = new ColumnData<string?>(group, rowCount);
                using var spanId = new ColumnData<string?>(group, rowCount);
                using var attributes = new ColumnData<string?>(group, rowCount);
                using var formattedMessage = new ColumnData<string?>(group, rowCount);
                using var body = new ColumnData<string?>(group, rowCount);

                for (int j = 0; j < nodes.Length; j++)
                {
                    nodes[j] = group.MetaData.Schema.Column(j);
                    switch (nodes[j].Name)
                    {
                        case "timestamp": timestamp.Read(j); break;
                        case "logLevel": logLevel.Read(j); break;
                        case "categoryName": categoryName.Read(j); break;
                        case "traceId": traceId.Read(j); break;
                        case "spanId": spanId.Read(j); break;
                        case "attributes": attributes.Read(j); break;
                        case "formattedMessage": formattedMessage.Read(j); break;
                        case "body": body.Read(j); break;
                        default:
                            break;
                    }
                }

                for (long j = 0; j < group.MetaData.NumRows; j++)
                {
                    var entity = new LogEntity();
                    for (int q = 0; q < nodes.Length; q++)
                    {
                        var name = nodes[q].Name;
                        switch (name)
                        {
                            case "timestamp": entity.Timestamp = timestamp.Datas![j]; break;
                            case "logLevel": entity.LogLevel = (LogLevel)logLevel.Datas![j]; break;
                            case "categoryName": entity.CategoryName = categoryName.Datas![j]; break;
                            case "traceId": entity.TraceId = traceId.Datas![j]; break;
                            case "spanId": entity.SpanId = spanId.Datas![j]; break;
                            case "attributes": entity.Attributes = JsonSerializer.Deserialize<Dictionary<string,string?>>(attributes.Datas![j]??"{}",jsonSerializerOptions); break;
                            case "formattedMessage": entity.FormattedMessage = formattedMessage.Datas![j]; break;
                            case "body": entity.Body = body.Datas![j]; break;
                            default:
                                break;
                        }
                    }
                    yield return entity;
                }
            }
        }

        public IEnumerable<MetricEntity> ReadMetrics()
        {
            throw new NotImplementedException();
        }
    }
}
