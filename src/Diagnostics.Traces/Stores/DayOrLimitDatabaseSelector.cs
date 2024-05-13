namespace Diagnostics.Traces.Stores
{
    public class DayOrLimitDatabaseSelector<TResult> : IUndefinedDatabaseSelector<TResult>
        where TResult : IDatabaseCreatedResult
    {
        public const long DefaultLimitCount = 500_000;

        class DatabaseManager
        {
            private int switching;
            private long inserted;
            private DateTime lastCreateTime;

            private TResult? result;

            public DatabaseManager(long limitCount, Func<TResult> databaseCreator)
            {
                LimitCount = limitCount;
                DatabaseCreator = databaseCreator;
            }

            public long LimitCount { get; }

            public Func<TResult> DatabaseCreator { get; }

            public IUndefinedDatabaseAfterSwitched<TResult>? AfterSwitched { get; set; }

            public IUndefinedResultInitializer<TResult>? ResultInitializer { get; set; }

            private void GetLocker(int sleepTime = 10)
            {
                while (Interlocked.CompareExchange(ref switching, 1, 0) != 0)
                {
                    Thread.Sleep(sleepTime);
                }
            }

            private void ReleaseLocker()
            {
                Interlocked.Exchange(ref switching, 0);
            }

            private void Switch()
            {
                GetLocker();
                try
                {
                    if (result == null || DateTime.Now.Date != lastCreateTime.Date || inserted > LimitCount)
                    {
                        lastCreateTime = DateTime.Now;
                        var old = result;
                        result = DatabaseCreator();
                        old?.Dispose();
                        if (old != null)
                        {
                            AfterSwitched?.AfterSwitched(old);
                        }
                        ResultInitializer?.InitializeResult(result);
                        inserted = 0;
                    }
                }
                finally
                {
                    ReleaseLocker();
                }
            }
            private void BeforeUsingDatabaseResult()
            {
                if (result == null)
                {
                    Switch();
                }
                if (DateTime.Now.Date != lastCreateTime.Date)
                {
                    Switch();
                }
            }
            public void UsingDatabaseResult<TState>(TState state, Action<TResult, TState> @using)
            {
                BeforeUsingDatabaseResult();
                GetLocker();
                try
                {
                    @using(result!, state);
                }
                finally
                {
                    ReleaseLocker();
                }
            }


            public void UsingDatabaseResult(Action<TResult> @using)
            {
                BeforeUsingDatabaseResult();
                GetLocker();
                try
                {
                    @using(result!);

                }
                finally
                {
                    ReleaseLocker();
                }
            }

            public void ReportInserted(int count)
            {
                if (Interlocked.Add(ref inserted, count) > LimitCount)
                {
                    Switch();
                }
            }
        }

        private readonly DatabaseManager manager;

        public DayOrLimitDatabaseSelector(Func<TraceTypes, TResult> databaseCreator,
            long limitCount = DefaultLimitCount,
            IUndefinedDatabaseAfterSwitched<TResult>? afterSwitched = null,
            IUndefinedResultInitializer<TResult>? initializer = null)
        {
            manager = new DatabaseManager(limitCount, () => databaseCreator(TraceTypes.Log));
            DatabaseCreator = databaseCreator;
            AfterSwitched = afterSwitched;
            Initializer = initializer;
        }

        private IUndefinedDatabaseAfterSwitched<TResult>? afterSwitched;
        private IUndefinedResultInitializer<TResult>? initializer;

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
        public IUndefinedResultInitializer<TResult>? Initializer
        {
            get => initializer;
            set
            {
                manager.ResultInitializer = value;
                initializer = value;
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

        public void UsingDatabaseResult<TState>(TraceTypes type, TState state, Action<TResult, TState> @using)
        {
            manager.UsingDatabaseResult(state, @using);
        }
    }
}
