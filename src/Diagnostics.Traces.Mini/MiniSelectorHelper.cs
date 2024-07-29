using Diagnostics.Traces.Stores;

namespace Diagnostics.Traces.Mini
{
    public static class MiniSelectorHelper
    {
        public static DayOrLimitDatabaseSelector<MiniDatabaseCreatedResult> CreateDayOrLimitDefault(string path,
            string selectorName,
            Func<string>? fileNameProvider = null,
            Action<MiniDatabaseCreatedResult>? databaseIniter = null,
            int keepFileCount = 50,
            int zstdCompressLevel = 0,
            int fileCapacity = 1024 * 1024 * 16,
            long preLimitCount = DayOrLimitDatabaseSelector<MiniDatabaseCreatedResult>.DefaultLimitCount,
            SaveLogModes saveLogMode = SaveLogModes.Mini,
            SaveExceptionModes saveExceptionModes = SaveExceptionModes.Mini,
            SaveActivityModes saveActivityModes = SaveActivityModes.Mini,
            string? deleteRulesPattern = "*.zstd",
            bool autoCapacity=true)
        {
            var dir = new DirectoryInfo(path);
            if (!dir.Exists)
            {
                dir.Create();
            }
            var selector = new DayOrLimitDatabaseSelector<MiniDatabaseCreatedResult>(() =>
            {
                var fileName = fileNameProvider?.Invoke() ?? $"{DateTime.Now:yyyyMMddHHmmss}.{selectorName}.minitraces";
                var full = Path.Combine(path, fileName);
                var result = new MiniDatabaseCreatedResult(full, fileName, fileCapacity, autoCapacity)
                {
                    SaveLogModes = saveLogMode,
                    SaveExceptionModes = saveExceptionModes,
                    SaveActivityModes = saveActivityModes
                };
                databaseIniter?.Invoke(result);
                return result;
            }, limitCount: preLimitCount);
            selector.AfterSwitcheds.Add(new ZstdDatabaseAfterSwitched<MiniDatabaseCreatedResult>(zstdCompressLevel, new StartWithLastWriteFileDeleteRules(path, keepFileCount, deleteRulesPattern ?? $"*.{selectorName}.zstd")));
            return selector;
        }
    }
}
