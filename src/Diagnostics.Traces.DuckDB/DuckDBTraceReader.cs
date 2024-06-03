using Diagnostics.Traces.Models;
using DuckDB.NET.Data;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Diagnostics.Traces.DuckDB
{
    public class DuckDBTraceReader : ITraceReader
    {
        private const string SelectLogs = "SELECT timestamp,logLevel,categoryName,traceId,spanId,attributes,formattedMessage,body FROM \"logs\"";
        private const string SelectExceptions = "SELECT traceId,spanId,createTime,typeName,message,helpLink,data,stackTrace,innerException FROM \"exceptions\"";
        private const string SelectMetrics = "SELECT name,unit,metricType,temporality,description,meterName,meterVersion,meterTags,createTime,points FROM \"metrics\"";
        private const string SelectActivities = "SELECT id,status,statusDescription,hasRemoteParent,kind,operationName,displayName,sourceName,sourceVersion,duration,startTimeUtc,parentId,rootId,tags,events,links,baggage,context,traceStateString,spanId,traceId,recorded,activityTraceFlags,parentSpanId FROM \"activities\"";

        public DuckDBTraceReader(DuckDBConnection connection)
        {
            Connection = connection;
        }

        public DuckDBConnection Connection { get; }

        #region Metrics

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MetricEntity ReadMetric(IDataRecord record)
        {
            var res = new MetricEntity
            {
                Name = record.IsDBNull(0) ? null : record.GetString(0),
                Unit = record.IsDBNull(1) ? null : record.GetString(1),
                MetricType = (MetricType)record.GetInt32(2),
                Temporality = (AggregationTemporality)record.GetInt32(3),
                Description = record.IsDBNull(4) ? null : record.GetString(4),
                MeterName = record.IsDBNull(5) ? null : record.GetString(5),
                MeterVersion = record.IsDBNull(6) ? null : record.GetString(6),
                MeterTags = record.IsDBNull(7) ? null : (Dictionary<string, string>)record[7],
                CreateTime = record.GetDateTime(8),
                Points = new List<MetricPointEntity>(0)
            };

            var point = record[9] as List<Dictionary<string, object>>;

            if (point != null)
            {
                foreach (var item in point)
                {
                    res.Points.Add(ReadEntity(item));
                }
            }

            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MetricPointEntity ReadEntity(Dictionary<string, object> map)
        {
            var entity = new MetricPointEntity();
            foreach (var item in map)
            {
                if (item.Value == null)
                {
                    continue;
                }
                switch (item.Key)
                {
                    case "name":
                        entity.Value = (double?)item.Value;
                        break;
                    case "sum":
                        entity.Sum = (double?)item.Value;
                        break;
                    case "count":
                        entity.Count = (int?)item.Value;
                        break;
                    case "min":
                        entity.Min = (double?)item.Value;
                        break;
                    case "max":
                        entity.Max = (double?)item.Value;
                        break;
                    case "histogram":
                        {
                            var histograms = new List<MetricHistogramEntity>();
                            var raw = (List<Dictionary<string, object>>)item.Value;
                            foreach (var r in raw)
                            {
                                var hiEntity = new MetricHistogramEntity();
                                foreach (var row in r)
                                {
                                    switch (row.Key)
                                    {
                                        case "rangeLeft":
                                            hiEntity.RangeLeft = (double)row.Value;
                                            break;
                                        case "rangeRight":
                                            hiEntity.RangeRight = (double)row.Value;
                                            break;
                                        case "bucketCount":
                                            hiEntity.BucketCount = (int)row.Value;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                histograms.Add(hiEntity);
                            }
                            entity.Histograms = histograms;
                        }
                        break;
                    case "zeroBucketCount":
                        entity.ZeroBucketCount = (long?)item.Value;
                        break;
                    case "buckets":
                        {
                            var buckets = new List<MetricBucketEntity>();
                            var raw = (List<Dictionary<string, object>>)item.Value;
                            foreach (var r in raw)
                            {
                                var hiEntity = new MetricBucketEntity();
                                foreach (var row in r)
                                {
                                    switch (row.Key)
                                    {
                                        case "lowerBound":
                                            hiEntity.LowerBound = (double)row.Value;
                                            break;
                                        case "upperBound":
                                            hiEntity.UpperBound = (double)row.Value;
                                            break;
                                        case "bucketCount":
                                            hiEntity.BucketCount = (int)row.Value;
                                            break;
                                        default:
                                            break;
                                    }
                                }

                                buckets.Add(hiEntity);
                            }
                            entity.Buckets = buckets;
                        }
                        break;
                    case "tags":
                        entity.Tags = (Dictionary<string, string>)item.Value;
                        break;
                    case "startTime":
                        entity.StartTime = (DateTime)item.Value;
                        break;
                    case "endTime":
                        entity.EndTime = (DateTime)item.Value;
                        break;
                    default:
                        break;
                }
            }
            return entity;
        }

        public IEnumerable<MetricEntity> ReadMetrics()
        {
            using (var comm = Connection.CreateCommand())
            {
                comm.CommandText = SelectMetrics;
                using (var reader = comm.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return ReadMetric(reader);
                    }
                }
            }
        }
        #endregion
        #region Exceptions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ExceptionEntity ReadException(IDataRecord record)
        {
            return new ExceptionEntity
            {
                TraceId = record.IsDBNull(0) ? null : record.GetString(0),
                SpanId = record.IsDBNull(1) ? null : record.GetString(1),
                CreateTime = record.GetDateTime(2),
                TypeName = record.IsDBNull(3) ? null : record.GetString(3),
                Message = record.IsDBNull(4) ? null : record.GetString(4),
                HelpLink = record.IsDBNull(5) ? null : record.GetString(5),
                Data = record.IsDBNull(6) ? null : (Dictionary<string, string>)record[6],
                StackTrace = record.IsDBNull(7) ? null : record.GetString(7),
                InnerException = record.IsDBNull(8) ? null : record.GetString(8),
            };
        }

        private string BuildWhereTraceSql(string sql,IEnumerable<string>? traceIds)
        {
            if (traceIds != null && traceIds.Any())
            {
                return $"{sql} WHERE traceId in ({string.Join(",", traceIds.Select(x => $"'{x}'"))})";
            }
            return sql;
        }

        public IEnumerable<ExceptionEntity> ReadExceptions(IEnumerable<string>? traceIds = null)
        {
            using (var comm = Connection.CreateCommand())
            {
                comm.CommandText = BuildWhereTraceSql(SelectExceptions,traceIds);
                using (var reader = comm.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return ReadException(reader);
                    }
                }
            }
        }
        #endregion
        #region Logs

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private LogEntity ReadEntity(IDataRecord record)
        {
            return new LogEntity
            {
                Timestamp = record.GetDateTime(0),
                LogLevel = (LogLevel)record.GetInt32(1),
                CategoryName = record.IsDBNull(2) ? null : record.GetString(2),
                TraceId = record.IsDBNull(3) ? null : record.GetString(3),
                SpanId = record.IsDBNull(4) ? null : record.GetString(4),
                Attributes = record.IsDBNull(5) ? null : (Dictionary<string, string>)record[5],
                FormattedMessage = record.IsDBNull(6) ? null : record.GetString(6),
                Body = record.IsDBNull(7) ? null : record.GetString(7),
            };
        }
        public IEnumerable<LogEntity> ReadLogs(IEnumerable<string>? traceIds = null)
        {
            using (var comm = Connection.CreateCommand())
            {
                comm.CommandText = BuildWhereTraceSql(SelectLogs, traceIds);
                using (var reader = comm.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return ReadEntity(reader);
                    }
                }
            }
        }
        #endregion

        #region Activities

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AcvtityEntity ReadActivity(IDataRecord record)
        {
            var activity = new AcvtityEntity
            {
                Id = record.GetString(0),
                Status = (ActivityStatusCode)record.GetInt32(1),
                StatusDescription = record.IsDBNull(2) ? null : record.GetString(2),
                HasRemoteParent = record.GetBoolean(3),
                Kind = (ActivityKind)record.GetInt32(4),
                OperationName = record.IsDBNull(5) ? null : record.GetString(5),
                DisplayName = record.IsDBNull(6) ? null : record.GetString(6),
                SourceName = record.IsDBNull(7) ? null : record.GetString(7),
                SourceVersion = record.IsDBNull(8) ? null : record.GetString(8),
                Duration = record.GetDouble(9),
                StartTimeUtc = record.GetDateTime(10),
                ParentId = record.IsDBNull(11) ? null : record.GetString(11),
                RootId = record.IsDBNull(12) ? null : record.GetString(12),
                Tags = record.IsDBNull(13) ? null : (Dictionary<string, string>)record[13],
                Events = new List<ActivityEventEntity>(),
                Links = new List<ActivityLinkEntity>(),
                Baggage = record.IsDBNull(16) ? null : (Dictionary<string, string>)record[16],
                TraceStateString = record.IsDBNull(18) ? null : record.GetString(18),
                SpanId = record.IsDBNull(19) ? null : record.GetString(19),
                TraceId = record.IsDBNull(20) ? null : record.GetString(20),
                Recorded = record.GetBoolean(21),
                ActivityTraceFlags = (ActivityTraceFlags)record.GetInt32(22),
                ParentSpanId = record.IsDBNull(23) ? null : record.GetString(23),
            };

            var events = record.IsDBNull(14) ? null : (List<Dictionary<string, object>>)record[14];
            var links = record.IsDBNull(15) ? null : (List<Dictionary<string, object>>)record[15];
            var context = record.IsDBNull(17) ? null : (Dictionary<string, object>)record[17];

            activity.Context = ReadContextEntity(context);

            if (events != null)
            {
                foreach (var item in events)
                {
                    activity.Events.Add(ReadEventEntity(item));
                }
            }
            if (links != null)
            {
                foreach (var item in links)
                {
                    activity.Links.Add(ReadLinkEntity(item));
                }
            }
            return activity;
        }
        private ActivityLinkEntity ReadLinkEntity(Dictionary<string, object> map)
        {
            var link = new ActivityLinkEntity();
            foreach (var item in map)
            {
                switch (item.Key)
                {
                    case "context":
                        {
                            var context = new ActivityLinkContextEntity();
                            var raw = (Dictionary<string, object>)item.Value;
                            foreach (var r in raw)
                            {
                                switch (r.Key)
                                {
                                    case "traceId":
                                        context.TraceId = (string?)r.Value;
                                        break;
                                    case "traceState":
                                        context.TraceState = (string?)r.Value;
                                        break;
                                    case "traceFlags":
                                        context.TraceFlags = (ActivityTraceFlags)r.Value;
                                        break;
                                    case "isRemote":
                                        context.IsRemote = (bool)r.Value;
                                        break;
                                    case "spanId":
                                        context.SpanId = (string?)r.Value;
                                        break;
                                    default:
                                        break;
                                }
                            }

                            link.Context = context;
                        }
                        break;
                    case "tags":
                        link.Tags = (Dictionary<string, string>?)item.Value;
                        break;
                    default:
                        break;
                }
            }

            return link;
        }
        private ActivityEventEntity ReadEventEntity(Dictionary<string, object> map)
        {
            var @event = new ActivityEventEntity();
            foreach (var item in map)
            {
                switch (item.Key)
                {
                    case "name":
                        @event.Name = (string?)item.Value;
                        break;
                    case "timestamp":
                        @event.Timestamp = (DateTime)item.Value;
                        break;
                    case "tags":
                        @event.Tags = (Dictionary<string, string>?)item.Value;
                        break;
                    default:
                        break;
                }
            }

            return @event;
        }
        private ActivityLinkContextEntity? ReadContextEntity(Dictionary<string, object>? map)
        {
            if (map == null)
            {
                return default;
            }
            var entity = new ActivityLinkContextEntity();
            foreach (var item in map)
            {
                switch (item.Key)
                {
                    case "traceId":
                        entity.TraceId = (string?)item.Value;
                        break;
                    case "traceState":
                        entity.TraceState = (string?)item.Value;
                        break;
                    case "traceFlags":
                        entity.TraceFlags = (ActivityTraceFlags)item.Value;
                        break;
                    case "isRemote":
                        entity.IsRemote = (bool)item.Value;
                        break;
                    case "spanId":
                        entity.TraceId = (string?)item.Value;
                        break;
                    default:
                        break;
                }
            }

            return entity;
        }
        public IEnumerable<AcvtityEntity> ReadActivities(IEnumerable<string>? traceIds = null)
        {
            using (var comm = Connection.CreateCommand())
            {
                comm.CommandText =BuildWhereTraceSql(SelectActivities, traceIds);
                using (var reader = comm.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return ReadActivity(reader);
                    }
                }
            }
        }
        #endregion
    }
}
