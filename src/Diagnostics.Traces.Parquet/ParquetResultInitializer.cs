using ParquetSharp;

namespace Diagnostics.Traces.Parquet
{
    internal record class DataField<TMode>(string Name,Type Type,TMode Mode) 
    {
        public Column CreateColumn()
        {
            return new Column(Type, Name);
        }
    }
    internal record class DataField<T,TMode>(string Name, TMode Mode) : DataField<TMode>(Name,typeof(T),Mode);
    public class ParquetResultInitializer
    {
        internal readonly static DataField<SaveActivityModes>[] InitActivityFields =
        [
            new DataField<string,SaveActivityModes>("id", SaveActivityModes.Id),
            new DataField<int,SaveActivityModes>("status", SaveActivityModes.Status),
            new DataField<string,SaveActivityModes>("statusDescription", SaveActivityModes.StatusDescription),
            new DataField<bool,SaveActivityModes>("hasRemoteParent",SaveActivityModes.HasRemoteParent),
            new DataField<int,SaveActivityModes>("kind",SaveActivityModes.Kind),
            new DataField<string,SaveActivityModes>("operationName",SaveActivityModes.OperationName),
            new DataField<string,SaveActivityModes>("displayName",SaveActivityModes.DisplayName),
            new DataField<string,SaveActivityModes>("sourceName",SaveActivityModes.SourceName),
            new DataField<string,SaveActivityModes>("sourceVersion",SaveActivityModes.SourceVersion),
            new DataField<double,SaveActivityModes>("duration",SaveActivityModes.Duration),
            new DataField<DateTime,SaveActivityModes>("startTimeUtc",SaveActivityModes.StartTimeUtc),
            new DataField<string,SaveActivityModes>("parentId",SaveActivityModes.ParentId),
            new DataField<string,SaveActivityModes>("rootId",SaveActivityModes.RootId),
            new DataField<string,SaveActivityModes>("tags",SaveActivityModes.Tags),
            new DataField<string,SaveActivityModes>("events",SaveActivityModes.Events),
            new DataField<string,SaveActivityModes>("links",SaveActivityModes.Links),
            new DataField<string,SaveActivityModes>("baggage",SaveActivityModes.Baggage),
            new DataField<string,SaveActivityModes>("context",SaveActivityModes.Context),
            new DataField<string,SaveActivityModes>("traceStateString",SaveActivityModes.TraceStateString),
            new DataField<string,SaveActivityModes>("spanId",SaveActivityModes.SpanId),
            new DataField<string,SaveActivityModes>("traceId",SaveActivityModes.TraceId),
            new DataField<bool,SaveActivityModes>("recorded",SaveActivityModes.Recorded),
            new DataField<int,SaveActivityModes>("activityTraceFlags",SaveActivityModes.ActivityTraceFlags),
            new DataField<string,SaveActivityModes>("parentSpanId",SaveActivityModes.ParentSpanId),
        ]; 
        internal readonly static DataField<SaveLogModes>[] InitLogFields =
        [
            new DataField<DateTime,SaveLogModes>("timestamp",  SaveLogModes.Timestamp),
            new DataField<int,SaveLogModes>("logLevel",  SaveLogModes.LogLevel),
            new DataField<string,SaveLogModes>("categoryName",  SaveLogModes.CategoryName),
            new DataField<string,SaveLogModes>("traceId",  SaveLogModes.TraceId),
            new DataField<string,SaveLogModes>("spanId", SaveLogModes.SpanId),
            new DataField<string,SaveLogModes>("attributes",  SaveLogModes.Attributes),
            new DataField<string,SaveLogModes>("formattedMessage", SaveLogModes.FormattedMessage),
            new DataField<string,SaveLogModes>("body",  SaveLogModes.Body)
        ];
        internal readonly static DataField<SaveExceptionModes>[] InitExceptionFields =
        [
            new DataField<string,SaveExceptionModes>("traceId",  SaveExceptionModes.TraceId),
            new DataField<string,SaveExceptionModes>("spanId", SaveExceptionModes.SpanId),
            new DataField<DateTime,SaveExceptionModes>("createTime", SaveExceptionModes.CreateTime),
            new DataField<string,SaveExceptionModes>("typeName", SaveExceptionModes.TypeName),
            new DataField<string,SaveExceptionModes>("message",  SaveExceptionModes.Message),
            new DataField<string,SaveExceptionModes>("helpLink",SaveExceptionModes.HelpLink),
            new DataField<string,SaveExceptionModes>("data",  SaveExceptionModes.Data),
            new DataField<string,SaveExceptionModes>("stackTrace",  SaveExceptionModes.StackTrace),
            new DataField<string,SaveExceptionModes>("innerException",  SaveExceptionModes.InnerException)
        ];
        public static Column[] GetStringStoreColumns()
        {
            return
            [
                new Column<DateTime>("time"),
                new Column<byte[]>("v"),
            ];
        }
        public static Column[] GetMeterColumns()
        {
            return
            [
                new Column<string>("name"),
                new Column<string>("unit"),
                new Column<int>("metricType"),
                new Column<byte>("temporality"),
                new Column<string>("description"),
                new Column<string>("meterName"),
                new Column<string>("meterVersion"),
                new Column<string>("meterTags"),
                new Column<DateTime>("createTime"),
                new Column<string>("points"),
            ];
        }
        public static Column[] GetActivityColumns(SaveActivityModes mode)
        {
            return CreateColumns(mode, InitActivityFields);
        }
        public static Column[] GetLogColumns(SaveLogModes mode)
        {
            return CreateColumns(mode, InitLogFields);
        }
        public static Column[] GetExceptionColumns(SaveExceptionModes mode)
        {
            return CreateColumns(mode, InitExceptionFields);
        }

        private static Column[] CreateColumns<T>(T value, DataField<T>[] fields)
            where T : struct, Enum
        {
            var acceptLogFields = new List<Column>(fields.Length);
            foreach (var item in fields)
            {
                if (value.HasFlag(item.Mode))
                {
                    acceptLogFields.Add(item.CreateColumn());
                }
            }
            if (acceptLogFields.Count == 0)
            {
                throw new InvalidOperationException("The field must at less one");
            }
            return acceptLogFields.ToArray();
        }
    }
}
