using System.IO.Compression;

namespace Diagnostics.Traces.Stores
{
    public class GzipDatabaseAfterSwitched<TResult> : IUndefinedDatabaseAfterSwitched<TResult>
        where TResult: IDatabaseCreatedResult
    {
        public CompressionLevel Level { get; }

        public GzipDatabaseAfterSwitched(CompressionLevel level)
        {
            Level = level;
        }
        protected virtual object? GetLocker(TResult result)
        {
            return result.Root;
        }

        protected virtual string? GetFilePath(TResult result)
        {
            return result.FilePath;
        }

        protected virtual Task BeginGzipAsync()
        {
            return Task.Delay(1000);
        }

        public void AfterSwitched(TResult result)
        {
            var filePath = GetFilePath(result);
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }
            _ = Task.Factory.StartNew(async () =>
            {
                await BeginGzipAsync();
                var locker= GetLocker(result);
                if (locker != null)
                {
                    Monitor.Enter(locker);
                }
                try
                {
                    if (filePath != null && File.Exists(filePath))
                    {
                        var gzPath = filePath + ".gz";
                        using (var raw = File.OpenRead(filePath))
                        using (var fs = File.Create(gzPath))
                        using (var gz = new GZipStream(fs, Level))
                        {
                            raw.CopyTo(gz);
                        }
                        File.Delete(filePath);
                    }
                }
                finally
                {
                    if (locker!=null)
                    {
                        Monitor.Exit(locker);
                    }
                }
            });
        }
    }
}
