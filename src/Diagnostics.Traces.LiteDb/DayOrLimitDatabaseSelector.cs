using LiteDB;
using System.IO;

namespace Diagnostics.Traces.LiteDb
{
    public class DayOrLimitDatabaseSelector : ILiteDatabaseSelector
    {
        public const long DefaultLimitCount = 500_000;

        class DatabaseManager
        {
            private long inserted;
            private DateTime lastCreateTime;

            private LiteDatabaseCreatedResult? database;

            private readonly object locker = new object();

            public DatabaseManager(long limitCount, Func<LiteDatabaseCreatedResult> databaseCreator)
            {
                LimitCount = limitCount;
                DatabaseCreator = databaseCreator;
            }

            public long LimitCount { get; }

            public Func<LiteDatabaseCreatedResult> DatabaseCreator { get; }

            public IDatabaseAfterSwitched? AfterSwitched { get; set; }

            private void Switch()
            {
                lastCreateTime = DateTime.Now;
                var old = database;
                if (old != null)
                {
                    Monitor.Enter(old.Value.Root);
                }
                try
                {
                    database = DatabaseCreator();
                    old?.Dispose();
                    if (old != null)
                    {
                        AfterSwitched?.AfterSwitched(old.Value);
                    }
                    inserted = 0;
                }
                finally
                {
                    if (old != null)
                    {
                        Monitor.Exit(old.Value.Root);
                    }
                }
            }

            public void UsingDatabaseResult(Action<LiteDatabaseCreatedResult> @using)
            {
                if (database==null)
                {
                    lock (locker)
                    {
                        if (database==null)
                        {
                            Switch();
                        }
                    }
                }
                if (DateTime.Now.Date != lastCreateTime.Date)
                {
                    lock (locker)
                    {
                        if (DateTime.Now.Date != lastCreateTime.Date)
                        {
                            Switch();
                        }
                    }
                }
                lock (database!.Value.Root)
                {
                    @using(database.Value);
                }
            }

            public void ReportInserted(int count)
            {
                lock (locker)
                {
                    inserted += count;
                    if (inserted > LimitCount)
                    {
                        Switch();
                    }
                }
            }
        }

        private readonly DatabaseManager logManager;
        private readonly DatabaseManager activityManager;
        private readonly DatabaseManager metricManager;

        public static DayOrLimitDatabaseSelector CreateByPath(string path, string prefx="trace",bool useGzip=true,Func<ConnectionString,ConnectionString>? connectionStringFun=null)
        {
            return new DayOrLimitDatabaseSelector((type) =>
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
            }, afterSwitched: useGzip?GzipDatabaseAfterSwitched.Fastest:null);
        }

        public DayOrLimitDatabaseSelector(Func<TraceTypes, LiteDatabaseCreatedResult> databaseCreator,
            long logLimitCount = DefaultLimitCount,
            long activityLimitCount = DefaultLimitCount,
            long metricLimitCount = DefaultLimitCount,
            IDatabaseAfterSwitched? afterSwitched = null)
        {
            logManager = new DatabaseManager(logLimitCount, () => databaseCreator(TraceTypes.Log));
            activityManager = metricManager = logManager;
            //activityManager = new DatabaseManager(activityLimitCount, () => databaseCreator(TraceTypes.Activity));
            //metricManager = new DatabaseManager(metricLimitCount, () => databaseCreator(TraceTypes.Metric));
            DatabaseCreator = databaseCreator;
            AfterSwitched = afterSwitched;
        }

        private IDatabaseAfterSwitched? afterSwitched;

        public Func<TraceTypes, LiteDatabaseCreatedResult> DatabaseCreator { get; }

        public IDatabaseAfterSwitched? AfterSwitched
        {
            get => afterSwitched;
            set
            {
                logManager.AfterSwitched = value;
                activityManager.AfterSwitched = value;
                metricManager.AfterSwitched = value;
                afterSwitched = value;
            }
        }

        public void UsingDatabaseResult(TraceTypes type,Action<LiteDatabaseCreatedResult> @using)
        {
            switch (type)
            {
                case TraceTypes.Log:
                    logManager.UsingDatabaseResult(@using);
                    break;
                case TraceTypes.Activity:
                    activityManager.UsingDatabaseResult(@using);
                    break;
                case TraceTypes.Metric:
                    metricManager.UsingDatabaseResult(@using);
                    break;
                default:
                    throw new NotSupportedException(type.ToString());
            }
        }

        public void ReportInserted(TraceTypes type, int count)
        {
            switch (type)
            {
                case TraceTypes.Log:
                    logManager.ReportInserted(count);
                    break;
                case TraceTypes.Activity:
                    activityManager.ReportInserted(count);
                    break;
                case TraceTypes.Metric:
                    metricManager.ReportInserted(count);
                    break;
                default:
                    throw new NotSupportedException(type.ToString());
            }
        }
    }
}
