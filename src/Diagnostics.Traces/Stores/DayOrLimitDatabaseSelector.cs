using System.Runtime.CompilerServices;

namespace Diagnostics.Traces.Stores
{
    public class DayOrLimitDatabaseSelector<TResult> : IUndefinedDatabaseSelector<TResult>
        where TResult : IDatabaseCreatedResult
    {
        public const long DefaultLimitCount = 500_000;

        class DatabaseManager
        {
            private SpinLock locker;
            private long inserted;
            private DateTime lastCreateTime;

            internal TResult? result;

            public DatabaseManager(long limitCount, Func<TResult> databaseCreator, IList<IUndefinedDatabaseAfterSwitched<TResult>> afterSwitcheds, IList<IUndefinedResultInitializer<TResult>> initializers)
            {
                locker = new SpinLock();
                LimitCount = limitCount;
                DatabaseCreator = databaseCreator;
                AfterSwitcheds = afterSwitcheds;
                Initializers = initializers;
            }

            public long LimitCount { get; }

            public Func<TResult> DatabaseCreator { get; }

            public IList<IUndefinedDatabaseAfterSwitched<TResult>> AfterSwitcheds { get; }

            public IList<IUndefinedResultInitializer<TResult>> Initializers { get; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void GetLocker()
            {
                bool b = false;
                locker.Enter(ref b);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void ReleaseLocker()
            {
                locker.Exit();
            }

            public void Switch(bool checkNeeds = true)
            {
                GetLocker();
                try
                {
                    if (!checkNeeds || (result == null || DateTime.Now.Date != lastCreateTime.Date || inserted > LimitCount))
                    {
                        lastCreateTime = DateTime.Now;
                        var old = result;
                        result = DatabaseCreator();
                        old?.Dispose();
                        if (old != null)
                        {
                            for (int i = 0; i < AfterSwitcheds.Count; i++)
                            {
                                AfterSwitcheds[i].AfterSwitched(old);
                            }
                        }
                        for (int i = 0; i < Initializers.Count; i++)
                        {
                            Initializers[i].InitializeResult(result);
                        }
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

        public DayOrLimitDatabaseSelector(Func<TResult> databaseCreator,
            long limitCount = DefaultLimitCount)
        {
            DatabaseCreator = databaseCreator;
            AfterSwitcheds = new List<IUndefinedDatabaseAfterSwitched<TResult>>();
            Initializers = new List<IUndefinedResultInitializer<TResult>>();
            manager = new DatabaseManager(limitCount, () => databaseCreator(), AfterSwitcheds, Initializers);
        }

        public IList<IUndefinedDatabaseAfterSwitched<TResult>> AfterSwitcheds { get; }

        public IList<IUndefinedResultInitializer<TResult>> Initializers { get; }

        public Func<TResult> DatabaseCreator { get; }

        public void UsingDatabaseResult(Action<TResult> @using)
        {
            manager.UsingDatabaseResult(@using);
        }

        public void ReportInserted(int count)
        {
            manager.ReportInserted(count);
        }

        public void UsingDatabaseResult<TState>(TState state, Action<TResult, TState> @using)
        {
            manager.UsingDatabaseResult(state, @using);
        }

        public bool Flush()
        {
            manager.Switch(false);
            return true;
        }

        public TResult? DangerousGetResult()
        {
            return manager.result;
        }
    }
}
