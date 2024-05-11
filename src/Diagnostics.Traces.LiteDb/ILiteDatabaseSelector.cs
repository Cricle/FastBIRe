using LiteDB;

namespace Diagnostics.Traces.LiteDb
{
    public interface ILiteDatabaseSelector
    {
        ILiteDatabase GetLiteDatabase(TraceTypes type);

        void ReportInserted(TraceTypes type, int count);
    }
    public class DayOrLimitDatabaseSelector : ILiteDatabaseSelector
    {
        public const long DefaultLimitCount = 1_000_000;

        class DatabaseManager
        {
            private long inserted;
            private DateTime lastCreateTime;
            
            private ILiteDatabase? database;

            private readonly object locker = new object();

            public DatabaseManager(long limitCount, TraceTypes type, Func<ILiteDatabase> databaseCreator)
            {
                LimitCount = limitCount;
                Type = type;
                DatabaseCreator = databaseCreator;
            }

            public long LimitCount { get; }

            public TraceTypes Type { get; }

            public Func<ILiteDatabase> DatabaseCreator { get; }

            private void Switch()
            {
                lastCreateTime = DateTime.Now;
                var old = database;
                database= DatabaseCreator();
                old?.Dispose();
            }

            public ILiteDatabase GetDatabase()
            {
                if (database == null || DateTime.Now.Date != lastCreateTime.Date)
                {
                    lock (locker)
                    {
                        if (database == null || DateTime.Now.Date != lastCreateTime.Date)
                        {
                            Switch();
                        }
                    }
                }
                return database!;
            }

            public void ReportInserted(int count)
            {
                lock (locker)
                {
                    inserted += count;
                    if (inserted>LimitCount)
                    {
                        Switch();
                    }
                }
            }
        }

        private readonly DatabaseManager logManager;
        private readonly DatabaseManager activityManager;
        private readonly DatabaseManager metricManager;

        public DayOrLimitDatabaseSelector(Func<TraceTypes, ILiteDatabase> databaseCreator,
            long logLimitCount= DefaultLimitCount,
            long activityLimitCount= DefaultLimitCount,
            long metricLimitCount= DefaultLimitCount)
        {
            logManager = new DatabaseManager(logLimitCount, TraceTypes.Log, () => databaseCreator(TraceTypes.Log));
            activityManager = new DatabaseManager(activityLimitCount, TraceTypes.Activity, () => databaseCreator(TraceTypes.Activity));
            metricManager = new DatabaseManager(metricLimitCount, TraceTypes.Metric, () => databaseCreator(TraceTypes.Metric));
            DatabaseCreator = databaseCreator;
        }

        public Func<TraceTypes, ILiteDatabase> DatabaseCreator { get; }

        public ILiteDatabase GetLiteDatabase(TraceTypes type)
        {
            switch (type)
            {
                case TraceTypes.Log:
                    return logManager.GetDatabase();
                case TraceTypes.Activity:
                    return activityManager.GetDatabase();
                case TraceTypes.Metric:
                    return metricManager.GetDatabase();
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
