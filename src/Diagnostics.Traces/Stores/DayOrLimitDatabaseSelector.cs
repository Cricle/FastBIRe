namespace Diagnostics.Traces.Stores
{
    public class DayOrLimitDatabaseSelector<TResult> : IUndefinedDatabaseSelector<TResult>
        where TResult:IDatabaseCreatedResult
    {
        public const long DefaultLimitCount = 500_000;

        class DatabaseManager
        {
            private long inserted;
            private DateTime lastCreateTime;

            private TResult? result;

            private readonly object locker = new object();

            public DatabaseManager(long limitCount, Func<TResult> databaseCreator)
            {
                LimitCount = limitCount;
                DatabaseCreator = databaseCreator;
            }

            public long LimitCount { get; }

            public Func<TResult> DatabaseCreator { get; }

            public IUndefinedDatabaseAfterSwitched<TResult>? AfterSwitched { get; set; }

            public IUndefinedResultInitializer<TResult>? ResultInitializer { get; set; }

            private void Switch()
            {
                lastCreateTime = DateTime.Now;
                var old = result;
                if (old != null)
                {
                    Monitor.Enter(old.Root);
                }
                try
                {
                    result = DatabaseCreator();
                    old?.Dispose();
                    if (old != null)
                    {
                        AfterSwitched?.AfterSwitched(old);
                    }
                    ResultInitializer?.InitializeResult(result);
                    inserted = 0;
                }
                finally
                {
                    if (old != null)
                    {
                        Monitor.Exit(old.Root);
                    }
                }
            }

            public void UsingDatabaseResult(Action<TResult> @using)
            {
                if (result == null)
                {
                    lock (locker)
                    {
                        if (result == null)
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
                lock (result!.Root)
                {
                    @using(result);
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

        private readonly DatabaseManager manager;

        public DayOrLimitDatabaseSelector(Func<TraceTypes, TResult> databaseCreator,
            long limitCount = DefaultLimitCount,
            IUndefinedDatabaseAfterSwitched<TResult>? afterSwitched = null)
        {
            manager = new DatabaseManager(limitCount, () => databaseCreator(TraceTypes.Log));
            DatabaseCreator = databaseCreator;
            AfterSwitched = afterSwitched;
        }

        private IUndefinedDatabaseAfterSwitched<TResult>? afterSwitched;

        public Func<TraceTypes, TResult> DatabaseCreator { get; }

        public IUndefinedDatabaseAfterSwitched<TResult>? AfterSwitched
        {
            get => afterSwitched;
            set
            {
                manager.AfterSwitched = value;
                afterSwitched = value;
            }
        }

        public void UsingDatabaseResult(TraceTypes type, Action<TResult> @using)
        {
            manager.UsingDatabaseResult(@using);
        }

        public void ReportInserted(TraceTypes type, int count)
        {
            manager.ReportInserted(count);
        }
    }
}
