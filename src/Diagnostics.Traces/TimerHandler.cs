namespace Diagnostics.Traces
{
    public abstract class TimerHandlerBase : IDisposable
    {
        private int disposedCount;
        private readonly CancellationTokenSource tokenSource;
        private readonly Task task;

        protected TimerHandlerBase(TimeSpan delayTime)
        {
            if (delayTime.Ticks <= 0)
            {
                throw new ArgumentOutOfRangeException($"The delayTime ticks is less than zero!");
            }
            DelayTime = delayTime;
            tokenSource = new CancellationTokenSource();
            task = Task.Factory.StartNew(HandleCore, this, TaskCreationOptions.LongRunning);
        }

        public bool IsDisposed => Volatile.Read(ref disposedCount) > 0;

        public Task Task => task;

        public TimeSpan DelayTime { get; }

        public event EventHandler<Exception>? ExceptionRaised;

        private async void HandleCore(object? state)
        {
            var handler = (TimerHandlerBase)state!;
            var ts = handler.tokenSource;
            var delay = handler.DelayTime;
            while (!ts.IsCancellationRequested)
            {
                try
                {
                    var tsk = handler.Handle();
                    if (!tsk.IsCompleted)
                    {
                        await tsk;
                    }
                }
                catch (Exception ex)
                {
                    handler.ExceptionRaised?.Invoke(this, ex);
                }
                finally
                {
                    await Task.Delay(delay);
                }
            }
            ts.Dispose();
        }

        protected abstract Task Handle();

        public void Dispose()
        {
            if (Interlocked.Increment(ref disposedCount) == 1)
            {
                tokenSource.Dispose();
            }
        }
    }
    public class AsyncTimerHandler : TimerHandlerBase
    {
        private readonly Func<Task> Action;

        public AsyncTimerHandler(TimeSpan delayTime, Func<Task> action) : base(delayTime)
        {
            Action = action;
        }

        protected override Task Handle()
        {
            return Action();
        }
    }
    public class TimerHandler : TimerHandlerBase
    {
        private readonly Action Action;

        public TimerHandler(TimeSpan delayTime, Action action) : base(delayTime)
        {
            Action = action;
        }

        protected override Task Handle()
        {
            Action();
            return Task.CompletedTask;
        }
    }
}
