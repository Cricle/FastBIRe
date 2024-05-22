using Diagnostics.Traces.Stores;
using LiteDB;
using System.IO.Compression;

namespace Diagnostics.Traces.LiteDb
{
	public static class LiteDbDayOrLimitHelper
	{
        public static DayOrLimitDatabaseSelector<LiteDatabaseCreatedResult> CreateByPath(string path, string prefx = "trace", bool useGzip = true, Func<ConnectionString, ConnectionString>? connectionStringFun = null)
        {
            var selector= new DayOrLimitDatabaseSelector<LiteDatabaseCreatedResult>(() =>
            {
                var now = DateTime.Now;
                var dir = Path.Combine(path, now.ToString("yyyyMMdd"));
                if (!File.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var fullPath = Path.Combine(dir, $"{prefx}.{now:HHmmss}.litedb");
                var connStr = new ConnectionString
                {
                    Filename = fullPath,
                    Connection = ConnectionType.Shared,
                };
                connStr = connectionStringFun?.Invoke(connStr) ?? connStr;
                return new LiteDatabaseCreatedResult(new LiteDatabase(connStr), fullPath);
            });
            if (useGzip)
            {
                selector.AfterSwitcheds.Add(new GzipDatabaseAfterSwitched<LiteDatabaseCreatedResult>(CompressionLevel.Fastest));
            }
            return selector;
        }

    }
}
