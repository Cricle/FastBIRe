using Diagnostics.Traces.Stores;

namespace Diagnostics.Traces.DuckDB
{
    public class DuckDBResultInitializer : IUndefinedResultInitializer<DuckDBDatabaseCreatedResult>
    {
        private const string InitSql = @"
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

CREATE TABLE IF NOT EXISTS ""metric""(
    name VARCHAR,
    unit VARCHAR,
    metricType INTEGER,
    temporality INTEGER,
    description VARCHAR,
    meterName VARCHAR,
    meterVersion VARCHAR,
    meterTags MAP(VARCHAR,VARCHAR),
    points STRUCT(
        sum DOUBLE,
        count INTEGER, 
        min DOUBLE,
        max DOUBLE, 
        value DOUBLE, 
        startTime DATETIME,
        endTime DATETIME, 
        tags MAP(VARCHAR,VARCHAR), 
        histogram STRUCT(
            rangeLeft DOUBLE,
            rangeRight DOUBLE,
            bucketCount INTEGER,
            zeroBucketCount BIGINT,
            buckets STRUCT(
                lowerBound DOUBLE,
                upperBound DOUBLE,
                bucketCount BIGINT
            )[]
        )[]
    )[]
);
";

        public void InitializeResult(DuckDBDatabaseCreatedResult result)
        {
            result.Database.Execute(InitSql);
        }
    }
}
