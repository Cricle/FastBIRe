using Diagnostics.Traces.Stores;

namespace Diagnostics.Traces.DuckDB
{
    public class DuckDBResultInitializer : IUndefinedResultInitializer<DuckDBDatabaseCreatedResult>
    {
        #region FULL

        private const string InitFullSqlLogs = @"
CREATE TABLE IF NOT EXISTS ""logs""(
    timestamp DATETIME,
    logLevel TINYINT,
    categoryName VARCHAR,
    traceId VARCHAR,
    spanId VARCHAR,
    attributes MAP(VARCHAR,VARCHAR),
    formattedMessage VARCHAR,
    body VARCHAR
);
";
        private const string InitFullSqlActivities = @"
CREATE TABLE IF NOT EXISTS ""activities""(
    id VARCHAR,
    status TINYINT,
    statusDescription VARCHAR,
    hasRemoteParent BOOLEAN,
    kind TINYINT,
    operationName VARCHAR,
    displayName VARCHAR,
    sourceName VARCHAR,
    sourceVersion VARCHAR,
    duration DOUBLE,
    startTimeUtc DATETIME,
    parentId VARCHAR,
    rootId VARCHAR,
    tags MAP(VARCHAR,VARCHAR),
    events STRUCT(name VARCHAR,timestamp DATETIME, tags MAP(VARCHAR,VARCHAR))[],
    links STRUCT(context STRUCT(traceId VARCHAR,traceState VARCHAR, traceFlags INTEGER, isRemote BOOLEAN, spanId VARCHAR),tags MAP(VARCHAR,VARCHAR))[],
    baggage MAP(VARCHAR,VARCHAR),
    context STRUCT(traceId VARCHAR,traceState VARCHAR, traceFlags INTEGER, isRemote BOOLEAN, spanId VARCHAR),
    traceStateString VARCHAR,
    spanId VARCHAR,
    traceId VARCHAR,
    recorded BOOLEAN,
    activityTraceFlags TINYINT,
    parentSpanId VARCHAR
);
";
        private const string InitFullSqlMetrics = @"
CREATE TABLE IF NOT EXISTS ""metrics""(
    name VARCHAR,
    unit VARCHAR,
    metricType INTEGER,
    temporality INTEGER,
    description VARCHAR,
    meterName VARCHAR,
    meterVersion VARCHAR,
    meterTags MAP(VARCHAR,VARCHAR),
    createTime DATETIME,
    points STRUCT(
        value DOUBLE, 
        sum DOUBLE,
        count INTEGER, 
        min DOUBLE,
        max DOUBLE, 
        histogram STRUCT(
            rangeLeft DOUBLE,
            rangeRight DOUBLE,
            bucketCount INTEGER
        )[],
        zeroBucketCount BIGINT,
        buckets STRUCT(
            lowerBound DOUBLE,
            upperBound DOUBLE,
            bucketCount BIGINT
        )[],
        tags MAP(VARCHAR,VARCHAR), 
        startTime DATETIME,
        endTime DATETIME
    )[]
);
";
        private const string InitFullSqlException = @"
CREATE TABLE IF NOT EXISTS ""exceptions""(
    traceId VARCHAR,
    spanId VARCHAR,
    createTime DATETIME,
    typeName VARCHAR,
    message VARCHAR,
    helpLink VARCHAR,
    hResult INTEGER,
    data MAP(VARCHAR,VARCHAR),
    stackTrace VARCHAR,
    innerException VARCHAR
);
";
        #endregion

        internal readonly static DataField<SaveExceptionModes>[] InitExceptionFields =
        [
            new DataField<SaveExceptionModes>("traceId", "VARCHAR", SaveExceptionModes.TraceId),
            new DataField<SaveExceptionModes>("spanId", "VARCHAR", SaveExceptionModes.SpanId),
            new DataField<SaveExceptionModes>("createTime", "DATETIME", SaveExceptionModes.CreateTime),
            new DataField<SaveExceptionModes>("typeName", "VARCHAR", SaveExceptionModes.TypeName),
            new DataField<SaveExceptionModes>("message", "VARCHAR", SaveExceptionModes.Message),
            new DataField<SaveExceptionModes>("helpLink", "VARCHAR", SaveExceptionModes.HelpLink),
            new DataField<SaveExceptionModes>("data", "MAP(VARCHAR,VARCHAR)", SaveExceptionModes.Data),
            new DataField<SaveExceptionModes>("stackTrace", "VARCHAR", SaveExceptionModes.StackTrace),
            new DataField<SaveExceptionModes>("innerException", "VARCHAR", SaveExceptionModes.InnerException)
        ];

        public static readonly DuckDBResultInitializer Instance = new DuckDBResultInitializer();

        private string createLogSql = InitFullSqlLogs;
        private string createExceptionSql = InitFullSqlException;

        private SaveLogModes saveLogModes = SaveLogModes.All;
        private SaveExceptionModes saveExceptionModes = SaveExceptionModes.All;

        public SaveExceptionModes SaveExceptionModes
        {
            get => saveExceptionModes;
            set
            {
                createExceptionSql = CreateCreateSql("exceptions", value, InitExceptionFields);
                saveExceptionModes = value;
            }
        }

        public SaveLogModes SaveLogModes
        {
            get => saveLogModes;
            set
            {
                createLogSql = CreateCreateSql("logs", value, InitLogFields);
                saveLogModes = value;
            }
        }


        internal readonly static DataField<SaveLogModes>[] InitLogFields =
        [
            new DataField<SaveLogModes>("timestamp", "DATETIME", SaveLogModes.Timestamp),
            new DataField<SaveLogModes>("logLevel", "TINYINT", SaveLogModes.LogLevel),
            new DataField<SaveLogModes>("categoryName", "VARCHAR", SaveLogModes.CategoryName),
            new DataField<SaveLogModes>("traceId", "VARCHAR", SaveLogModes.TraceId),
            new DataField<SaveLogModes>("spanId", "VARCHAR", SaveLogModes.SpanId),
            new DataField<SaveLogModes>("attributes", "MAP(VARCHAR,VARCHAR)", SaveLogModes.Attributes),
            new DataField<SaveLogModes>("formattedMessage", "VARCHAR", SaveLogModes.FormattedMessage),
            new DataField<SaveLogModes>("body", "VARCHAR", SaveLogModes.Body)
        ];


        private static string CreateCreateSql<T>(string tableName, T value, DataField<T>[] fields)
            where T : struct, Enum
        {
            var acceptLogFields = new List<DataField<T>>(fields.Length);
            foreach (var item in fields)
            {
                if (value.HasFlag(item.Mode))
                {
                    acceptLogFields.Add(item);
                }
            }
            if (acceptLogFields.Count == 0)
            {
                throw new InvalidOperationException("The field must at less one");
            }
            return $@"CREATE TABLE IF NOT EXISTS ""{tableName}""(
    {string.Join($",{Environment.NewLine}", acceptLogFields)}
);";
        }

        public void InitializeResult(DuckDBDatabaseCreatedResult result)
        {
            result.Connection.ExecuteNoQuery(createLogSql);
            result.Connection.ExecuteNoQuery(InitFullSqlMetrics);
            result.Connection.ExecuteNoQuery(InitFullSqlActivities);
            result.Connection.ExecuteNoQuery(createExceptionSql);
            AfterInitializeResult(result);
        }
        protected virtual void AfterInitializeResult(DuckDBDatabaseCreatedResult result)
        {
        }
    }
}
