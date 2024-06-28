using Diagnostics.Traces.Stores;
using DuckDB.NET.Data;
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
            long preLimitCount = DayOrLimitDatabaseSelector<DuckDBDatabaseCreatedResult>.DefaultLimitCount,
            SaveLogModes saveLogMode = SaveLogModes.Mini)
        {
            var dir = new DirectoryInfo(path);
            if (!dir.Exists)
            {
                dir.Create();
            }
            var selector = new DayOrLimitDatabaseSelector<DuckDBDatabaseCreatedResult>(() =>
            {
                var fileName = fileNameProvider?.Invoke() ?? $"{DateTime.Now:yyyyMMddHHmmss}.ducktraces";
                var full = Path.Combine(path, fileName);
                var database = new DuckDBConnection($"Data source={full}");
                database.Open();
                var result = new DuckDBDatabaseCreatedResult(database, full, fileName)
                {
                     SaveLogModes=saveLogMode
                };
                databaseIniter?.Invoke(result);
                return result;
            }, limitCount: preLimitCount);
            selector.AfterSwitcheds.Add(new GzipDatabaseAfterSwitched<DuckDBDatabaseCreatedResult>(gzipLevel, new StartWithLastWriteFileDeleteRules(path, keepFileCount, "*.gz")));
            selector.Initializers.Add(new DuckDBResultInitializer
            {
                SaveLogModes = saveLogMode
            });
            return selector;
        }
    }
}
