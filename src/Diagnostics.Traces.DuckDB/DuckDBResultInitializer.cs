using Diagnostics.Traces.Stores;

namespace Diagnostics.Traces.DuckDB
{
    public class DuckDBResultInitializer : IUndefinedResultInitializer<DuckDBDatabaseCreatedResult>
    {
        public static readonly DuckDBResultInitializer Instance = new DuckDBResultInitializer();

        private const string InitSqlLogs = @"
CREATE TABLE IF NOT EXISTS ""logs""(
    timestamp DATETIME,
    logLevel INTEGER,
    categoryName VARCHAR,
    traceId VARCHAR,
    spanId VARCHAR,
    attributes MAP(VARCHAR,VARCHAR),
    formattedMessage VARCHAR,
    body VARCHAR
);
";

        private const string InitSqlActivities = @"
CREATE TABLE IF NOT EXISTS ""activities""(
    id VARCHAR,
    status INTEGER,
    statusDescription VARCHAR,
    hasRemoteParent BOOLEAN,
    Kind INTEGER,
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
    activityTraceFlags INTEGER,
    parentSpanId VARCHAR
);
";
        private const string InitSqlMetrics= @"
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
        private const string InitSqlException= @"
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
        public void InitializeResult(DuckDBDatabaseCreatedResult result)
        {
            result.Connection.Execute(InitSqlLogs);
            result.Connection.Execute(InitSqlMetrics);
            result.Connection.Execute(InitSqlActivities);
            result.Connection.Execute(InitSqlException);
            AfterInitializeResult(result);
        }
        protected virtual void AfterInitializeResult(DuckDBDatabaseCreatedResult result)
        {
        }
    }
}
