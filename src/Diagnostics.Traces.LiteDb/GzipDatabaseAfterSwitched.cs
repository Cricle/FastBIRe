using System.IO.Compression;

namespace Diagnostics.Traces.LiteDb
{
    public class GzipDatabaseAfterSwitched : IDatabaseAfterSwitched
    {
        public static readonly GzipDatabaseAfterSwitched Fastest = new GzipDatabaseAfterSwitched(CompressionLevel.Fastest);
        public static readonly GzipDatabaseAfterSwitched Optimal = new GzipDatabaseAfterSwitched(CompressionLevel.Optimal);

        public CompressionLevel Level { get; }

        public GzipDatabaseAfterSwitched(CompressionLevel level)
        {
            Level = level;
        }

        public void AfterSwitched(LiteDatabaseCreatedResult result)
        {
            _ = Task.Factory.StartNew(async () =>
            {
                await Task.Delay(1000);
                lock (result.Root)
                {
                    if (result.FilePath != null && File.Exists(result.FilePath))
                    {
                        var gzPath = result.FilePath + ".gz";
                        using (var raw = File.OpenRead(result.FilePath))
                        using (var fs = File.Create(gzPath))
                        using (var gz = new GZipStream(fs, Level))
                        {
                            raw.CopyTo(gz);
                        }
                        File.Delete(result.FilePath);
                    }
                }
            });
        }
    }
}
