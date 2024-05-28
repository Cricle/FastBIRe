using System;
using System.Threading;
using System.Threading.Tasks;

namespace Diagnostics.Helpers
{
    public abstract class SampleProvider : ISampleProvider
    {
        private long isStop = 1;
        private readonly Task task;
        private readonly CancellationTokenSource tokenSource;

        public ICounterResult CounterResult { get; }

        public bool IsStop => Interlocked.Read(ref isStop) != 0;

        public Task Task => task;

        public IEventCounterProvider Counter { get; }

        public SampleProvider(ICounterResult counterResult, IEventCounterProvider counter)
        {
            CounterResult = counterResult;
            Counter = counter;
            tokenSource = new CancellationTokenSource();
            task = Task.Factory.StartNew(() => CounterResult.StartSessionAsync(tokenSource.Token)).Unwrap();
            Resume();
        }

        protected abstract void OnPayloadReceived(object? sender, ICounterPayload e);

        public void Dispose()
        {
            tokenSource.Cancel();
            OnDisposed();

        }
        protected virtual void OnDisposed()
        {

        }
        public async Task OnceAsync(Action<RuntimeEventCounter> action, CancellationToken token)
        {
            await OnceAsync(token);
        }

        public void Pause()
        {
            if (Interlocked.CompareExchange(ref isStop, 1, 0) == 0)
            {
                CounterResult.PayloadReceived -= OnPayloadReceived;
            }
        }

        public void Resume()
        {
            if (Interlocked.CompareExchange(ref isStop, 0, 1) == 1)
            {
                CounterResult.PayloadReceived += OnPayloadReceived;
            }
        }

        public async Task OnceAsync(CancellationToken token)
        {
            using (var ts = new CancellationTokenSource())
            {
                token.Register(() => ts.Cancel());
                var startTask = Task.Factory.StartNew(() => CounterResult.StartSessionAsync(ts.Token));
                var onceTask = GetOnceTask(token);
                await onceTask;
                ts.Cancel();
                await startTask;
            }
        }
        protected abstract Task GetOnceTask(CancellationToken token);
    }
}
