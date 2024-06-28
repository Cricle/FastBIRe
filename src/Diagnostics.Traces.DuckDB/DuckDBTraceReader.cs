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
        private const string SelectLogs = "SELECT * FROM \"logs\"";
        private const string SelectExceptions = "SELECT * FROM \"exceptions\"";
        private const string SelectMetrics = "SELECT * FROM \"metrics\"";
        private const string SelectActivities = "SELECT * FROM \"activities\"";

        public DuckDBTraceReader(DuckDBConnection connection)
        {
            Connection = connection;
        }

        public DuckDBConnection Connection { get; }

        #region Metrics

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private MetricEntity ReadMetric(IDataRecord record)
        {
            var entity = new MetricEntity { Points=new List<MetricPointEntity>() };
            for (int i = 0; i < record.FieldCount; i++)
            {
                var name = record.GetName(i);
                switch (name)
                {
                    case "name": entity.Name = record.IsDBNull(i) ? null : record.GetString(i); break;
                    case "unit": entity.Unit = record.IsDBNull(i) ? null : record.GetString(i); break;
                    case "metricType": entity.MetricType = (MetricType)record.GetInt32(i); break;
                    case "temporality": entity.Temporality = (AggregationTemporality)record.GetInt32(i); break;
                    case "description": entity.Description = record.IsDBNull(i) ? null : record.GetString(i); break;
                    case "meterName": entity.MeterName = record.IsDBNull(i) ? null : record.GetString(i); break;
                    case "meterVersion": entity.MeterVersion = record.IsDBNull(i) ? null : record.GetString(i); break;
                    case "meterTags":entity.MeterTags= record.IsDBNull(i) ? null : (Dictionary<string, string>)record[i]; break;
                    case "createTime": entity.CreateTime = record.GetDateTime(i); break;
                    case "points":
                        {
                            var points = record[i] as List<Dictionary<string, object>>;
                            if (points!=null)
                            {
                                foreach (var item in points)
                                {
                                    entity.Points.Add(ReadEntity(item));
                                }
                            }
                            break;
                        }
                    default:
                        break;
                }
            }

            return entity;
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
        public IEnumerable<MetricEntity> ReadMetrics(string sql)
        {
            using (var comm = Connection.CreateCommand())
            {
                comm.CommandText = sql;
                using (var reader = comm.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return ReadMetric(reader);
                    }
                }
            }
        }
        public IEnumerable<MetricEntity> ReadMetrics()
        {
            return ReadMetrics(SelectMetrics);
        }
        #endregion
        #region Exceptions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ExceptionEntity ReadException(IDataRecord record)
        {
            var ex = new ExceptionEntity();
            for (int i = 0; i < record.FieldCount; i++)
            {
                switch (record.GetName(i))
                {
                    case "traceId":ex.TraceId=record.IsDBNull(i) ? null : record.GetString(i);break;
                    case "spanId":ex.SpanId=record.IsDBNull(i) ? null : record.GetString(i);break;
                    case "createTime":ex.CreateTime=record.GetDateTime(i);break;
                    case "typeName":ex.TypeName=record.IsDBNull(i) ? null : record.GetString(i);break;
                    case "message":ex.Message=record.IsDBNull(i) ? null : record.GetString(i);break;
                    case "helpLink":ex.HelpLink=record.IsDBNull(i) ? null : record.GetString(i);break;
                    case "data":ex.Data = record.IsDBNull(i) ? null : (Dictionary<string, string>)record[i];break;
                    case "stackTrace":ex.StackTrace=record.IsDBNull(i) ? null : record.GetString(i);break;
                    case "innerException":ex.InnerException=record.IsDBNull(i) ? null : record.GetString(i);break;
                    default:
                        break;
                }
            }

            return ex;
        }

        private string BuildWhereTraceSql(string sql,IEnumerable<string>? traceIds)
        {
            if (traceIds != null && traceIds.Any())
            {
                return $"{sql} WHERE traceId in ({string.Join(",", traceIds.Select(x => $"'{x}'"))})";
            }
            return sql;
        }
        public IEnumerable<ExceptionEntity> ReadExceptions(string sql,IEnumerable<string>? traceIds = null)
        {
            using (var comm = Connection.CreateCommand())
            {
                comm.CommandText = BuildWhereTraceSql(sql, traceIds);
                using (var reader = comm.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return ReadException(reader);
                    }
                }
            }
        }
        public IEnumerable<ExceptionEntity> ReadExceptions(IEnumerable<string>? traceIds = null)
        {
            return ReadExceptions(SelectExceptions, traceIds);
        }
        #endregion
        #region Logs

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private LogEntity ReadEntity(IDataRecord record)
        {
            var log = new LogEntity();
            for (int i = 0; i < record.FieldCount; i++)
            {
                switch (record.GetName(i))
                {
                    case "timestamp":log.Timestamp= record.GetDateTime(i);break;
                    case "logLevel":log.LogLevel = (LogLevel)record.GetInt32(i);break;
                    case "categoryName":log.CategoryName = record.IsDBNull(i) ? null : record.GetString(i);break;
                    case "traceId":log.TraceId = record.IsDBNull(i) ? null : record.GetString(i);break;
                    case "spanId":log.SpanId = record.IsDBNull(i) ? null : record.GetString(i); break;
                    case "attributes":log.Attributes = record.IsDBNull(i) ? null : (Dictionary<string, string>)record[i]; break;
                    case "formattedMessage":log.FormattedMessage = record.IsDBNull(i) ? null : record.GetString(i); break;
                    case "body":log.Body = record.IsDBNull(i) ? null : record.GetString(i); break;
                    default:
                        break;
                }
            }
            return log;
        }
        public IEnumerable<LogEntity> ReadLogs(string sql,IEnumerable<string>? traceIds = null)
        {
            using (var comm = Connection.CreateCommand())
            {
                comm.CommandText = BuildWhereTraceSql(sql, traceIds);
                using (var reader = comm.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return ReadEntity(reader);
                    }
                }
            }
        }
        public IEnumerable<LogEntity> ReadLogs(IEnumerable<string>? traceIds = null)
        {
            return ReadLogs(SelectLogs, traceIds);
        }
        #endregion

        #region Activities

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AcvtityEntity ReadActivity(IDataRecord record)
        {
            var activity = new AcvtityEntity
            {
                Links = new List<ActivityLinkEntity>(),
                Events = new List<ActivityEventEntity>()
            };
            for (int i = 0; i < record.FieldCount; i++)
            {
                switch (record.GetName(i))
                {
                    case "id":activity.Id = record.GetString(i); break;
                    case "status":activity.Status = (ActivityStatusCode)record.GetInt32(i); break;
                    case "statusDescription":activity.StatusDescription = record.IsDBNull(i) ? null : record.GetString(i); break;
                    case "hasRemoteParent":activity.HasRemoteParent = record.GetBoolean(i); break;
                    case "kind":activity.Kind = (ActivityKind)record.GetInt32(i); break;
                    case "operationName":activity.OperationName = record.IsDBNull(i) ? null : record.GetString(i); break;
                    case "displayName":activity.DisplayName = record.IsDBNull(i) ? null : record.GetString(i); break;
                    case "sourceName":activity.SourceName = record.IsDBNull(i) ? null : record.GetString(i); break;
                    case "sourceVersion":activity.SourceVersion = record.IsDBNull(i) ? null : record.GetString(i); break;
                    case "duration":activity.Duration = record.GetDouble(i); break;
                    case "startTimeUtc":activity.StartTimeUtc = record.GetDateTime(i); break;
                    case "parentId":activity.ParentId = record.IsDBNull(i) ? null : record.GetString(i); break;
                    case "rootId":activity.RootId = record.IsDBNull(i) ? null : record.GetString(i); break;
                    case "tags":activity.Tags = record.IsDBNull(i) ? null : (Dictionary<string, string>)record[i]; break;
                    case "baggage":activity.Baggage = record.IsDBNull(i) ? null : (Dictionary<string, string>)record[i]; break;
                    case "traceStateString":activity.TraceStateString = record.IsDBNull(i) ? null : record.GetString(i); break;
                    case "spanId":activity.SpanId = record.IsDBNull(i) ? null : record.GetString(i); break;
                    case "traceId":activity.TraceId = record.IsDBNull(i) ? null : record.GetString(i); break;
                    case "recorded":activity.Recorded = record.GetBoolean(i); break;
                    case "activityTraceFlags":activity.ActivityTraceFlags = (ActivityTraceFlags)record.GetInt32(i); break;
                    case "parentSpanId":activity.ParentSpanId = record.IsDBNull(i) ? null : record.GetString(i); break;
                    case "links":
                        {
                            var links = record.IsDBNull(i) ? null : (List<Dictionary<string, object>>)record[i];
                            if (links != null)
                            {
                                foreach (var item in links)
                                {
                                    activity.Links.Add(ReadLinkEntity(item));
                                }
                            }
                            break;
                        }
                    case "events":
                        {
                            var events = record.IsDBNull(i) ? null : (List<Dictionary<string, object>>)record[i];
                            if (events != null)
                            {
                                foreach (var item in events)
                                {
                                    activity.Events.Add(ReadEventEntity(item));
                                }
                            }
                            break;
                        }
                    case "context":activity.Context= record.IsDBNull(i) ? null : ReadContextEntity((Dictionary<string, object>)record[i]);break;
                    default:break;
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
        public IEnumerable<AcvtityEntity> ReadActivities(string sql,IEnumerable<string>? traceIds = null)
        {
            using (var comm = Connection.CreateCommand())
            {
                comm.CommandText = BuildWhereTraceSql(sql, traceIds);
                using (var reader = comm.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return ReadActivity(reader);
                    }
                }
            }
        }
        public IEnumerable<AcvtityEntity> ReadActivities(IEnumerable<string>? traceIds = null)
        {
            return ReadActivities(SelectActivities, traceIds);
        }
        #endregion
    }
}
