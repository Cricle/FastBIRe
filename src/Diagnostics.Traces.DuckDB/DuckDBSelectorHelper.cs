using Diagnostics.Traces.Stores;
using DuckDB.NET.Native;
using System.IO.Compression;

namespace Diagnostics.Traces.DuckDB
{
    public static class DuckDBSelectorHelper
    {
        public static DayOrLimitDatabaseSelector<DuckDBDatabaseCreatedResult> CreateDayOrLimitDefault(string path,
            Func<string>? fileNameProvider = null,
            Action<DuckDBDatabaseCreatedResult>? databaseIniter = null,
            CompressionLevel gzipLevel = CompressionLevel.Fastest,
            int keepFileCount = 10,
            long preLimitCount = DayOrLimitDatabaseSelector<DuckDBDatabaseCreatedResult>.DefaultLimitCount)
        {
            var selector = new DayOrLimitDatabaseSelector<DuckDBDatabaseCreatedResult>(() =>
            {
                var full = Path.Combine(path, fileNameProvider?.Invoke() ?? $"{DateTime.Now:yyyyMMddHHmmss}.traces");
                var status = NativeMethods.Startup.DuckDBOpen(full, out var database);
                if (status != DuckDBState.Success)
                {
                    throw new TraceDuckDbException($"Fail to open or create database {full}");
                }
                var result = new DuckDBDatabaseCreatedResult(database, full);
                databaseIniter?.Invoke(result);
                return result;
            }, limitCount: preLimitCount);
            selector.AfterSwitcheds.Add(new GzipDatabaseAfterSwitched<DuckDBDatabaseCreatedResult>(gzipLevel, new StartWithLastWriteFileDeleteRules(path, keepFileCount, "*.gz")));
            selector.Initializers.Add(DuckDBResultInitializer.Instance);
            return selector;
        }
    }
}
